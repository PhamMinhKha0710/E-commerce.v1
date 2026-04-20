using E_commerce.v1.Application.DTOs.Coupon;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Queries.GetCouponById;

public record GetCouponByIdQuery(Guid Id) : IRequest<CouponDto>;
