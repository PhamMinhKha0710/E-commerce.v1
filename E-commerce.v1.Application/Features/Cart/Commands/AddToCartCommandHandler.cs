using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Unit>
{
    private readonly ICartService _cartService;

    public AddToCartCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        await _cartService.AddToCartAsync(request.UserId, request.ProductId, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
