using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface ICartRepository
{
    Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Get cart with items and product details
    /// </summary>
    Task<Cart?> GetCartWithItemsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get coupon by code
    /// </summary>
    Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken);

    /// <summary>
    /// Update cart with applied coupon
    /// </summary>
    Task UpdateCartCouponAsync(Guid cartId, Guid couponId, string code, decimal discount, CancellationToken cancellationToken);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
