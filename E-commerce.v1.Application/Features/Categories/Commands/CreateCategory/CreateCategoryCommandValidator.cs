using FluentValidation;

namespace E_commerce.v1.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Slug)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.Image)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrEmpty(x.Image));
    }
}
