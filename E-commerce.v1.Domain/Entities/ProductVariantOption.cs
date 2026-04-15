namespace E_commerce.v1.Domain.Entities;

public class ProductVariantOption
{
    public Guid VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

