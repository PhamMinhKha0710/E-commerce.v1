namespace E_commerce.v1.Domain.Entities;

public class PromotionBuyXGetYAction
{
    public Guid PromotionRuleId { get; set; }
    public PromotionRule PromotionRule { get; set; } = null!;

    public Guid? BuyProductId { get; set; }
    public Product? BuyProduct { get; set; }
    public Guid? BuyCategoryId { get; set; }
    public Category? BuyCategory { get; set; }

    public int BuyQty { get; set; }

    public Guid? GetProductId { get; set; }
    public Product? GetProduct { get; set; }
    public Guid? GetCategoryId { get; set; }
    public Category? GetCategory { get; set; }

    public int GetQty { get; set; }
    public int? LimitPerOrder { get; set; }
}

