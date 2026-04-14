using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;

namespace E_commerce.v1.Application.Common;

public static class CouponCalculator
{
    public static decimal ValidateAndCalculateDiscount(Coupon coupon, decimal subtotal, DateTime utcNow)
    {
        if (!coupon.IsActive)
            throw new BadRequestException("Mã giảm giá hiện không khả dụng.");
        if (utcNow < coupon.StartDate || utcNow > coupon.EndDate)
            throw new BadRequestException("Mã giảm giá đã hết hạn hoặc chưa đến thời gian áp dụng.");
        if (coupon.UsedCount >= coupon.UsageLimit)
            throw new BadRequestException("Mã giảm giá đã hết lượt sử dụng.");
        if (subtotal < coupon.MinOrderValue)
            throw new BadRequestException("Đơn hàng chưa đạt giá trị tối thiểu để áp dụng mã.");

        var discount = coupon.DiscountType switch
        {
            DiscountType.Percentage => subtotal * (coupon.Value / 100m),
            DiscountType.FixedAmount => coupon.Value,
            _ => 0m
        };

        if (discount < 0)
            discount = 0;
        if (discount > subtotal)
            discount = subtotal;

        return decimal.Round(discount, 2, MidpointRounding.AwayFromZero);
    }
}
