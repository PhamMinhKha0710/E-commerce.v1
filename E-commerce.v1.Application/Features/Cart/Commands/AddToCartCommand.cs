using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class AddToCartCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}