using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public record CheckoutCommand(Guid UserId) : IRequest<CheckoutResponse>;

public class CheckoutResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
}