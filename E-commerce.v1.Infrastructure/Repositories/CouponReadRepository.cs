using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CouponReadRepository : ICouponReadRepository
{
    private readonly AppDbContext _context;

    public CouponReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CouponDto>> SearchAsync(
        string? code,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 20 : pageSize;

        var query = _context.Coupons.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalized = code.Trim();
            query = query.Where(c => c.Code.Contains(normalized));
        }
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType,
                Value = c.Value,
                MinOrderValue = c.MinOrderValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }

    public Task<CouponDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Coupons
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType,
                Value = c.Value,
                MinOrderValue = c.MinOrderValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
