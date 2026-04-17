using E_commerce.v1.Application.DTOs.Category;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryListDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryListDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var roots = await _categoryRepository.GetAllCategoriesHierarchyAsync(cancellationToken);
        return roots.Select(MapCategory).ToList();
    }

    private static CategoryListDto MapCategory(Category c)
    {
        return new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            Image = c.Image,
            IsActive = c.IsActive,
            Children = c.Children
                .OrderBy(x => x.Name)
                .Select(ch => new CategoryChildDto
                {
                    Id = ch.Id,
                    Name = ch.Name,
                    Slug = ch.Slug
                })
                .ToList(),
            Products = c.Products
                .OrderBy(p => p.Name)
                .Select(p => new CategoryProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug
                })
                .ToList()
        };
    }
}
