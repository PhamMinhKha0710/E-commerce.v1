using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Soft Delete Flag
    public bool IsDeleted { get; set; } = false;

    // Navigation property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
