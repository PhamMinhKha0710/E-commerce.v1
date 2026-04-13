
namespace E_commerce.v1.Application.Interfaces;

public interface IAppDbContext
{
    // CÁC THỰC THỂ CỦA DOMAIN SẼ ĐƯỢC THAY SAU
    // DbSet<User> Users { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
