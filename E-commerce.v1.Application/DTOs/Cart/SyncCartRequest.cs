namespace E_commerce.v1.Application.DTOs.Cart;

/// <summary>Body cho POST /api/v1/cart/sync sau khi đăng nhập.</summary>
public class SyncCartRequest
{
    public List<SyncCartLineDto> Items { get; set; } = new();
}
