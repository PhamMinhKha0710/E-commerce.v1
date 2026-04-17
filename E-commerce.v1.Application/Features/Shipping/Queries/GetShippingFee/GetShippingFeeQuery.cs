using E_commerce.v1.Application.DTOs.Shipping;
using MediatR;

namespace E_commerce.v1.Application.Features.Shipping.Queries.GetShippingFee;

public record GetShippingFeeQuery(Guid UserId, GetShippingFeeRequest Body) : IRequest<ShippingFeeResponse>;
