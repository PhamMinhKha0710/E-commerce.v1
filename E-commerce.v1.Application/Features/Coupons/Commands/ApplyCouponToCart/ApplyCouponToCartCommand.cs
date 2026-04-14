using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Commands.ApplyCouponToCart;

public record ApplyCouponToCartCommand(Guid UserId, string CouponCode) : IRequest<ApplyCouponToCartResponse>;

public class ApplyCouponToCartResponse
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal CartSubtotal { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal TotalAfterDiscount { get; set; }
}
