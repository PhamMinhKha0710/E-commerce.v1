using System.Data;
using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;

namespace E_commerce.v1.Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ICheckoutRepository _checkoutRepository;
    private readonly IPromotionRuleReadRepository _promotionRuleReadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(
        ICheckoutRepository checkoutRepository,
        IPromotionRuleReadRepository promotionRuleReadRepository,
        IUnitOfWork unitOfWork)
    {
        _checkoutRepository = checkoutRepository;
        _promotionRuleReadRepository = promotionRuleReadRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<CheckoutResponse> CheckoutAsync(Guid userId, PaymentMethod paymentMethod, CancellationToken cancellationToken)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var cart = await _checkoutRepository.GetCartForCheckoutAsync(userId, ct);
            if (cart == null || cart.CartItems.Count == 0)
                throw new BadRequestException("Giỏ hàng trống.");

            var selectedItems = cart.CartItems.OrderBy(ci => ci.ProductId).ToList();
            var order = await BuildOrderAsync(userId, paymentMethod, selectedItems, cart.AppliedCouponId, ct);

            await _checkoutRepository.PersistCheckoutAsync(order, cart, selectedItems, clearWholeCart: true, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return ToCheckoutResponse(order);
        }, IsolationLevel.Serializable, cancellationToken);
    }

    public Task<CheckoutResponse> CheckoutSelectedAsync(
        Guid userId,
        IReadOnlyCollection<Guid> cartItemIds,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken)
    {
        var selectedIds = cartItemIds.Distinct().ToHashSet();
        return _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var cart = await _checkoutRepository.GetCartForCheckoutAsync(userId, ct);
            if (cart == null || cart.CartItems.Count == 0)
                throw new BadRequestException("Giỏ hàng trống.");

            var selectedItems = cart.CartItems
                .Where(ci => selectedIds.Contains(ci.Id))
                .OrderBy(ci => ci.ProductId)
                .ToList();

            if (selectedItems.Count != selectedIds.Count)
                throw new BadRequestException("Một số sản phẩm được chọn không tồn tại trong giỏ hàng.");

            var order = await BuildOrderAsync(userId, paymentMethod, selectedItems, cart.AppliedCouponId, ct);

            await _checkoutRepository.PersistCheckoutAsync(order, cart, selectedItems, clearWholeCart: false, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return ToCheckoutResponse(order);
        }, IsolationLevel.Serializable, cancellationToken);
    }

    private async Task<Order> BuildOrderAsync(
        Guid userId,
        PaymentMethod paymentMethod,
        List<CartItem> selectedItems,
        Guid? appliedCouponId,
        CancellationToken cancellationToken)
    {
        var lockedProducts = await _checkoutRepository.LockProductsForCheckoutAsync(
            selectedItems.Select(i => i.ProductId).Distinct().ToList(),
            cancellationToken);

        foreach (var item in selectedItems)
        {
            if (!lockedProducts.TryGetValue(item.ProductId, out var product))
                throw new NotFoundException("Sản phẩm không tồn tại.");
            if (!product.IsActive)
                throw new BadRequestException($"Sản phẩm '{product.Name}' đã ngừng bán.");
            if (product.Stock < item.Quantity)
                throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");

            product.Stock -= item.Quantity;
        }

        var now = DateTime.UtcNow;
        Guid? usedCouponId = null;

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30],
            PaymentMethod = paymentMethod
        };

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

        var promoItems = order.Items
            .Select(i =>
            {
                var categoryId = lockedProducts[i.ProductId].CategoryId;
                var unitAfterDiscount = Math.Max(0, i.UnitPrice - i.Discount);
                return new PromotionCartItem(i.ProductId, categoryId, unitAfterDiscount, i.Quantity);
            })
            .ToList();

        var bestPromo = await PromotionEngine.CalculateBestAsync(_promotionRuleReadRepository, promoItems, now, cancellationToken);
        order.PromotionRuleId = bestPromo?.RuleId;
        order.PromotionDiscount = bestPromo?.DiscountAmount ?? 0m;
        order.PromotionSummary = bestPromo?.Summary;

        var amountAfterPromotion = Math.Max(0, order.Subtotal - order.PromotionDiscount);

        if (appliedCouponId.HasValue)
        {
            var coupon = await _checkoutRepository.GetCouponByIdAsync(appliedCouponId.Value, cancellationToken);
            if (coupon == null)
                throw new BadRequestException("Mã giảm giá không còn tồn tại.");

            order.CouponDiscount = CouponCalculator.ValidateAndCalculateDiscount(coupon, amountAfterPromotion, now);
            order.CouponCode = coupon.Code;
            coupon.UsedCount += 1;
            usedCouponId = coupon.Id;
        }

        var amountAfterCoupon = amountAfterPromotion - order.CouponDiscount;
        var rank = await _checkoutRepository.GetUserRankAsync(userId, cancellationToken);
        order.RankAtCheckout = rank;
        var rankDiscountPercent = LoyaltyPolicy.GetRankDiscountPercent(rank);
        order.RankDiscount = decimal.Round(amountAfterCoupon * rankDiscountPercent, 2, MidpointRounding.AwayFromZero);
        order.DiscountTotal = order.PromotionDiscount + order.CouponDiscount + order.RankDiscount;
        order.GrandTotal = Math.Max(0, order.Subtotal - order.DiscountTotal);

        if (usedCouponId.HasValue)
        {
            _checkoutRepository.AddCouponRedemption(new CouponRedemption
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

    private static CheckoutResponse ToCheckoutResponse(Order order) => new()
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

