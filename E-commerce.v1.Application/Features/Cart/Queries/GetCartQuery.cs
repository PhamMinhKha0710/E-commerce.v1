using E_commerce.v1.Application.DTOs.Cart;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Queries;

public class GetCartQuery : IRequest<CartDto>
{
    public Guid UserId { get; set; }
}
