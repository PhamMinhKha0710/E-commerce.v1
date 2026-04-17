using E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Payments;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.api.Controllers;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Payments;
using E_commerce.v1.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Tests;

public class DevPayosWebhookTriggerTests
{
    private const string ChecksumKey = "checksum-key";

    [Fact]
    public async Task should_mark_order_paid_when_trigger_called_in_development()
    {
        await using var context = CreateContext();

        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat-payos-trigger" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = "SKU-PAYOS-TRIGGER",
            Slug = "item-payos-trigger",
            Price = 100_000m,
            Stock = 9,
            CategoryId = categoryId
        });

        var order = new Order
        {
            UserId = Guid.NewGuid(),
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..30],
            PaymentMethod = PaymentMethod.EWallet,
            PaymentStatus = PaymentStatus.Pending,
            GrandTotal = 100_000m,
            Subtotal = 100_000m,
            PayosOrderCode = 991_234
        };
        order.Items.Add(new OrderItem
        {
            ProductId = productId,
            ProductNameSnapshot = "P",
            UnitPrice = 100_000m,
            Discount = 0,
            Quantity = 1,
            LineTotal = 100_000m
        });
        context.Orders.Add(order);
        context.StockReservations.Add(new StockReservation
        {
            OrderId = order.Id,
            ProductId = productId,
            Quantity = 1,
            Status = StockReservationStatus.Reserved,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context, environmentName: "Development");
        var request = new DevPayosWebhookGenerateRequest
        {
            OrderCode = order.PayosOrderCode!.Value,
            Amount = 100_000,
            Status = "PAID",
            PaymentLinkId = "pl-trigger-1",
            Id = "evt-trigger-1"
        };

        var result = await controller.Trigger(request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);

        var reloaded = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        Assert.Equal(PaymentStatus.Paid, reloaded.PaymentStatus);
        Assert.NotNull(reloaded.PaidAt);

        var reservation = await context.StockReservations.AsNoTracking().FirstAsync(r => r.OrderId == order.Id);
        Assert.Equal(StockReservationStatus.Converted, reservation.Status);
    }

    [Fact]
    public async Task should_return_not_found_when_trigger_called_outside_development()
    {
        await using var context = CreateContext();
        var controller = CreateController(context, environmentName: "Production");

        var result = await controller.Trigger(
            new DevPayosWebhookGenerateRequest
            {
                OrderCode = 123,
                Amount = 100_000,
                Status = "PAID"
            },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task should_reject_invalid_order_code_in_trigger()
    {
        await using var context = CreateContext();
        var controller = CreateController(context, environmentName: "Development");

        var result = await controller.Trigger(
            new DevPayosWebhookGenerateRequest
            {
                OrderCode = 0,
                Amount = 100_000,
                Status = "PAID"
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static DevPayosWebhookSimulatorController CreateController(AppDbContext context, string environmentName)
    {
        var env = new StubHostEnvironment(environmentName);
        var options = Options.Create(new PayosOptions { ChecksumKey = ChecksumKey });
        var mediator = new DirectWebhookMediator(BuildHandler(context));
        return new DevPayosWebhookSimulatorController(env, options, mediator);
    }

    private static ProcessPayosWebhookCommandHandler BuildHandler(AppDbContext context)
    {
        var verifier = new PayosWebhookVerifier(Options.Create(new PayosOptions { ChecksumKey = ChecksumKey }));
        var orderRepo = new OrderRepository(context);
        var checkoutRepo = new CheckoutRepository(context);
        var paymentRepo = new PaymentRepository(context, checkoutRepo);
        var uow = new EfUnitOfWork(context);
        var ahamoveOptions = Options.Create(new AhamoveOptions
        {
            Pickup = new AhamovePickupOptions
            {
                Lat = 10.77,
                Lng = 106.66,
                Address = "Pickup",
                Name = "Shop",
                Mobile = "0900000000"
            }
        });

        return new ProcessPayosWebhookCommandHandler(
            verifier,
            orderRepo,
            paymentRepo,
            uow,
            new FakeAhamoveClient(),
            ahamoveOptions);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
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
        public Task<IReadOnlyList<AhamoveEstimateResultItem>> EstimateAsync(AhamoveEstimateRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AhamoveEstimateResultItem>>(Array.Empty<AhamoveEstimateResultItem>());

        public Task<AhamoveCreateOrderResponse> CreateOrderAsync(AhamoveCreateOrderRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AhamoveCreateOrderResponse { OrderId = "AH-TEST", Status = "ASSIGNING" });
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string environmentName) => EnvironmentName = environmentName;

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "E-commerce.v1.api.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class DirectWebhookMediator : IMediator
    {
        private readonly ProcessPayosWebhookCommandHandler _handler;

        public DirectWebhookMediator(ProcessPayosWebhookCommandHandler handler)
        {
            _handler = handler;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is ProcessPayosWebhookCommand cmd)
            {
                var unit = await _handler.Handle(cmd, cancellationToken);
                return (TResponse)(object)unit;
            }

            throw new NotSupportedException($"Unsupported request type {request.GetType().FullName}");
        }

        public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (request is ProcessPayosWebhookCommand cmd)
                return await _handler.Handle(cmd, cancellationToken);
            throw new NotSupportedException($"Unsupported request type {request.GetType().FullName}");
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
            => Task.CompletedTask;
    }
}
