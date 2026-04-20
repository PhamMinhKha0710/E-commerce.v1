using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrders;

public class GetAdminOrdersQueryHandler : IRequestHandler<GetAdminOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderReadRepository _orderReadRepository;

    public GetAdminOrdersQueryHandler(IOrderReadRepository orderReadRepository)
    {
        _orderReadRepository = orderReadRepository;
    }

    public Task<PagedResult<OrderDto>> Handle(GetAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        return _orderReadRepository.SearchAsync(
            request.Status,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
