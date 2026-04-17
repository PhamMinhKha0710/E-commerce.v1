using E_commerce.v1.Domain.Common;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class StockReservation : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public StockReservationStatus Status { get; set; } = StockReservationStatus.Reserved;

    public DateTime ExpiresAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}

