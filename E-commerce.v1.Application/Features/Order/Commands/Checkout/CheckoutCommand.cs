using MediatR;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public record CheckoutCommand(Guid UserId, PaymentMethod PaymentMethod) : IRequest<CheckoutResponse>;

public class CheckoutResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}