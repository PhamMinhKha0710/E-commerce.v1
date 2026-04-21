using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    private readonly AppDbContext _context;

    public ProductReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> GetListAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var resolvedPage = page > 0 ? page : 1;
        var resolvedPageSize = pageSize > 0 ? pageSize : 10;
        var skip = (resolvedPage - 1) * resolvedPageSize;

        var query = _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim();
            query = query.Where(p =>
                p.Name.Contains(s) ||
                (p.Description != null && p.Description.Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);

        // Step 1: fetch the page without correlated variant aggregates to avoid
        // expensive per-row subqueries (Count/Min/Max) on the Products listing.
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(resolvedPageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                IsActive = p.IsActive,
                ProductType = p.ProductType,
                Kind = p.Kind,
                DocumentIds = p.DocumentIds,
                CategoryName = p.Category.Name
            })
            .ToListAsync(cancellationToken);

        // Step 2: batch-load variant aggregates for just the current page.
        if (items.Count > 0)
        {
            var productIds = items.Select(i => i.Id).ToList();

            var aggregates = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => !v.IsDeleted && productIds.Contains(v.ProductId))
                .GroupBy(v => v.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count(),
                    Min = (decimal?)g.Min(v => v.Price),
                    Max = (decimal?)g.Max(v => v.Price)
                })
                .ToDictionaryAsync(x => x.ProductId, cancellationToken);

            foreach (var dto in items)
            {
                if (aggregates.TryGetValue(dto.Id, out var agg))
                {
                    dto.VariantCount = agg.Count;
                    dto.MinVariantPrice = agg.Min;
                    dto.MaxVariantPrice = agg.Max;
                }
            }
        }

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = resolvedPage,
            PageSize = resolvedPageSize
        };
    }

    public async Task<ProductDetailDto?> GetDetailAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                NameEn = p.NameEn,
                Description = p.Description,
                DescriptionEn = p.DescriptionEn,
                Sku = p.Sku,
                Slug = p.Slug,
                Price = p.Price,
                Discount = p.Discount,
                Stock = p.Stock,
                Unit = p.Unit,
                UnitEn = p.UnitEn,
                IsActive = p.IsActive,
                ProductType = p.ProductType,
                Kind = p.Kind,
                BuyType = p.BuyType,
                Priority = p.Priority,
                DocumentIds = p.DocumentIds,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                StoreId = p.StoreId,
                CreatedAt = p.CreatedAt,
                VariantCount = p.Variants.Count(v => !v.IsDeleted),
                MinVariantPrice = p.Variants.Where(v => !v.IsDeleted).Select(v => (decimal?)v.Price).Min(),
                MaxVariantPrice = p.Variants.Where(v => !v.IsDeleted).Select(v => (decimal?)v.Price).Max(),
                Variants = p.Variants
                    .Where(v => v.IsActive && !v.IsDeleted)
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        ProductId = v.ProductId,
                        Sku = v.Sku,
                        Price = v.Price,
                        Inventory = v.Inventory,
                        IsActive = v.IsActive,
                        Options = v.Options
                            .Select(o => new ProductVariantOptionDto { Key = o.Key, Value = o.Value })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

