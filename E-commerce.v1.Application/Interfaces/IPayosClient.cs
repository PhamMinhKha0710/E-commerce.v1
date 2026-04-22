using E_commerce.v1.Application.Common.Payments;

namespace E_commerce.v1.Application.Interfaces;

public interface IPayosClient
{
    Task<PayosCreatePaymentLinkResult> CreatePaymentLinkAsync(
        PayosCreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default);
}

