using E_commerce.v1.Application.DTOs.Category;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
            throw new NotFoundException("Danh mục không tồn tại.");

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            Image = category.Image,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId
        };
    }
}
