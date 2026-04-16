using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _context;

    public CouponRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsByCodeAsync(string normalizedCode, CancellationToken cancellationToken)
    {
        return _context.Coupons.AsNoTracking().AnyAsync(c => c.Code == normalizedCode, cancellationToken);
    }

    public Task AddAsync(Coupon coupon, CancellationToken cancellationToken)
    {
        return _context.Coupons.AddAsync(coupon, cancellationToken).AsTask();
    }
}

