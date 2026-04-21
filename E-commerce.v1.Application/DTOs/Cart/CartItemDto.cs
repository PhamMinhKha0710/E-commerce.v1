namespace E_commerce.v1.Application.DTOs.Cart;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
