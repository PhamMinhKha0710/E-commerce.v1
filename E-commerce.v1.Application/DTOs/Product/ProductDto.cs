namespace E_commerce.v1.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailDto : ProductDto
{
    public string? Description { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
}
