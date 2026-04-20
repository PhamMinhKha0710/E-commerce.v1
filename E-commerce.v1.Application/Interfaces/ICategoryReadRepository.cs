using E_commerce.v1.Application.DTOs.Category;

namespace E_commerce.v1.Application.Interfaces;

public interface ICategoryReadRepository
{
    Task<IReadOnlyList<CategoryListDto>> GetHierarchyAsync(CancellationToken cancellationToken);
}

