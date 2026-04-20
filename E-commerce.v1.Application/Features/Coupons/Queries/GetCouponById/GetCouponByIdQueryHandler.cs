using E_commerce.v1.Application.DTOs.Coupon;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Coupons.Queries.GetCouponById;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDto>
{
    private readonly ICouponReadRepository _repository;

    public GetCouponByIdQueryHandler(ICouponReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<CouponDto> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repository.GetByIdAsync(request.Id, cancellationToken)
                  ?? throw new NotFoundException("Không tìm thấy coupon.");
        return dto;
    }
}
