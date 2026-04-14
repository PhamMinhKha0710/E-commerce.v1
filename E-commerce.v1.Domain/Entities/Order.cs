using E_commerce.v1.Domain.Common;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}