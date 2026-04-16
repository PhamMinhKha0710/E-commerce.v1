using System.Text.Json;
using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;
using E_commerce.v1.Application.Features.Shipping.Commands.ProcessAhamoveWebhook;
using E_commerce.v1.Application.Features.Shipping.Queries.GetShippingFee;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Tests;

public class ShippingHandlersTests
{
    [Fact]
    public async Task should_return_estimates_when_ahamove_returns_data()
    {
        var client = new FakeAhamoveClient();
        client.EstimateResult = new List<AhamoveEstimateResultItem>
        {
            new()
            {
                ServiceId = "SGN-BIKE",
                Data = new AhamoveEstimateData
                {
                    Distance = 1,
                    Duration = 100,
                    DistanceFee = 20000,
                    RequestFee = 0,
                    TotalFee = 25000
                }
            }
        };

        var handler = new GetShippingFeeQueryHandler(
            client,
            Options.Create(new AhamoveOptions
            {
                Pickup = new AhamovePickupOptions
                {
                    Lat = 10.77,
                    Lng = 106.66,
                    Address = "Pickup",
                    Name = "Shop",
                    Mobile = "0900000000"
                }
            }));
        var response = await handler.Handle(
            new GetShippingFeeQuery(
                Guid.NewGuid(),
                new GetShippingFeeRequest
                {
                    OrderTime = 0,
                    Path = TwoPointPath(),
                    Services = new List<string> { "SGN-BIKE" }
                }),
            CancellationToken.None);

        Assert.Single(response.Estimates);
        Assert.Equal("SGN-BIKE", response.Estimates[0].ServiceId);
        Assert.Equal(25000, response.Estimates[0].TotalFee);
    }

    [Fact]
    public async Task should_create_shipment_and_persist_ahamove_id()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var orderId = await SeedOrder(context, userId, PaymentMethod.Cod, 150_000m);

        var client = new FakeAhamoveClient();
        client.CreateResult = new AhamoveCreateOrderResponse
        {
            OrderId = "AH-TEST-1",
            Status = "ASSIGNING",
            SharedLink = "https://example.com/track"
        };

        var orderRepo = new OrderRepository(context);
        var uow = new EfUnitOfWork(context);
        var handler = new CreateShipmentCommandHandler(orderRepo, client, uow);

        var result = await handler.Handle(
            new CreateShipmentCommand(
                userId,
                orderId,
                new CreateShipmentRequest
                {
                    OrderTime = 0,
                    Path = TwoPointPath(),
                    ServiceId = "SGN-BIKE",
                    AhamovePaymentMethod = "BALANCE"
                }),
            CancellationToken.None);

        Assert.Equal("AH-TEST-1", result.AhamoveOrderId);

