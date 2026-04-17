using E_commerce.v1.Application.Features.Payment.Commands.CreatePayosPaymentLink;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Payments;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Tests;

public class CreatePayosPaymentLinkOrderCodeTests
{
    private const long PayosMaxSafeOrderCode = 9_007_199_254_740_991L;

    [Fact]
    public async Task should_generate_order_code_within_payos_max_safe_integer()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat-payos-ordercode" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = "SKU-PAYOS-ORDERCODE",
            Slug = "item-payos-ordercode",
            Price = 100_000m,
            Stock = 10,
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
            PayosOrderCode = null
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
        await context.SaveChangesAsync();

        var checkoutRepo = new CheckoutRepository(context);
        var paymentRepo = new PaymentRepository(context, checkoutRepo);
        var orderRepo = new OrderRepository(context);
        var uow = new EfUnitOfWork(context);

        var fakePayosClient = new CapturingPayosClient();
        var handler = new CreatePayosPaymentLinkCommandHandler(
            orderRepo,
            paymentRepo,
            uow,
            fakePayosClient,
            Options.Create(new PayosOptions
            {
                ReservationTtlMinutes = 15,
                ReturnUrl = "https://example.com/return",
                CancelUrl = "https://example.com/cancel"
            }));

        var result = await handler.Handle(
            new CreatePayosPaymentLinkCommand(userId, order.Id, order.GrandTotal, "test"),
            CancellationToken.None);

        Assert.True(fakePayosClient.LastRequestOrderCode.HasValue);
        Assert.True(fakePayosClient.LastRequestOrderCode.Value > 0);
        Assert.True(fakePayosClient.LastRequestOrderCode.Value <= PayosMaxSafeOrderCode);
        Assert.Equal(fakePayosClient.LastRequestOrderCode.Value, result.OrderCode);
    }

    private sealed class CapturingPayosClient : IPayosClient
    {
        public long? LastRequestOrderCode { get; private set; }

        public Task<PayosCreatePaymentLinkResult> CreatePaymentLinkAsync(
            PayosCreatePaymentLinkRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequestOrderCode = request.OrderCode;
            return Task.FromResult(new PayosCreatePaymentLinkResult
            {
                CheckoutUrl = "https://payos.test/checkout",
                PaymentLinkId = "pl-test",
                OrderCode = request.OrderCode
            });
        }
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
}

