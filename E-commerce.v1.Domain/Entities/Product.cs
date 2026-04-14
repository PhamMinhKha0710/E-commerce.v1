using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    public int Stock { get; set; }
    
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    // Soft Delete Flag
    public bool IsDeleted { get; set; } = false;
}
