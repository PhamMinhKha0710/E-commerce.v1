using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesHierarchyAsync(CancellationToken cancellationToken);

    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Category?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra slug đã tồn tại trong DB (bao gồm cả hàng đã soft-delete) vì unique index ở DB không bỏ qua soft-deleted rows.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken);

    Task AddAsync(Category category, CancellationToken cancellationToken);
}
