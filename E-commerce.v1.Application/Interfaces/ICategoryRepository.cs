using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesHierarchyAsync(CancellationToken cancellationToken);
}
