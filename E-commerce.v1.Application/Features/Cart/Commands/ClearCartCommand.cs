using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public record ClearCartCommand(Guid UserId) : IRequest<Unit>;
