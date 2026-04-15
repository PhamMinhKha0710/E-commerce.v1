using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Features.Coupons.Services;

/// <summary>
/// Service for coupon discount calculation and validation
/// </summary>
public interface ICouponCalculationService
{
    /// <summary>
    /// Calculate and validate coupon discount
    /// </summary>
    decimal CalculateDiscount(Coupon coupon, decimal subtotal, DateTime currentTime);

    /// <summary>
    /// Calculate cart subtotal from cart items
    /// </summary>
    decimal CalculateCartSubtotal(IEnumerable<CartItem> cartItems);
}
