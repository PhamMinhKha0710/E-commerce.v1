using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutCommandHandler(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        return await _checkoutService.CheckoutAsync(request.UserId, request.PaymentMethod, request.Shipping, cancellationToken);
    }
}
