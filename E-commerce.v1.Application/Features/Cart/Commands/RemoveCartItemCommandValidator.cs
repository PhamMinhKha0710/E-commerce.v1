using FluentValidation;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CartItemId).NotEmpty();
    }
}
