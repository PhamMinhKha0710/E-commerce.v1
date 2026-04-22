using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;

public class CheckoutSelectedCommandHandler : IRequestHandler<CheckoutSelectedCommand, CheckoutResponse>
{
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<CheckoutSelectedCommandHandler> _logger;

    public CheckoutSelectedCommandHandler(ICheckoutService checkoutService, ILogger<CheckoutSelectedCommandHandler> logger)
    {
        _checkoutService = checkoutService;
        _logger = logger;
    }

    public async Task<CheckoutResponse> Handle(CheckoutSelectedCommand request, CancellationToken cancellationToken)
    {
        var cartItemIds = request.CartItemIds ?? Array.Empty<Guid>();
        _logger.LogInformation(
            "CheckoutSelected requested by user {UserId} with {CartItemCount} items and payment method {PaymentMethod}",
            request.UserId,
            cartItemIds.Count,
            request.PaymentMethod);
        return await _checkoutService.CheckoutSelectedAsync(
            request.UserId,
            cartItemIds,
            request.PaymentMethod,
            request.Shipping,
            cancellationToken);
    }
}
