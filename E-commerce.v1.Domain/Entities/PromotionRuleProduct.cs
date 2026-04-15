namespace E_commerce.v1.Domain.Entities;

public class PromotionRuleProduct
{
    public Guid PromotionRuleId { get; set; }
    public PromotionRule PromotionRule { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

