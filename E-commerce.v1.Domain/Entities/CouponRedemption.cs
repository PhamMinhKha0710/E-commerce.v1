using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class CouponRedemption : BaseEntity
{
    public Guid CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}
