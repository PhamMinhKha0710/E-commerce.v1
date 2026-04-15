using E_commerce.v1.Domain.Enums;
using FluentValidation;

namespace E_commerce.v1.Application.Features.Promotions.Commands.CreatePromotionRule;

public class CreatePromotionRuleCommandValidator : AbstractValidator<CreatePromotionRuleCommand>
{
    public CreatePromotionRuleCommandValidator()
    {
        RuleFor(x => x.Rule).NotNull();
        RuleFor(x => x.Rule.Name)
            .NotEmpty().WithMessage("Name không được để trống.")
            .MaximumLength(255).WithMessage("Name tối đa 255 ký tự.");

        RuleFor(x => x.Rule.StartDate)
            .LessThan(x => x.Rule.EndDate).WithMessage("StartDate phải nhỏ hơn EndDate.");

        RuleFor(x => x.Rule.Type).IsInEnum();

        When(x => x.Rule.Type == PromotionRuleType.PercentageDiscount, () =>
        {
            RuleFor(x => x.Rule.PercentageAction).NotNull().WithMessage("PercentageAction là bắt buộc.");
            RuleFor(x => x.Rule.PercentageAction!.Percent)
                .GreaterThan(0).WithMessage("Percent phải > 0.")
                .LessThanOrEqualTo(100).WithMessage("Percent phải <= 100.");
        });

        When(x => x.Rule.Type == PromotionRuleType.BuyXGetY, () =>
        {
            RuleFor(x => x.Rule.BuyXGetYAction).NotNull().WithMessage("BuyXGetYAction là bắt buộc.");
            RuleFor(x => x.Rule.BuyXGetYAction!.BuyQty).GreaterThan(0);
            RuleFor(x => x.Rule.BuyXGetYAction!.GetQty).GreaterThan(0);
        });
    }
}

