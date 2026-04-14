using E_commerce.v1.Application.DTOs.Category;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryListDto>>;
