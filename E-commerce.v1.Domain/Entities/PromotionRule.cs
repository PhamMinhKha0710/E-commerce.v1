using E_commerce.v1.Domain.Common;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class PromotionRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public PromotionRuleType Type { get; set; }
    public int Priority { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PromotionRuleProduct> Products { get; set; } = new List<PromotionRuleProduct>();
    public ICollection<PromotionRuleCategory> Categories { get; set; } = new List<PromotionRuleCategory>();

    public PromotionBuyXGetYAction? BuyXGetYAction { get; set; }
    public PromotionPercentageAction? PercentageAction { get; set; }
}

