using FluentValidation;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID không được để trống.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Phương thức thanh toán không hợp lệ.")
            .Must(x => Enum.IsDefined(typeof(PaymentMethod), x))
            .WithMessage("Phương thức thanh toán không hợp lệ.");
    }
}
