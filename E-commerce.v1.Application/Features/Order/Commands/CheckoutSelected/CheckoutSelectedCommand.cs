using E_commerce.v1.Domain.Enums;
using MediatR;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.DTOs.Shipping;

namespace E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;

public record CheckoutSelectedCommand(
    Guid UserId,
    IReadOnlyCollection<Guid> CartItemIds,
    PaymentMethod PaymentMethod,
    CheckoutShippingInfo? Shipping) : IRequest<CheckoutResponse>;
