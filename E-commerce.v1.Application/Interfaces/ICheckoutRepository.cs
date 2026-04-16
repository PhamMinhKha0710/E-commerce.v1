using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Interfaces;

public interface ICheckoutRepository
{
    Task<Cart?> GetCartForCheckoutAsync(Guid userId, CancellationToken cancellationToken);

    Task<Dictionary<Guid, Product>> LockProductsForCheckoutAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    Task<Coupon?> GetCouponByIdAsync(Guid couponId, CancellationToken cancellationToken);

    Task<LoyaltyRank> GetUserRankAsync(Guid userId, CancellationToken cancellationToken);

    void AddCouponRedemption(CouponRedemption redemption);

    Task PersistCheckoutAsync(
        Order order,
        Cart cart,
        IReadOnlyCollection<CartItem> checkedOutItems,
        bool clearWholeCart,
        CancellationToken cancellationToken);
}
