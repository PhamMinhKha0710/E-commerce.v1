namespace E_commerce.v1.Domain.Entities;

public class PromotionRuleCategory
{
    public Guid PromotionRuleId { get; set; }
    public PromotionRule PromotionRule { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

