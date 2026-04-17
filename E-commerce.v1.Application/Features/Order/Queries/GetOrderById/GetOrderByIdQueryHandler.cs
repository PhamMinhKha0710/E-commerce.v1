using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderReadRepository _orderReadRepository;

    public GetOrderByIdQueryHandler(IOrderReadRepository orderReadRepository)
    {
        _orderReadRepository = orderReadRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _orderReadRepository.GetByIdAsync(
            request.OrderId,
            request.UserId != Guid.Empty ? request.UserId : null,
            cancellationToken);
        if (dto == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        return dto;
    }
}
