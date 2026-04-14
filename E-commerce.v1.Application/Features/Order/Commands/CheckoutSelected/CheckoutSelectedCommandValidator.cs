using E_commerce.v1.Domain.Enums;
using FluentValidation;

namespace E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;

public class CheckoutSelectedCommandValidator : AbstractValidator<CheckoutSelectedCommand>
{
    public CheckoutSelectedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID không được để trống.");

        RuleFor(x => x.CartItemIds)
            .NotNull()
            .WithMessage("Danh sách sản phẩm cần checkout không được null.")
            .Must(x => x is { Count: > 0 })
            .WithMessage("Bạn phải chọn ít nhất 1 sản phẩm để checkout.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Phương thức thanh toán không hợp lệ.")
            .Must(x => Enum.IsDefined(typeof(PaymentMethod), x))
            .WithMessage("Phương thức thanh toán không hợp lệ.");
    }
}
