using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Coupons.Commands.ApplyCouponToCart;

public class ApplyCouponToCartCommandHandler : IRequestHandler<ApplyCouponToCartCommand, ApplyCouponToCartResponse>
{
    private readonly IAppDbContext _context;

    public ApplyCouponToCartCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApplyCouponToCartResponse> Handle(ApplyCouponToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null || cart.CartItems.Count == 0)
            throw new BadRequestException("Giỏ hàng trống.");

        var couponCode = request.CouponCode.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode, cancellationToken);
        if (coupon == null)
            throw new NotFoundException("Không tìm thấy mã giảm giá.");

        var subtotal = cart.CartItems.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);
        var couponDiscount = CouponCalculator.ValidateAndCalculateDiscount(coupon, subtotal, DateTime.UtcNow);

        cart.AppliedCouponId = coupon.Id;
        cart.AppliedCouponCode = coupon.Code;
        cart.CouponDiscountPreview = couponDiscount;

        await _context.SaveChangesAsync(cancellationToken);

        return new ApplyCouponToCartResponse
        {
            CouponCode = coupon.Code,
            CartSubtotal = subtotal,
            CouponDiscount = couponDiscount,
            TotalAfterDiscount = subtotal - couponDiscount
        };
    }
}
