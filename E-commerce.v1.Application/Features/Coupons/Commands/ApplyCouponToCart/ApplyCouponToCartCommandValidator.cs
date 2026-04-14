using FluentValidation;

namespace E_commerce.v1.Application.Features.Coupons.Commands.ApplyCouponToCart;

public class ApplyCouponToCartCommandValidator : AbstractValidator<ApplyCouponToCartCommand>
{
    public ApplyCouponToCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.CouponCode)
            .NotEmpty()
            .MaximumLength(64);
    }
}
