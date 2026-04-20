namespace E_commerce.v1.Application.DTOs.Product;

public class ProductVariantOptionDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public int Inventory { get; set; }
    public bool IsActive { get; set; }

    public IList<ProductVariantOptionDto> Options { get; set; } = new List<ProductVariantOptionDto>();
}

