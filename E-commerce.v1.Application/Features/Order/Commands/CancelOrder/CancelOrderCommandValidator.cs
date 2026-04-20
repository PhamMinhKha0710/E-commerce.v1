using FluentValidation;

namespace E_commerce.v1.Application.Features.Order.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID không được để trống.");
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID không được để trống.");
    }
}
