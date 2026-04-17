using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface IVariantRepository
{
    Task<ProductVariant> CreateVariantAsync(ProductVariant variant, CancellationToken cancellationToken);

    Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken);

    Task UpdateVariantAsync(ProductVariant variant, CancellationToken cancellationToken);

    Task<bool> SoftDeleteVariantAsync(Guid variantId, CancellationToken cancellationToken);

    Task<bool> SkuExistsAsync(string sku, Guid? excludeVariantId, CancellationToken cancellationToken);
}
