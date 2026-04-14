namespace E_commerce.v1.Application.DTOs.Cart;

public class CartDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    public decimal TotalAmount { get; set; } // Tổng giá tiền của tất cả items
}
