using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class UpdateCartItemCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}
