using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface IProductRepository
{
    Task<Category?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);
}
