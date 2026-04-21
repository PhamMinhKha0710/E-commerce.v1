namespace E_commerce.v1.Application.DTOs.Cart;

/// <summary>Body cho <c>POST /api/v1/cart/sync</c> (merge giỏ guest sau khi đăng nhập).</summary>
public class SyncCartRequest
{
    public List<SyncCartLineDto> Items { get; set; } = new();
}
