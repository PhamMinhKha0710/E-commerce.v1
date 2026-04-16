using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;

public class CheckoutSelectedCommandHandler : IRequestHandler<CheckoutSelectedCommand, CheckoutResponse>
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutSelectedCommandHandler(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    public async Task<CheckoutResponse> Handle(CheckoutSelectedCommand request, CancellationToken cancellationToken)
    {
        return await _checkoutService.CheckoutSelectedAsync(
            request.UserId,
            request.CartItemIds,
            request.PaymentMethod,
            cancellationToken);
    }
}
