using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.CancelOrder;

public record CancelOrderCommand(Guid UserId, Guid OrderId) : IRequest<Unit>;
