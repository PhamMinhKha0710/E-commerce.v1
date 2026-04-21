using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Services;
using E_commerce.v1.Domain.Exceptions;

namespace E_commerce.v1.Application.Features.Coupons.Services;

public class CouponCalculationService : ICouponCalculationService
{
    public decimal CalculateDiscount(Coupon coupon, decimal subtotal, DateTime currentTime)
    {
        return CouponCalculator.ValidateAndCalculateDiscount(coupon, subtotal, currentTime);
    }

    public decimal CalculateCartSubtotal(IEnumerable<CartItem> cartItems)
    {
        return cartItems.Sum(item => (item.Product?.Price ?? 0) * item.Quantity);
    }
}
