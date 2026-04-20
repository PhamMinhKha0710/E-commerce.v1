using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.DTOs.Promotion;

public class PromotionRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PromotionRuleType Type { get; set; }
    public int Priority { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PromotionRuleDetailDto : PromotionRuleDto
{
    public PromotionRuleScopeDto? Scope { get; set; }
    public BuyXGetYActionDto? BuyXGetYAction { get; set; }
    public PercentageActionDto? PercentageAction { get; set; }
}
