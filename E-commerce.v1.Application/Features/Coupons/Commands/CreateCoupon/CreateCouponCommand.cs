using E_commerce.v1.Domain.Enums;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommand : IRequest<Guid>
{
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UsageLimit { get; set; }
}
