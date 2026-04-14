using E_commerce.v1.Application.DTOs.Order;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.GetMyOrders;

public record GetMyOrdersQuery(Guid UserId) : IRequest<List<OrderDto>>;
