using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid ProductId { get; set; }

    public string ProductNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }

    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}