using E_commerce.v1.Domain.Enums;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Unit>;
