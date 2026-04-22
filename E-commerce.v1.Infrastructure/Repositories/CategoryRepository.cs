using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategoriesHierarchyAsync(CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null)
            .Include(c => c.Children)
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters: unique index ở DB không phân biệt soft-deleted rows.
        var query = _context.Categories
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(c => c.Slug == slug);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }
}
