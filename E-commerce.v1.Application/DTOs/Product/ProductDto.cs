using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public bool IsActive { get; set; }
    public ProductType ProductType { get; set; }
    public ProductKind Kind { get; set; }
    public IList<string> DocumentIds { get; set; } = new List<string>();

    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailDto : ProductDto
{
    public string? NameEn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? Unit { get; set; }
    public string? UnitEn { get; set; }
    public BuyType BuyType { get; set; }
    public int Priority { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public int? StoreId { get; set; }
    public DateTime CreatedAt { get; set; }
}
