using E_commerce.v1.Application.Payments;

namespace E_commerce.v1.Application.Interfaces;

public interface IPayosClient
{
    Task<PayosCreatePaymentLinkResult> CreatePaymentLinkAsync(
        PayosCreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default);
}

