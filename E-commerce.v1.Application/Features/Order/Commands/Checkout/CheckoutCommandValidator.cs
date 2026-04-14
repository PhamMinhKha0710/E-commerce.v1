using FluentValidation;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID không được để trống.");
    }
}
