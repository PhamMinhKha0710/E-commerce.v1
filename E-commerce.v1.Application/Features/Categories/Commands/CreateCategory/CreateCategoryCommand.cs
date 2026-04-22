using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description = null,
    string? Slug = null,
    string? Image = null,
    bool IsActive = true,
    Guid? ParentCategoryId = null) : IRequest<Guid>;
