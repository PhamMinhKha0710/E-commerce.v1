using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Payments;
using Microsoft.Extensions.Options;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Resources.V2.PaymentRequests;

namespace E_commerce.v1.Infrastructure.ExternalServices.Payments;

public class PayosClient : IPayosClient
{
    private readonly PayosOptions _options;
    private readonly PayOS.PayOSClient _client;
    private readonly PaymentRequests _paymentRequests;

    public PayosClient(IOptions<PayosOptions> options)
    {
        _options = options.Value;

        var payosOptions = new PayOS.PayOSOptions
        {
            ClientId = _options.ClientId,
            ApiKey = _options.ApiKey,
            ChecksumKey = _options.ChecksumKey,
            TimeoutMs = Math.Max(1, _options.TimeoutSeconds) * 1000
        };
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            payosOptions.BaseUrl = _options.BaseUrl.Trim().TrimEnd('/');

        _client = new PayOS.PayOSClient(payosOptions);
        _paymentRequests = new PaymentRequests(_client);
    }

    public async Task<PayosCreatePaymentLinkResult> CreatePaymentLinkAsync(
        PayosCreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var create = new CreatePaymentLinkRequest
        {
            OrderCode = request.OrderCode,
            Amount = request.Amount,
            Description = request.Description,
            ReturnUrl = request.ReturnUrl,
            CancelUrl = request.CancelUrl
        };

        var response = await _paymentRequests.CreateAsync(
            create,
            new RequestOptions<CreatePaymentLinkRequest>
            {
                CancellationToken = cancellationToken,
                Signature = new SignatureOptions
                {
                    Request = RequestSignatureTypes.CreatePaymentLink,
                    Response = ResponseSignatureTypes.Body
                }
            });

        return new PayosCreatePaymentLinkResult
        {
            CheckoutUrl = response.CheckoutUrl ?? string.Empty,
            PaymentLinkId = response.PaymentLinkId,
            OrderCode = response.OrderCode
        };
    }
}

