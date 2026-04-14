using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Interfaces;

public interface IAppDbContext
{

    DbSet<E_commerce.v1.Domain.Entities.User> Users { get; }
    DbSet<E_commerce.v1.Domain.Entities.Role> Roles { get; }
    DbSet<E_commerce.v1.Domain.Entities.UserRole> UserRoles { get; }
    
    DbSet<E_commerce.v1.Domain.Entities.Category> Categories { get; }
    DbSet<E_commerce.v1.Domain.Entities.Product> Products { get; }
    DbSet<E_commerce.v1.Domain.Entities.Cart> Carts { get; }
    DbSet<E_commerce.v1.Domain.Entities.CartItem> CartItems { get; }
    DbSet<E_commerce.v1.Domain.Entities.Order> Orders { get; }
    DbSet<E_commerce.v1.Domain.Entities.OrderItem> OrderItems { get; }
    DbSet<E_commerce.v1.Domain.Entities.Review> Reviews { get; }
    DbSet<E_commerce.v1.Domain.Entities.Coupon> Coupons { get; }
    DbSet<E_commerce.v1.Domain.Entities.CouponRedemption> CouponRedemptions { get; }


    void ClearChangeTracker();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
