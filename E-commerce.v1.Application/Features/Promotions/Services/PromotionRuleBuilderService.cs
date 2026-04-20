using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Features.Promotions.Services;

public class PromotionRuleBuilderService : IPromotionRuleBuilderService
{
    public Task ApplyScopeAsync(PromotionRule rule, PromotionRuleScopeDto? scope, CancellationToken cancellationToken)
    {
        if (scope?.ProductIds != null)
        {
            foreach (var id in scope.ProductIds.Distinct())
                rule.Products.Add(new PromotionRuleProduct { PromotionRuleId = rule.Id, ProductId = id });
        }

        if (scope?.CategoryIds != null)
        {
            foreach (var id in scope.CategoryIds.Distinct())
                rule.Categories.Add(new PromotionRuleCategory { PromotionRuleId = rule.Id, CategoryId = id });
        }

        return Task.CompletedTask;
    }

    public void ApplyAction(PromotionRule rule, PromotionRuleUpsertDto dto)
    {
        switch (dto.Type)
        {
            case PromotionRuleType.BuyXGetY:
            {
                var a = dto.BuyXGetYAction!;
                rule.BuyXGetYAction = new PromotionBuyXGetYAction
                {
                    PromotionRuleId = rule.Id,
                    BuyProductId = a.BuyProductId,
                    BuyCategoryId = a.BuyCategoryId,
                    BuyQty = a.BuyQty,
                    GetProductId = a.GetProductId,
                    GetCategoryId = a.GetCategoryId,
                    GetQty = a.GetQty,
                    LimitPerOrder = a.LimitPerOrder
                };
                break;
            }
            case PromotionRuleType.PercentageDiscount:
            {
                var a = dto.PercentageAction!;
                rule.PercentageAction = new PromotionPercentageAction
                {
                    PromotionRuleId = rule.Id,
                    Percent = a.Percent,
                    Target = a.Target
                };
                break;
            }
        }
    }
}
