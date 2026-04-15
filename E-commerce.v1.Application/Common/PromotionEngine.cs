using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Common;

public record PromotionCartItem(Guid ProductId, Guid CategoryId, decimal UnitPrice, int Quantity);

public record PromotionCandidate(Guid RuleId, decimal DiscountAmount, int Priority, string Summary);

public static class PromotionEngine
{
    public static async Task<PromotionCandidate?> CalculateBestAsync(
        IAppDbContext context,
        IReadOnlyList<PromotionCartItem> items,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return null;

        var rules = await context.PromotionRules
            .AsNoTracking()
            .Include(r => r.Products)
            .Include(r => r.Categories)
            .Include(r => r.BuyXGetYAction)
            .Include(r => r.PercentageAction)
            .Where(r => r.IsActive && r.StartDate <= utcNow && r.EndDate >= utcNow)
            .ToListAsync(cancellationToken);

        PromotionCandidate? best = null;
        foreach (var rule in rules)
        {
            var candidate = EvaluateRule(rule, items);
            if (candidate == null)
                continue;

            if (best == null || Compare(candidate, best) < 0)
                best = candidate;
        }

        return best;
    }

    private static int Compare(PromotionCandidate a, PromotionCandidate b)
    {
        // prefer higher discount; tie-break by higher priority; then stable by ruleId
        var d = b.DiscountAmount.CompareTo(a.DiscountAmount);
        if (d != 0) return d;
        var p = b.Priority.CompareTo(a.Priority);
        if (p != 0) return p;
        return a.RuleId.CompareTo(b.RuleId);
    }

    private static PromotionCandidate? EvaluateRule(PromotionRule rule, IReadOnlyList<PromotionCartItem> items)
    {
        var eligibleItems = FilterEligibleItems(rule, items);
        if (eligibleItems.Count == 0)
            return null;

        return rule.Type switch
        {
            PromotionRuleType.BuyXGetY => EvaluateBuyXGetY(rule, eligibleItems),
            PromotionRuleType.PercentageDiscount => EvaluatePercentage(rule, eligibleItems),
            _ => null
        };
    }

    private static List<PromotionCartItem> FilterEligibleItems(PromotionRule rule, IReadOnlyList<PromotionCartItem> items)
    {
        var hasProductScope = rule.Products.Count > 0;
        var hasCategoryScope = rule.Categories.Count > 0;

        if (!hasProductScope && !hasCategoryScope)
            return items.ToList();

        var productIds = hasProductScope ? rule.Products.Select(x => x.ProductId).ToHashSet() : null;
        var categoryIds = hasCategoryScope ? rule.Categories.Select(x => x.CategoryId).ToHashSet() : null;

        return items
            .Where(i =>
                (productIds != null && productIds.Contains(i.ProductId)) ||
                (categoryIds != null && categoryIds.Contains(i.CategoryId)))
            .ToList();
    }

    private static PromotionCandidate? EvaluatePercentage(PromotionRule rule, IReadOnlyList<PromotionCartItem> eligibleItems)
    {
        var action = rule.PercentageAction;
        if (action == null)
            return null;

        var baseAmount = action.Target switch
        {
            PromotionDiscountTarget.WholeOrder => eligibleItems.Sum(i => i.UnitPrice * i.Quantity),
            _ => eligibleItems.Sum(i => i.UnitPrice * i.Quantity)
        };

        if (baseAmount <= 0 || action.Percent <= 0)
            return null;

        var discount = decimal.Round(baseAmount * (action.Percent / 100m), 2, MidpointRounding.AwayFromZero);
        if (discount <= 0)
            return null;

        return new PromotionCandidate(
            rule.Id,
            discount,
            rule.Priority,
            $"{rule.Name} (-{action.Percent:0.##}%)");
    }

    private static PromotionCandidate? EvaluateBuyXGetY(PromotionRule rule, IReadOnlyList<PromotionCartItem> eligibleItems)
    {
        var action = rule.BuyXGetYAction;
        if (action == null || action.BuyQty <= 0 || action.GetQty <= 0)
            return null;

        var buyCount = eligibleItems.Sum(i => i.Quantity);
        if (buyCount < action.BuyQty)
            return null;

        var bundleCount = buyCount / action.BuyQty;
        if (bundleCount <= 0)
            return null;

        var freeQty = bundleCount * action.GetQty;
        if (action.LimitPerOrder.HasValue)
            freeQty = Math.Min(freeQty, action.LimitPerOrder.Value);

        // default: discount based on cheapest eligible item unit price
        var unit = eligibleItems.Min(i => i.UnitPrice);
        if (unit <= 0 || freeQty <= 0)
            return null;

        var discount = decimal.Round(unit * freeQty, 2, MidpointRounding.AwayFromZero);
        if (discount <= 0)
            return null;

        return new PromotionCandidate(
            rule.Id,
            discount,
            rule.Priority,
            $"{rule.Name} (-{freeQty} free)");
    }
}

