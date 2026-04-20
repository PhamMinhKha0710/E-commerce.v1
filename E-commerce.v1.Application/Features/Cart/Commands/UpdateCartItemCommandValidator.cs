using FluentValidation;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID không được để trống.");

        RuleFor(v => v.CartItemId)
            .NotEmpty().WithMessage("Cart Item ID không được để trống.");

        RuleFor(v => v.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng phải lớn hơn hoặc bằng 0.");
    }
}
