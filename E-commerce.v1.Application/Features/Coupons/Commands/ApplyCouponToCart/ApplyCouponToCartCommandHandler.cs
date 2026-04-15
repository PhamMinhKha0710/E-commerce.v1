using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Features.Coupons.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Commands.ApplyCouponToCart;

public class ApplyCouponToCartCommandHandler : IRequestHandler<ApplyCouponToCartCommand, ApplyCouponToCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICouponCalculationService _couponCalculationService;

    public ApplyCouponToCartCommandHandler(
        ICartRepository cartRepository,
        ICouponCalculationService couponCalculationService)
    {
        _cartRepository = cartRepository;
        _couponCalculationService = couponCalculationService;
    }

    public async Task<ApplyCouponToCartResponse> Handle(ApplyCouponToCartCommand request, CancellationToken cancellationToken)
    {
        // Get cart with items
        var cart = await _cartRepository.GetCartWithItemsAsync(request.UserId, cancellationToken);
        if (cart == null || cart.CartItems.Count == 0)
            throw new BadRequestException("Giỏ hàng trống.");

        // Get coupon
        var couponCode = request.CouponCode.Trim().ToUpperInvariant();
        var coupon = await _cartRepository.GetCouponByCodeAsync(couponCode, cancellationToken);
        if (coupon == null)
            throw new NotFoundException("Không tìm thấy mã giảm giá.");

        // Calculate discount
        var subtotal = _couponCalculationService.CalculateCartSubtotal(cart.CartItems);
        var couponDiscount = _couponCalculationService.CalculateDiscount(coupon, subtotal, DateTime.UtcNow);

        // Apply coupon to cart
        await _cartRepository.UpdateCartCouponAsync(cart.Id, coupon.Id, coupon.Code, couponDiscount, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return new ApplyCouponToCartResponse
        {
            CouponCode = coupon.Code,
            CartSubtotal = subtotal,
            CouponDiscount = couponDiscount,
            TotalAfterDiscount = subtotal - couponDiscount
        };
    }
}
