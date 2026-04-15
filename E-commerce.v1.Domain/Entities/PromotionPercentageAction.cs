using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class PromotionPercentageAction
{
    public Guid PromotionRuleId { get; set; }
    public PromotionRule PromotionRule { get; set; } = null!;

    public decimal Percent { get; set; }
    public PromotionDiscountTarget Target { get; set; } = PromotionDiscountTarget.EligibleItems;
}

