using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class AddToCartCommand : IRequest<Unit>
{
    public Guid UserId { get; set; } //lấy tokken từ controler đẩy qua đây
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}