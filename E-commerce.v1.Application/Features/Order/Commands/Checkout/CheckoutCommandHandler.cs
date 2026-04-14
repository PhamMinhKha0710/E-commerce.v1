using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    private readonly ICheckoutRepository _checkoutRepository;

    public CheckoutCommandHandler(ICheckoutRepository checkoutRepository)
    {
        _checkoutRepository = checkoutRepository;
    }

    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        return await _checkoutRepository.CheckoutAsync(request.UserId, request.PaymentMethod, cancellationToken);
    }
}
