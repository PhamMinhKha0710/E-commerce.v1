using System.Data;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    private readonly IAppDbContext _context;

    public CheckoutCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        if (_context is not DbContext dbContext)
            throw new InvalidOperationException("IAppDbContext must be backed by DbContext for transaction support.");

        // Serializable: tránh đọc Stock cũ khi hai checkout cùng lúc; entity load bằng LINQ để SaveChanges ghi Stock đúng.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null || cart.CartItems.Count == 0)
            throw new BadRequestException("Giỏ hàng trống.");

        var sortedItems = cart.CartItems
            .OrderBy(ci => ci.ProductId)
            .ToList();

        var lockedProducts = new Dictionary<Guid, Product>();
        foreach (var item in sortedItems)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

            if (product == null)
                throw new NotFoundException("Sản phẩm không tồn tại.");

            if (!product.IsActive)
                throw new BadRequestException($"Sản phẩm '{product.Name}' đã ngừng bán.");

            if (product.Stock < item.Quantity)
                throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");

            product.Stock -= item.Quantity;

            lockedProducts[item.ProductId] = product;
        }

        var now = DateTime.UtcNow;
        var order = new Domain.Entities.Order
        {
            UserId = request.UserId,
            OrderNumber = $"ORD-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30]
        };

        foreach (var item in sortedItems)
        {
            var product = lockedProducts[item.ProductId];
            var unitAfterDiscount = product.Price - product.Discount;
            if (unitAfterDiscount < 0)
                unitAfterDiscount = 0;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                UnitPrice = product.Price,
                Discount = product.Discount,
                Quantity = item.Quantity,
                LineTotal = unitAfterDiscount * item.Quantity
            });
        }

        order.Subtotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        order.DiscountTotal = order.Items.Sum(i => i.Discount * i.Quantity);
        order.GrandTotal = order.Items.Sum(i => i.LineTotal);

        _context.Orders.Add(order);
        _context.Carts.Remove(cart);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CheckoutResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            GrandTotal = order.GrandTotal
        };
    }
}
