namespace E_commerce.v1.Application.Features.Products.Services;

/// <summary>
/// Service for generating product SKU and slug
/// </summary>
public interface IProductSlugService
{
    /// <summary>
    /// Generate unique SKU for product
    /// </summary>
    string GenerateSku(Guid productId);

    /// <summary>
    /// Generate URL-friendly slug from product name
    /// </summary>
    string GenerateSlug(string productName, Guid productId);
}
