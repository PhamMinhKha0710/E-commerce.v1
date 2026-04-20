using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrderById;

public class GetAdminOrderByIdQueryHandler : IRequestHandler<GetAdminOrderByIdQuery, OrderDto>
{
    private readonly IOrderReadRepository _orderReadRepository;

    public GetAdminOrderByIdQueryHandler(IOrderReadRepository orderReadRepository)
    {
        _orderReadRepository = orderReadRepository;
    }

    public async Task<OrderDto> Handle(GetAdminOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _orderReadRepository.GetByIdAsync(request.OrderId, userId: null, cancellationToken);
        if (dto == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        return dto;
    }
}
