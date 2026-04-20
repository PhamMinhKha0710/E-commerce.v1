using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Queries;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICartReadRepository _cartReadRepository;

    public GetCartQueryHandler(ICartReadRepository cartReadRepository)
    {
        _cartReadRepository = cartReadRepository;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        return await _cartReadRepository.GetCartAsync(request.UserId, cancellationToken);
    }
}
