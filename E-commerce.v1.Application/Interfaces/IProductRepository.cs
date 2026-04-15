using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

/// <summary>
/// Repository for Product entity operations
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Get category by ID
    /// </summary>
    Task<Category?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Add product to database
    /// </summary>
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
