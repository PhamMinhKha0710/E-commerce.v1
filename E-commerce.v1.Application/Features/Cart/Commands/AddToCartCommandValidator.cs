using FluentValidation;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID không được để trống.");

        RuleFor(v => v.ProductId)
            .NotEmpty().WithMessage("Product ID không được để trống.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
    }
}
