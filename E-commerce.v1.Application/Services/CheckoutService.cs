using System.Data;
using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ICheckoutRepository _checkoutRepository;
    private readonly IPromotionRuleReadRepository _promotionRuleReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAhamoveClient _ahamoveClient;
    private readonly AhamoveOptions _ahamoveOptions;
    private readonly IOrderRepository _orderRepository;

    public CheckoutService(
        ICheckoutRepository checkoutRepository,
        IPromotionRuleReadRepository promotionRuleReadRepository,
        IUnitOfWork unitOfWork,
        IAhamoveClient ahamoveClient,
        IOptions<AhamoveOptions> ahamoveOptions,
        IOrderRepository orderRepository)
    {
        _checkoutRepository = checkoutRepository;
        _promotionRuleReadRepository = promotionRuleReadRepository;
        _unitOfWork = unitOfWork;
        _ahamoveClient = ahamoveClient;
        _ahamoveOptions = ahamoveOptions.Value;
        _orderRepository = orderRepository;
    }

    public async Task<CheckoutResponse> CheckoutAsync(
        Guid userId,
        PaymentMethod paymentMethod,
        CheckoutShippingInfo? shipping,
        CancellationToken cancellationToken)
    {
        (decimal Fee, string ServiceId)? shippingFeeInfo = shipping != null
            ? await CalculateShippingFeeAsync(shipping, cancellationToken)
            : null;

        var response = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var cart = await _checkoutRepository.GetCartForCheckoutAsync(userId, ct);
            if (cart == null || cart.CartItems.Count == 0)
                throw new BadRequestException("Giỏ hàng trống.");

            var selectedItems = cart.CartItems.OrderBy(ci => ci.ProductId).ToList();
            var order = await BuildOrderAsync(userId, paymentMethod, selectedItems, cart.AppliedCouponId, shipping, shippingFeeInfo, ct);

            await _checkoutRepository.PersistCheckoutAsync(order, cart, selectedItems, clearWholeCart: true, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return ToCheckoutResponse(order);
        }, IsolationLevel.Serializable, cancellationToken);

        if (shipping != null)
        {
            await TryAutoCreateShipmentAsync(response.OrderId, cancellationToken);
            var order = await _orderRepository.GetOrderByIdAsync(response.OrderId, cancellationToken);
            response.AhamoveOrderId = order?.AhamoveOrderId;
        }

        return response;
    }

    public async Task<CheckoutResponse> CheckoutSelectedAsync(
        Guid userId,
        IReadOnlyCollection<Guid> cartItemIds,
        PaymentMethod paymentMethod,
        CheckoutShippingInfo? shipping,
        CancellationToken cancellationToken)
    {
        (decimal Fee, string ServiceId)? shippingFeeInfo = shipping != null
            ? await CalculateShippingFeeAsync(shipping, cancellationToken)
            : null;

        var selectedIds = cartItemIds.Distinct().ToHashSet();
        var response = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
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

            var order = await BuildOrderAsync(userId, paymentMethod, selectedItems, cart.AppliedCouponId, shipping, shippingFeeInfo, ct);

            await _checkoutRepository.PersistCheckoutAsync(order, cart, selectedItems, clearWholeCart: false, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return ToCheckoutResponse(order);
        }, IsolationLevel.Serializable, cancellationToken);

        if (shipping != null)
        {
            await TryAutoCreateShipmentAsync(response.OrderId, cancellationToken);
            var order = await _orderRepository.GetOrderByIdAsync(response.OrderId, cancellationToken);
            response.AhamoveOrderId = order?.AhamoveOrderId;
        }

        return response;
    }

    private async Task<Order> BuildOrderAsync(
        Guid userId,
        PaymentMethod paymentMethod,
        List<CartItem> selectedItems,
        Guid? appliedCouponId,
        CheckoutShippingInfo? shipping,
        (decimal Fee, string ServiceId)? shippingFeeInfo,
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

            if (paymentMethod == PaymentMethod.Cod)
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

        if (shipping != null && shippingFeeInfo.HasValue)
        {
            order.ShippingServiceId = shippingFeeInfo.Value.ServiceId;
            order.ShippingFee = shippingFeeInfo.Value.Fee;
            order.ShippingReceiverName = shipping.Dropoff.Name;
            order.ShippingReceiverPhone = shipping.Dropoff.Mobile;
            order.ShippingAddressLine = shipping.Dropoff.Address;
            order.ShippingLat = shipping.Dropoff.Lat;
            order.ShippingLng = shipping.Dropoff.Lng;
            order.ShippingNote = shipping.Note;
        }

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
        CouponCode = order.CouponCode,
        ShippingFee = order.ShippingFee,
        ShippingServiceId = order.ShippingServiceId,
        AhamoveOrderId = order.AhamoveOrderId
    };

    private async Task<(decimal Fee, string ServiceId)> CalculateShippingFeeAsync(
        CheckoutShippingInfo shipping,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shipping.ServiceId))
            throw new BadRequestException("Shipping.ServiceId là bắt buộc.");
        if (shipping.Dropoff == null)
            throw new BadRequestException("Shipping.Dropoff là bắt buộc.");
        if (string.IsNullOrWhiteSpace(shipping.Dropoff.Address))
            throw new BadRequestException("Địa chỉ giao hàng không hợp lệ.");

        var pickup = _ahamoveOptions.Pickup;
        var path = new List<AhamovePathPoint>
        {
            new()
            {
                Lat = pickup.Lat,
                Lng = pickup.Lng,
                Address = pickup.Address,
                Name = pickup.Name,
                Mobile = pickup.Mobile
            },
            shipping.Dropoff
        };

        var serviceId = shipping.ServiceId.Trim();
        var results = await _ahamoveClient.EstimateAsync(new AhamoveEstimateRequest
        {
            OrderTime = 0,
            Path = path,
            Services = new List<AhamoveEstimateService> { new() { Id = serviceId } },
            Items = shipping.Items,
            RouteOptimized = shipping.RouteOptimized
        }, cancellationToken);

        var estimate = results.FirstOrDefault(r => r.ServiceId == serviceId);
        var fee = estimate?.Data?.TotalFee ?? 0m;
        if (fee <= 0)
            throw new BadRequestException("Không tính được phí vận chuyển. Vui lòng kiểm tra lại địa chỉ/dịch vụ.");

        return (fee, serviceId);
    }

    private async Task TryAutoCreateShipmentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
            if (order == null)
                return;
            if (!string.IsNullOrWhiteSpace(order.AhamoveOrderId))
                return;
            if (string.IsNullOrWhiteSpace(order.ShippingServiceId) ||
                order.ShippingLat == null || order.ShippingLng == null ||
                string.IsNullOrWhiteSpace(order.ShippingAddressLine))
                return;

            var pickup = _ahamoveOptions.Pickup;
            var path = new List<AhamovePathPoint>
            {
                new()
                {
                    Lat = pickup.Lat,
                    Lng = pickup.Lng,
                    Address = pickup.Address,
                    Name = pickup.Name,
                    Mobile = pickup.Mobile
                },
                new()
                {
                    Lat = order.ShippingLat.Value,
                    Lng = order.ShippingLng.Value,
                    Address = order.ShippingAddressLine,
                    Name = order.ShippingReceiverName,
                    Mobile = order.ShippingReceiverPhone,
                    Cod = order.PaymentMethod == PaymentMethod.Cod ? order.GrandTotal : null
                }
            };

            var result = await _ahamoveClient.CreateOrderAsync(new AhamoveCreateOrderRequest
            {
                OrderTime = 0,
                Path = path,
                ServiceId = order.ShippingServiceId,
                PaymentMethod = "BALANCE",
                Remarks = string.IsNullOrWhiteSpace(order.ShippingNote)
                    ? $"Order {order.OrderNumber}"
                    : $"{order.ShippingNote} | {order.OrderNumber}"
            }, cancellationToken);

            order.AhamoveOrderId = result.OrderId;
            order.AhamoveLastStatus = result.Status;

            var mapped = AhamoveStatusMapper.TryMapToOrderStatus(result.Status);
            if (mapped.HasValue)
                order.Status = mapped.Value;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // do not fail checkout due to shipping provider issues
        }
    }
}

