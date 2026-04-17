using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class VariantReadRepository : IVariantReadRepository
{
    private readonly AppDbContext _context;

    public VariantReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductVariantDto>> SearchAsync(
        Guid? productId,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 20 : pageSize;

        var query = _context.ProductVariants
            .AsNoTracking()
            .Where(v => !v.IsDeleted);

        if (productId.HasValue)
            query = query.Where(v => v.ProductId == productId.Value);
        if (isActive.HasValue)
            query = query.Where(v => v.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                Price = v.Price,
                Inventory = v.Inventory,
                IsActive = v.IsActive,
                Options = v.Options.Select(o => new ProductVariantOptionDto
                {
                    Key = o.Key,
                    Value = o.Value
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductVariantDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }

    public Task<ProductVariantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.Id == id && !v.IsDeleted)
            .Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                Price = v.Price,
                Inventory = v.Inventory,
                IsActive = v.IsActive,
                Options = v.Options.Select(o => new ProductVariantOptionDto
                {
                    Key = o.Key,
                    Value = o.Value
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
