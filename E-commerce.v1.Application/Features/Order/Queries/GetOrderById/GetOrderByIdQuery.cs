using E_commerce.v1.Application.DTOs.Order;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid UserId, Guid OrderId) : IRequest<OrderDto>;
