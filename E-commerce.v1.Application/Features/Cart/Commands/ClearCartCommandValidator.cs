using FluentValidation;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
