using E_commerce.v1.Domain.Enums;
using FluentValidation;

namespace E_commerce.v1.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.DiscountType)
            .IsInEnum();

        RuleFor(x => x.Value)
            .GreaterThan(0);

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage("Mã phần trăm không được vượt quá 100%.");
    }
}
