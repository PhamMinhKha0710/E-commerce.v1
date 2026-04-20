using E_commerce.v1.Domain.Common;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public int Stock { get; set; }

    public string? Unit { get; set; }
    public string? UnitEn { get; set; }

    public bool IsActive { get; set; } = true;

    public ProductType ProductType { get; set; } = ProductType.Single;
    public ProductKind Kind { get; set; } = ProductKind.Sale;
    public BuyType BuyType { get; set; } = BuyType.All;

    public int Priority { get; set; }


    public List<string> DocumentIds { get; set; } = new();

    public int? StoreId { get; set; }

    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    public bool IsDeleted { get; set; } = false;
}
