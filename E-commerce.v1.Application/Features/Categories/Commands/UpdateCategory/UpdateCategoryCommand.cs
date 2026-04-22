using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description = null,
    string? Slug = null,
    string? Image = null,
    bool? IsActive = null,
    Guid? ParentCategoryId = null) : IRequest<bool>;
