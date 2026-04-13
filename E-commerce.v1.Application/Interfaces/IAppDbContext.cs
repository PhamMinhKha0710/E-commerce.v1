using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Interfaces;

public interface IAppDbContext
{
    // CÁC THỰC THỂ CỦA DOMAIN SẼ ĐƯỢC THAY SAU
    DbSet<E_commerce.v1.Domain.Entities.User> Users { get; }
    
    DbSet<E_commerce.v1.Domain.Entities.Category> Categories { get; }
    DbSet<E_commerce.v1.Domain.Entities.Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
