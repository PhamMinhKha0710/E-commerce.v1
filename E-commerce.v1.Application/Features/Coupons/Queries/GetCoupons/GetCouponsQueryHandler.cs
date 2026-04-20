using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, PagedResult<CouponDto>>
{
    private readonly ICouponReadRepository _repository;

    public GetCouponsQueryHandler(ICouponReadRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        return _repository.SearchAsync(request.Code, request.IsActive, request.PageNumber, request.PageSize, cancellationToken);
    }
}
