using E_commerce.v1.Application.Features.Coupons.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Commands.ApplyCouponToCart;

public class ApplyCouponToCartCommandHandler : IRequestHandler<ApplyCouponToCartCommand, ApplyCouponToCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICouponCalculationService _couponCalculationService;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyCouponToCartCommandHandler(
        ICartRepository cartRepository,
        ICouponCalculationService couponCalculationService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _couponCalculationService = couponCalculationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplyCouponToCartResponse> Handle(ApplyCouponToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(request.UserId, cancellationToken);
        if (cart == null || cart.CartItems.Count == 0)
            throw new BadRequestException("Giỏ hàng trống.");

        var couponCode = request.CouponCode.Trim().ToUpperInvariant();
        var coupon = await _cartRepository.GetCouponByCodeAsync(couponCode, cancellationToken);
        if (coupon == null)
            throw new NotFoundException("Không tìm thấy mã giảm giá.");

        var subtotal = _couponCalculationService.CalculateCartSubtotal(cart.CartItems);
        var couponDiscount = _couponCalculationService.CalculateDiscount(coupon, subtotal, DateTime.UtcNow);

        await _cartRepository.UpdateCartCouponAsync(cart.Id, coupon.Id, coupon.Code, couponDiscount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApplyCouponToCartResponse
        {
            CouponCode = coupon.Code,
            CartSubtotal = subtotal,
            CouponDiscount = couponDiscount,
            TotalAfterDiscount = subtotal - couponDiscount
        };
    }
}
