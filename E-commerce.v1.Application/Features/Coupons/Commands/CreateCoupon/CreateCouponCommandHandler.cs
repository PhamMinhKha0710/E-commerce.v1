using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateCouponCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await _context.Coupons.AnyAsync(c => c.Code == normalizedCode, cancellationToken);
        if (exists)
            throw new BadRequestException("Mã giảm giá đã tồn tại.");

        var coupon = new Coupon
        {
            Code = normalizedCode,
            DiscountType = request.DiscountType,
            Value = request.Value,
            MinOrderValue = request.MinOrderValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            IsActive = true
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }
}
