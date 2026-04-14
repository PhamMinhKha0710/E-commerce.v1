namespace E_commerce.v1.Application.DTOs.Cart;

/// <summary>Dòng giỏ từ guest (localStorage). Chỉ gửi ProductId + Quantity — không gửi giá.</summary>
public class SyncCartLineDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
