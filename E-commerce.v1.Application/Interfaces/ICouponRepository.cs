using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface ICouponRepository
{
    Task<bool> ExistsByCodeAsync(string normalizedCode, CancellationToken cancellationToken);

    Task AddAsync(Coupon coupon, CancellationToken cancellationToken);
}

