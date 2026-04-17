using MediatR;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Application.DTOs.Shipping;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public record CheckoutCommand(Guid UserId, PaymentMethod PaymentMethod, CheckoutShippingInfo? Shipping) : IRequest<CheckoutResponse>;

public class CheckoutResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal RankDiscount { get; set; }
    public string? CouponCode { get; set; }
    public decimal ShippingFee { get; set; }
    public string? ShippingServiceId { get; set; }
    public string? AhamoveOrderId { get; set; }
}