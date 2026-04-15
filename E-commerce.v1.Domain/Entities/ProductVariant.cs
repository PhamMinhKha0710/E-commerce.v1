using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public int Inventory { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public ICollection<ProductVariantOption> Options { get; set; } = new List<ProductVariantOption>();
}

