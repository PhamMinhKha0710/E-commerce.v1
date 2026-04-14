using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    //cart của ai
    public Cart? Cart { get; set; } 
    public Guid ProductId { get; set; }
    //sản phẩm nào
    public Product? Product { get; set; } 
    public int Quantity { get; set; }
}
