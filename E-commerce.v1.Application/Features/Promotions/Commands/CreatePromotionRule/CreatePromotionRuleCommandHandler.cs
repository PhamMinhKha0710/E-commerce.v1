using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Promotions.Commands.CreatePromotionRule;

public class CreatePromotionRuleCommandHandler : IRequestHandler<CreatePromotionRuleCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreatePromotionRuleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Rule;

        var rule = new PromotionRule
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Type = dto.Type,
            Priority = dto.Priority,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };

        ApplyScope(rule, dto.Scope);
        ApplyAction(rule, dto);

        await _context.PromotionRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }

    private static void ApplyScope(PromotionRule rule, PromotionRuleScopeDto? scope)
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
    }

    private static void ApplyAction(PromotionRule rule, PromotionRuleUpsertDto dto)
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

