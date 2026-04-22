using System.Text.Json;
using E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Shipping;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.ExternalServices.Payments;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PayOS.Crypto;

namespace E_commerce.v1.Application.Tests;

public class PayosWebhookTests
{
    [Fact]
    public async Task should_verify_and_parse_webhook_when_signature_valid()
    {
        var verifier = new PayosWebhookVerifier(Options.Create(new Application.Common.Payments.PayosOptions
        {
            ChecksumKey = "checksum-key"
        }));

        var raw = CreateSignedWebhookBody(
            orderCode: 123,
            amount: 1000,
            status: "PAID",
            checksumKey: "checksum-key",
            paymentLinkId: "pl-1",
            id: "evt-1");

        var evt = await verifier.VerifyAndParseAsync(raw);

        Assert.Equal(123, evt.OrderCode);
        Assert.Equal(1000, evt.Amount);
        Assert.Equal("PAID", evt.Status);
        Assert.Equal("pl-1", evt.PaymentLinkId);
        Assert.Equal("evt-1", evt.RawDataId);
    }

    [Fact]
    public async Task should_mark_order_paid_and_convert_reservation_when_webhook_paid()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat-payos" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = "SKU-PAYOS",
            Slug = "item-payos",
            Price = 100_000m,
            Stock = 9,
            CategoryId = categoryId
        });

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..30],
            PaymentMethod = PaymentMethod.EWallet,
            PaymentStatus = PaymentStatus.Pending,
            GrandTotal = 100_000m,
            Subtotal = 100_000m,
            PayosOrderCode = 999001
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

        var checksumKey = "checksum-key";
        var raw = CreateSignedWebhookBody(order.PayosOrderCode.Value, 100_000, "PAID", checksumKey, "pl-1", "evt-1");

        var handler = CreateHandler(context, checksumKey);
        await handler.Handle(new ProcessPayosWebhookCommand(raw), CancellationToken.None);

        var reloaded = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        Assert.Equal(PaymentStatus.Paid, reloaded.PaymentStatus);
        Assert.NotNull(reloaded.PaidAt);

        var reservation = await context.StockReservations.AsNoTracking().FirstAsync(r => r.OrderId == order.Id);
        Assert.Equal(StockReservationStatus.Converted, reservation.Status);

        var product = await context.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
        Assert.Equal(9, product.Stock);

        Assert.Single(await context.PaymentTransactions.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task should_release_reservation_and_restore_stock_when_webhook_failed()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat-payos2" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = "SKU-PAYOS2",
            Slug = "item-payos2",
            Price = 100_000m,
            Stock = 9,
            CategoryId = categoryId
        });

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..30],
            PaymentMethod = PaymentMethod.EWallet,
            PaymentStatus = PaymentStatus.Pending,
            GrandTotal = 100_000m,
            Subtotal = 100_000m,
            PayosOrderCode = 999002
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

        var checksumKey = "checksum-key";
        var raw = CreateSignedWebhookBody(order.PayosOrderCode.Value, 100_000, "FAILED", checksumKey, "pl-2", "evt-2");

        var handler = CreateHandler(context, checksumKey);
        await handler.Handle(new ProcessPayosWebhookCommand(raw), CancellationToken.None);

        var reloaded = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        Assert.Equal(PaymentStatus.Failed, reloaded.PaymentStatus);

        var reservation = await context.StockReservations.AsNoTracking().FirstAsync(r => r.OrderId == order.Id);
        Assert.Equal(StockReservationStatus.Released, reservation.Status);
        Assert.NotNull(reservation.ReleasedAt);

        var product = await context.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public async Task should_be_idempotent_when_webhook_received_twice()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat-payos3" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = "SKU-PAYOS3",
            Slug = "item-payos3",
            Price = 100_000m,
            Stock = 9,
            CategoryId = categoryId
        });

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..30],
            PaymentMethod = PaymentMethod.EWallet,
            PaymentStatus = PaymentStatus.Pending,
            GrandTotal = 100_000m,
            Subtotal = 100_000m,
            PayosOrderCode = 999003
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

        var checksumKey = "checksum-key";
        var raw = CreateSignedWebhookBody(order.PayosOrderCode.Value, 100_000, "PAID", checksumKey, "pl-3", "evt-3");

        var handler = CreateHandler(context, checksumKey);
        await handler.Handle(new ProcessPayosWebhookCommand(raw), CancellationToken.None);
        await handler.Handle(new ProcessPayosWebhookCommand(raw), CancellationToken.None);

        Assert.Single(await context.PaymentTransactions.AsNoTracking().ToListAsync());
    }

    private static ProcessPayosWebhookCommandHandler CreateHandler(AppDbContext context, string checksumKey)
    {
        var verifier = new PayosWebhookVerifier(Options.Create(new Application.Common.Payments.PayosOptions
        {
            ChecksumKey = checksumKey
        }));

        var orderRepo = new OrderRepository(context);
        var checkoutRepo = new CheckoutRepository(context);
        var paymentRepo = new PaymentRepository(context, checkoutRepo);
        var uow = new EfUnitOfWork(context);

        var ahamoveClient = new FakeAhamoveClient();
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
            ahamoveClient,
            ahamoveOptions,
            NullLogger<ProcessPayosWebhookCommandHandler>.Instance);
    }

    private static string CreateSignedWebhookBody(long orderCode, int amount, string status, string checksumKey, string paymentLinkId, string id)
    {
        var data = JsonSerializer.SerializeToElement(new
        {
            orderCode,
            amount,
            status,
            paymentLinkId,
            id
        });

        var signature = new CryptoProvider().CreateSignatureFromObject(data, checksumKey);
        return JsonSerializer.Serialize(new { data, signature });
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

        public Task<AhamoveOrderDetailsResponse> GetOrderDetailsAsync(string orderId, CancellationToken cancellationToken = default)
            => Task.FromResult(new AhamoveOrderDetailsResponse { OrderId = orderId, Status = "ASSIGNING" });
    }
}

