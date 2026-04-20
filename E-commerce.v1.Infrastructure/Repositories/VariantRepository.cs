using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class VariantRepository : IVariantRepository
{
    private readonly AppDbContext _context;

    public VariantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductVariant> CreateVariantAsync(ProductVariant variant, CancellationToken cancellationToken)
    {
        await _context.ProductVariants.AddAsync(variant, cancellationToken);
        return variant;
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken)
    {
        return await _context.ProductVariants
            .Include(v => v.Options)
            .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
    }

    public Task UpdateVariantAsync(ProductVariant variant, CancellationToken cancellationToken)
    {
        _context.ProductVariants.Update(variant);
        return Task.CompletedTask;
    }

    public async Task<bool> SoftDeleteVariantAsync(Guid variantId, CancellationToken cancellationToken)
    {
        var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
        if (variant != null)
        {
            variant.IsDeleted = true;
            return true;
        }
        return false;
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeVariantId, CancellationToken cancellationToken)
    {
        var normalizedSku = sku.Trim();
        var query = _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.Sku == normalizedSku);

        if (excludeVariantId.HasValue)
            query = query.Where(v => v.Id != excludeVariantId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
