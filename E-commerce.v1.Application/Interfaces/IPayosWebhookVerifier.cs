using E_commerce.v1.Application.Common.Payments;

namespace E_commerce.v1.Application.Interfaces;

public interface IPayosWebhookVerifier
{
    Task<PayosWebhookEvent> VerifyAndParseAsync(string rawBody, CancellationToken cancellationToken = default);
}

