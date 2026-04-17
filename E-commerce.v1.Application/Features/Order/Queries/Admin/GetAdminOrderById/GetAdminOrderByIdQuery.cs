using E_commerce.v1.Application.DTOs.Order;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrderById;

public record GetAdminOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
