using E_commerce.v1.Application.DTOs.Category;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
