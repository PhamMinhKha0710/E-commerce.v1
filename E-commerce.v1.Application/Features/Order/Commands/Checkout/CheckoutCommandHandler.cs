using E_commerce.v1.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<CheckoutCommandHandler> _logger;

    public CheckoutCommandHandler(ICheckoutService checkoutService, ILogger<CheckoutCommandHandler> logger)
    {
        _checkoutService = checkoutService;
        _logger = logger;
    }

    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checkout requested by user {UserId} with payment method {PaymentMethod}", request.UserId, request.PaymentMethod);
        return await _checkoutService.CheckoutAsync(request.UserId, request.PaymentMethod, request.Shipping, cancellationToken);
    }
}
