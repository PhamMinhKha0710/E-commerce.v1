using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public record RemoveCartItemCommand(Guid UserId, Guid CartItemId) : IRequest<Unit>;
