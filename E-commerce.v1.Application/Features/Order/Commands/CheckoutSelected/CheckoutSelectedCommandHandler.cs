using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;

public class CheckoutSelectedCommandHandler : IRequestHandler<CheckoutSelectedCommand, CheckoutResponse>
{
    private readonly ICheckoutRepository _checkoutRepository;

    public CheckoutSelectedCommandHandler(ICheckoutRepository checkoutRepository)
    {
        _checkoutRepository = checkoutRepository;
    }

    public async Task<CheckoutResponse> Handle(CheckoutSelectedCommand request, CancellationToken cancellationToken)
    {
        return await _checkoutRepository.CheckoutSelectedAsync(
            request.UserId,
            request.CartItemIds,
            request.PaymentMethod,
            cancellationToken);
    }
}