        var reloaded = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId);
        Assert.Equal("AH-TEST-1", reloaded.AhamoveOrderId);
        Assert.Equal("ASSIGNING", reloaded.AhamoveLastStatus);
    }

    [Fact]
    public async Task should_throw_when_shipment_already_exists()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var orderId = await SeedOrder(context, userId, PaymentMethod.Cod, 100m);

        var client = new FakeAhamoveClient
        {
            CreateResult = new AhamoveCreateOrderResponse { OrderId = "AH-DUP-1", Status = "ASSIGNING" }
        };
        var orderRepo = new OrderRepository(context);
        var uow = new EfUnitOfWork(context);
        var handler = new CreateShipmentCommandHandler(orderRepo, client, uow);

        await handler.Handle(
            new CreateShipmentCommand(
                userId,
                orderId,
                new CreateShipmentRequest
                {
                    OrderTime = 0,
                    Path = TwoPointPath(),
                    ServiceId = "SGN-BIKE"
                }),
            CancellationToken.None);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(
            new CreateShipmentCommand(
                userId,
                orderId,
                new CreateShipmentRequest
                {
                    OrderTime = 0,
                    Path = TwoPointPath(),
                    ServiceId = "SGN-BIKE"
                }),
            CancellationToken.None));
    }

    [Fact]
    public async Task should_throw_when_order_not_owned_by_user()
    {
        await using var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var orderId = await SeedOrder(context, ownerId, PaymentMethod.Cod, 100m);

        var handler = new CreateShipmentCommandHandler(
            new OrderRepository(context),
            new FakeAhamoveClient(),
            new EfUnitOfWork(context));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new CreateShipmentCommand(
                otherId,
                orderId,
                new CreateShipmentRequest
                {
                    OrderTime = 0,
                    Path = TwoPointPath(),
                    ServiceId = "SGN-BIKE"
                }),
            CancellationToken.None));
    }

    [Fact]
    public async Task should_update_order_from_webhook_when_api_key_matches()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var orderId = await SeedOrder(context, userId, PaymentMethod.Cod, 100m);

        var order = await context.Orders.FirstAsync(o => o.Id == orderId);
        order.AhamoveOrderId = "WH-1";
        order.AhamoveLastStatus = "ASSIGNING";
        await context.SaveChangesAsync();

        var options = Options.Create(new AhamoveOptions { WebhookApiKey = "secret-key" });
        var handler = new ProcessAhamoveWebhookCommandHandler(
            new OrderRepository(context),
            new EfUnitOfWork(context),
            options);

        var payload = JsonSerializer.SerializeToElement(new
        {
            _id = "WH-1",
            status = "COMPLETED",
            api_key = "secret-key"
        });

        await handler.Handle(new ProcessAhamoveWebhookCommand(payload), CancellationToken.None);

        var reloaded = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId);
        Assert.Equal("COMPLETED", reloaded.AhamoveLastStatus);
        Assert.Equal(OrderStatus.Completed, reloaded.Status);
    }

    [Fact]
    public async Task should_reject_webhook_when_api_key_wrong()
    {
        await using var context = CreateContext();
        var options = Options.Create(new AhamoveOptions { WebhookApiKey = "secret-key" });
        var handler = new ProcessAhamoveWebhookCommandHandler(
            new OrderRepository(context),
            new EfUnitOfWork(context),
            options);

        var payload = JsonSerializer.SerializeToElement(new
        {
            _id = "X",
            status = "COMPLETED",
            api_key = "wrong"
        });

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(
            new ProcessAhamoveWebhookCommand(payload),
            CancellationToken.None));
    }

    private static List<AhamovePathPoint> TwoPointPath() => new()
    {
        new AhamovePathPoint
        {
            Lat = 10.77,
            Lng = 106.66,
            Address = "Pickup A",
            Name = "A",
            Mobile = "0900000001"
        },
        new AhamovePathPoint
        {
            Lat = 10.78,
            Lng = 106.67,
            Address = "Drop B",
            Name = "B",
            Mobile = "0900000002"
        }
    };

    private static async Task<Guid> SeedOrder(AppDbContext context, Guid userId, PaymentMethod payment, decimal grandTotal)
    {
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat-ship" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = "SKU-SHIP",
            Slug = "item-ship",
            Price = grandTotal,
            Stock = 10,
            CategoryId = categoryId
        });

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..30],
            PaymentMethod = payment,
            Subtotal = grandTotal,
            GrandTotal = grandTotal,
            Status = OrderStatus.Pending
        };
        order.Items.Add(new OrderItem
        {
            ProductId = productId,
            ProductNameSnapshot = "P",
            UnitPrice = grandTotal,
            Discount = 0,
            Quantity = 1,
            LineTotal = grandTotal
        });
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order.Id;
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new FakeCurrentUserService());
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => Guid.NewGuid();
        public string? Email => "test@example.com";
        public string GetActor() => "test-user";
    }

    private sealed class FakeAhamoveClient : IAhamoveClient
    {
        public IReadOnlyList<AhamoveEstimateResultItem> EstimateResult { get; set; } = Array.Empty<AhamoveEstimateResultItem>();
        public AhamoveCreateOrderResponse CreateResult { get; set; } = new();

        public Task<IReadOnlyList<AhamoveEstimateResultItem>> EstimateAsync(
            AhamoveEstimateRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EstimateResult);
        }

        public Task<AhamoveCreateOrderResponse> CreateOrderAsync(
            AhamoveCreateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResult);
        }
    }
}
