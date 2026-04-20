using E_commerce.v1.Application.DTOs.Category;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CategoryReadRepository : ICategoryReadRepository
{
    private readonly AppDbContext _context;

    public CategoryReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryListDto>> GetHierarchyAsync(CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                Image = c.Image,
                IsActive = c.IsActive,
                Children = c.Children
                    .Where(ch => !ch.IsDeleted)
                    .OrderBy(ch => ch.Name)
                    .Select(ch => new CategoryChildDto
                    {
                        Id = ch.Id,
                        Name = ch.Name,
                        Slug = ch.Slug
                    })
                    .ToList(),
                Products = c.Products
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.Name)
                    .Select(p => new CategoryProductSummaryDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Slug = p.Slug
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}

