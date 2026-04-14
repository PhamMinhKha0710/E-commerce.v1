using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Interfaces;

public interface IAppDbContext
{
    // CÁC THỰC THỂ CỦA DOMAIN SẼ ĐƯỢC THAY SAU
    DbSet<E_commerce.v1.Domain.Entities.User> Users { get; }
    
    DbSet<E_commerce.v1.Domain.Entities.Category> Categories { get; }
    DbSet<E_commerce.v1.Domain.Entities.Product> Products { get; }
    DbSet<E_commerce.v1.Domain.Entities.Cart> Carts { get; }
    DbSet<E_commerce.v1.Domain.Entities.CartItem> CartItems { get; }
    DbSet<E_commerce.v1.Domain.Entities.Order> Orders { get; }
    DbSet<E_commerce.v1.Domain.Entities.OrderItem> OrderItems { get; }

    /// <summary>Xóa tracker (dùng khi retry sau vi phạm unique, v.v.).</summary>
    void ClearChangeTracker();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
