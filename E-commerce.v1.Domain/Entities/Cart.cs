using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; } 
    public Guid? AppliedCouponId { get; set; }
    public Coupon? AppliedCoupon { get; set; }
    public string? AppliedCouponCode { get; set; }
    public decimal CouponDiscountPreview { get; set; }

    //một giỏ hàng thì có nhiều sản phẩm 
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}