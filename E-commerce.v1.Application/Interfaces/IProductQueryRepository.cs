using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface IProductQueryRepository
{
    Task<(List<Product> Items, int Total)> GetProductsAsync(
        int skip,
        int take,
        string? categoryId,
        string? search,
        string? sortBy,
        CancellationToken cancellationToken);

    Task<Product?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);

    Task<Product?> UpdateProductAsync(Guid productId, Action<Product> updateAction, CancellationToken cancellationToken);

    Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken);
}
