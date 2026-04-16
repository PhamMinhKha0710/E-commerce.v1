using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface ICartRepository
{
    Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken);

    Task<Cart?> GetCartWithItemsAsync(Guid userId, CancellationToken cancellationToken);

    Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<CartItem?> GetCartItemWithDetailsAsync(Guid cartItemId, CancellationToken cancellationToken);

    Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken);

    Task UpdateCartCouponAsync(Guid cartId, Guid couponId, string code, decimal discount, CancellationToken cancellationToken);

    Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken);

    Task<CartItem?> UpdateCartItemQuantityAsync(Guid cartItemId, int quantity, CancellationToken cancellationToken);

    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken);

    Task<Cart?> SyncCartItemsAsync(Guid userId, List<CartItem> items, CancellationToken cancellationToken);

    Task<Product?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
}
