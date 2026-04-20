using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery(
    string? Code = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<CouponDto>>;
