using System.Data;
using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CheckoutRepository : ICheckoutRepository
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutRepository(AppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public Task<CheckoutResponse> CheckoutAsync(Guid userId, PaymentMethod paymentMethod, CancellationToken cancellationToken)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId, ct);
            if (cart == null || cart.CartItems.Count == 0) throw new BadRequestException("Giỏ hàng trống.");
            var sortedItems = cart.CartItems.OrderBy(ci => ci.ProductId).ToList();
            var order = await BuildOrderFromItems(userId, paymentMethod, sortedItems, cart.AppliedCouponId, ct);
            _context.Orders.Add(order);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync(ct);
            return ToCheckoutResponse(order);
        }, IsolationLevel.Serializable, cancellationToken);
    }

    public Task<CheckoutResponse> CheckoutSelectedAsync(Guid userId, IReadOnlyCollection<Guid> cartItemIds, PaymentMethod paymentMethod, CancellationToken cancellationToken)
    {
        var selectedIds = cartItemIds.Distinct().ToHashSet();
        return _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId, ct);
            if (cart == null || cart.CartItems.Count == 0) throw new BadRequestException("Giỏ hàng trống.");
            var selectedItems = cart.CartItems.Where(ci => selectedIds.Contains(ci.Id)).OrderBy(ci => ci.ProductId).ToList();
            if (selectedItems.Count != selectedIds.Count) throw new BadRequestException("Một số sản phẩm được chọn không tồn tại trong giỏ hàng.");
            var order = await BuildOrderFromItems(userId, paymentMethod, selectedItems, cart.AppliedCouponId, ct);
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(selectedItems);
            await _context.SaveChangesAsync(ct);
            return ToCheckoutResponse(order);
        }, IsolationLevel.Serializable, cancellationToken);
    }

    private async Task<Order> BuildOrderFromItems(Guid userId, PaymentMethod paymentMethod, List<CartItem> selectedItems, Guid? appliedCouponId, CancellationToken cancellationToken)
    {
        var lockedProducts = new Dictionary<Guid, Product>();
        foreach (var item in selectedItems)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
            if (product == null) throw new NotFoundException("Sản phẩm không tồn tại.");
            if (!product.IsActive) throw new BadRequestException($"Sản phẩm '{product.Name}' đã ngừng bán.");
            if (product.Stock < item.Quantity) throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");
            product.Stock -= item.Quantity;
            lockedProducts[item.ProductId] = product;
        }

        var now = DateTime.UtcNow;
        Guid? usedCouponId = null;
        var order = new Order { UserId = userId, OrderNumber = $"ORD-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30], PaymentMethod = paymentMethod };

        foreach (var item in selectedItems)
        {
            var product = lockedProducts[item.ProductId];
            var unitAfterDiscount = Math.Max(0, product.Price - product.Discount);
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

        order.Subtotal = order.Items.Sum(i => i.LineTotal);
        if (appliedCouponId.HasValue)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == appliedCouponId.Value, cancellationToken);
            if (coupon == null) throw new BadRequestException("Mã giảm giá không còn tồn tại.");
            order.CouponDiscount = CouponCalculator.ValidateAndCalculateDiscount(coupon, order.Subtotal, now);
            order.CouponCode = coupon.Code;
            coupon.UsedCount += 1;
            usedCouponId = coupon.Id;
        }

        var amountAfterCoupon = order.Subtotal - order.CouponDiscount;
        var rank = await _context.Users.Where(u => u.Id == userId).Select(u => u.LoyaltyRank).FirstOrDefaultAsync(cancellationToken);
        order.RankAtCheckout = rank;
        var rankDiscountPercent = LoyaltyPolicy.GetRankDiscountPercent(rank);
        order.RankDiscount = decimal.Round(amountAfterCoupon * rankDiscountPercent, 2, MidpointRounding.AwayFromZero);
        order.DiscountTotal = order.CouponDiscount + order.RankDiscount;
        order.GrandTotal = Math.Max(0, order.Subtotal - order.DiscountTotal);

        if (usedCouponId.HasValue)
        {
            _context.CouponRedemptions.Add(new CouponRedemption
            {
                CouponId = usedCouponId.Value,
                UserId = userId,
                OrderId = order.Id,
                DiscountAmount = order.CouponDiscount,
                RedeemedAt = now
            });
        }

        return order;
    }

    private static CheckoutResponse ToCheckoutResponse(Order order)
    {
        return new CheckoutResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            GrandTotal = order.GrandTotal,
            PaymentMethod = order.PaymentMethod,
            CouponDiscount = order.CouponDiscount,
            RankDiscount = order.RankDiscount,
            CouponCode = order.CouponCode
        };
    }
}
