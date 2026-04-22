using System.Data;
using E_commerce.v1.Application.Features.Order.Commands.CancelOrder;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace E_commerce.v1.Application.Tests;

public class CancelOrderCommandHandlerTests
{
    [Fact]
    public async Task should_cancel_order_and_release_reserved_stock_when_pending()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var (orderId, productId) = await SeedOrderWithReservation(context, userId, OrderStatus.Pending, PaymentStatus.Pending);

        var handler = BuildHandler(context);

        await handler.Handle(new CancelOrderCommand(userId, orderId), CancellationToken.None);

        var order = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId);
        var reservation = await context.StockReservations.AsNoTracking().FirstAsync(r => r.OrderId == orderId);
        var product = await context.Products.AsNoTracking().FirstAsync(p => p.Id == productId);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(StockReservationStatus.Released, reservation.Status);
        Assert.Equal(12, product.Stock); // restored +2 from released reservation
    }

    [Fact]
    public async Task should_be_idempotent_when_order_already_cancelled()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var (orderId, _) = await SeedOrderWithReservation(context, userId, OrderStatus.Cancelled, PaymentStatus.Pending);

        var handler = BuildHandler(context);

        await handler.Handle(new CancelOrderCommand(userId, orderId), CancellationToken.None);

        var order = await context.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task should_throw_BadRequest_when_order_already_delivering()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var (orderId, _) = await SeedOrderWithReservation(context, userId, OrderStatus.Delivering, PaymentStatus.Pending);

        var handler = BuildHandler(context);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new CancelOrderCommand(userId, orderId), CancellationToken.None));
    }

    [Fact]
    public async Task should_throw_BadRequest_when_payment_already_paid()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var (orderId, _) = await SeedOrderWithReservation(context, userId, OrderStatus.Confirmed, PaymentStatus.Paid);

        var handler = BuildHandler(context);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new CancelOrderCommand(userId, orderId), CancellationToken.None));
    }

    [Fact]
    public async Task should_throw_NotFound_when_user_is_not_owner()
    {
        await using var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var intruderId = Guid.NewGuid();
        var (orderId, _) = await SeedOrderWithReservation(context, ownerId, OrderStatus.Pending, PaymentStatus.Pending);

        var handler = BuildHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CancelOrderCommand(intruderId, orderId), CancellationToken.None));
    }

    [Fact]
    public async Task should_throw_NotFound_when_order_missing()
    {
        await using var context = CreateContext();
        var handler = BuildHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CancelOrderCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    private static CancelOrderCommandHandler BuildHandler(AppDbContext context)
    {
        var orderRepo = new OrderRepository(context);
        var checkoutRepo = new CheckoutRepository(context);
        var paymentRepo = new PaymentRepository(context, checkoutRepo);
        var uow = new NoopUnitOfWork(context);
        return new CancelOrderCommandHandler(orderRepo, paymentRepo, uow, NullLogger<CancelOrderCommandHandler>.Instance);
    }

    private static async Task<(Guid OrderId, Guid ProductId)> SeedOrderWithReservation(
        AppDbContext context,
        Guid userId,
        OrderStatus status,
        PaymentStatus paymentStatus)
    {
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = $"cat-{Guid.NewGuid():N}" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Item",
            Sku = $"SKU-{Guid.NewGuid():N}",
            Slug = $"item-{Guid.NewGuid():N}",
            Price = 100m,
            Stock = 10,
            CategoryId = categoryId
        });

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..30],
            PaymentMethod = PaymentMethod.Cod,
            PaymentStatus = paymentStatus,
            Subtotal = 200m,
            GrandTotal = 200m,
            Status = status
        };
        order.Items.Add(new OrderItem
        {
            ProductId = productId,
            ProductNameSnapshot = "P",
            UnitPrice = 100m,
            Discount = 0,
            Quantity = 2,
            LineTotal = 200m
        });
        context.Orders.Add(order);

        context.StockReservations.Add(new StockReservation
        {
            OrderId = order.Id,
            ProductId = productId,
            Quantity = 2,
            Status = StockReservationStatus.Reserved,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });

        await context.SaveChangesAsync();
        return (order.Id, productId);
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

    /// <summary>InMemory provider does not support transactions; bypass while preserving SaveChanges semantics.</summary>
    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public NoopUnitOfWork(AppDbContext context) => _context = context;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);

        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken) => operation(cancellationToken);

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken) => await operation(cancellationToken);
    }
}
