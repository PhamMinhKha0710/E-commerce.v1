using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Promotions.Commands.UpdatePromotionRule;

public class UpdatePromotionRuleCommandHandler : IRequestHandler<UpdatePromotionRuleCommand>
{
    private readonly IAppDbContext _context;

    public UpdatePromotionRuleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.PromotionRules
            .Include(r => r.Products)
            .Include(r => r.Categories)
            .Include(r => r.BuyXGetYAction)
            .Include(r => r.PercentageAction)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
            throw new NotFoundException("Promotion rule không tồn tại.");

        var dto = request.Rule;
        rule.Name = dto.Name.Trim();
        rule.Type = dto.Type;
        rule.Priority = dto.Priority;
        rule.StartDate = dto.StartDate;
        rule.EndDate = dto.EndDate;
        rule.IsActive = dto.IsActive;

        rule.Products.Clear();
        rule.Categories.Clear();
        ApplyScope(rule, dto.Scope);

        rule.BuyXGetYAction = null;
        rule.PercentageAction = null;
        ApplyAction(rule, dto);

        await _context.SaveChangesAsync(cancellationToken);
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

