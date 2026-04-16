using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutResponse> CheckoutAsync(Guid userId, PaymentMethod paymentMethod, CancellationToken cancellationToken);

    Task<CheckoutResponse> CheckoutSelectedAsync(
        Guid userId,
        IReadOnlyCollection<Guid> cartItemIds,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken);
}

