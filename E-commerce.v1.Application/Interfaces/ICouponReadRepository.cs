using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;

namespace E_commerce.v1.Application.Interfaces;

public interface ICouponReadRepository
{
    Task<PagedResult<CouponDto>> SearchAsync(
        string? code,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<CouponDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
