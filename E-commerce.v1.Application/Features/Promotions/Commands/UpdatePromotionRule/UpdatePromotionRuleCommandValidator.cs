using E_commerce.v1.Domain.Enums;
using FluentValidation;

namespace E_commerce.v1.Application.Features.Promotions.Commands.UpdatePromotionRule;

public class UpdatePromotionRuleCommandValidator : AbstractValidator<UpdatePromotionRuleCommand>
{
    public UpdatePromotionRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Rule).NotNull();
        RuleFor(x => x.Rule.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Rule.StartDate)
            .LessThan(x => x.Rule.EndDate);

        When(x => x.Rule.Type == PromotionRuleType.PercentageDiscount, () =>
        {
            RuleFor(x => x.Rule.PercentageAction).NotNull();
            RuleFor(x => x.Rule.PercentageAction!.Percent)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
        });

        When(x => x.Rule.Type == PromotionRuleType.BuyXGetY, () =>
        {
            RuleFor(x => x.Rule.BuyXGetYAction).NotNull();
            RuleFor(x => x.Rule.BuyXGetYAction!.BuyQty).GreaterThan(0);
            RuleFor(x => x.Rule.BuyXGetYAction!.GetQty).GreaterThan(0);
        });
    }
}

