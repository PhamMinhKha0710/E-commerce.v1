using System.Data;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CheckoutRepository : ICheckoutRepository
{
    private readonly AppDbContext _context;

    public CheckoutRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Cart?> GetCartForCheckoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<Dictionary<Guid, Product>> LockProductsForCheckoutAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        // NOTE: This method is intentionally lock-acquiring to avoid oversell under concurrency.
        // SQL Server lock hints ensure writers serialize on the same rows in the current transaction.
        var result = new Dictionary<Guid, Product>();
        foreach (var id in productIds.Distinct())
        {
            var list = await _context.Products
                .FromSqlInterpolated($"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
                .ToListAsync(cancellationToken);
            var product = list.FirstOrDefault();
            if (product != null)
                result[id] = product;
        }
        return result;
    }

    public Task<Coupon?> GetCouponByIdAsync(Guid couponId, CancellationToken cancellationToken)
    {
        return _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
    }

    public Task<LoyaltyRank> GetUserRankAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.LoyaltyRank)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void AddCouponRedemption(CouponRedemption redemption)
    {
        _context.CouponRedemptions.Add(redemption);
    }

    public Task PersistCheckoutAsync(
        Order order,
        Cart cart,
        IReadOnlyCollection<CartItem> checkedOutItems,
        bool clearWholeCart,
        CancellationToken cancellationToken)
    {
        _context.Orders.Add(order);

        if (clearWholeCart)
        {
            _context.Carts.Remove(cart);
        }
        else
        {
            _context.CartItems.RemoveRange(checkedOutItems);
        }

        return Task.CompletedTask;
    }
}
