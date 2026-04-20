using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.DTOs.Promotion;

public class PromotionRuleScopeDto
{
    public IReadOnlyList<Guid>? ProductIds { get; set; }
    public IReadOnlyList<Guid>? CategoryIds { get; set; }
}

public class BuyXGetYActionDto
{
    public Guid? BuyProductId { get; set; }
    public Guid? BuyCategoryId { get; set; }
    public int BuyQty { get; set; }

    public Guid? GetProductId { get; set; }
    public Guid? GetCategoryId { get; set; }
    public int GetQty { get; set; }

    public int? LimitPerOrder { get; set; }
}

public class PercentageActionDto
{
    public decimal Percent { get; set; }
    public PromotionDiscountTarget Target { get; set; } = PromotionDiscountTarget.EligibleItems;
}

public class PromotionRuleUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public PromotionRuleType Type { get; set; }
    public int Priority { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public PromotionRuleScopeDto? Scope { get; set; }
    public BuyXGetYActionDto? BuyXGetYAction { get; set; }
    public PercentageActionDto? PercentageAction { get; set; }
}

