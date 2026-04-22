namespace E_commerce.v1.Application.DTOs.Cart;

/// <summary>
/// Dòng giỏ từ guest (localStorage). Client chỉ gửi ProductId + Quantity;
/// giá/tổng tiền luôn được tính lại phía server để tránh bị sửa payload.
/// </summary>
public class SyncCartLineDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
