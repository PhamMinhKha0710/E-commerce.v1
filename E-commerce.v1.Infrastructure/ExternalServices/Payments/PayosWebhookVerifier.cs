using System.Text.Json;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Payments;
using E_commerce.v1.Domain.Exceptions;
using Microsoft.Extensions.Options;
using PayOS.Crypto;

namespace E_commerce.v1.Infrastructure.ExternalServices.Payments;

public class PayosWebhookVerifier : IPayosWebhookVerifier
{
    private readonly PayosOptions _options;
    private readonly CryptoProvider _crypto;

    public PayosWebhookVerifier(IOptions<PayosOptions> options)
    {
        _options = options.Value;
        _crypto = new CryptoProvider();
    }

    public Task<PayosWebhookEvent> VerifyAndParseAsync(string rawBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            throw new BadRequestException("Webhook payload trống.");

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("data", out var data))
            throw new BadRequestException("Webhook payload thiếu trường data.");
        if (!root.TryGetProperty("signature", out var signatureEl))
            throw new BadRequestException("Webhook payload thiếu chữ ký.");

        var provided = signatureEl.GetString();
        if (string.IsNullOrWhiteSpace(provided))
            throw new BadRequestException("Webhook signature không hợp lệ.");

        var computed = _crypto.CreateSignatureFromObject(data, _options.ChecksumKey);
        if (!SecureEquals(computed, provided))
            throw new BadRequestException("Webhook signature không hợp lệ.");

        var evt = new PayosWebhookEvent
        {
            OrderCode = data.TryGetProperty("orderCode", out var oc) ? oc.GetInt64() : 0,
            Amount = data.TryGetProperty("amount", out var amt) ? amt.GetInt32() : 0,
            Status = data.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty,
            PaymentLinkId = data.TryGetProperty("paymentLinkId", out var pl) ? pl.GetString() : null,
            RawDataId = data.TryGetProperty("id", out var id) ? id.GetString() : null
        };

        if (evt.OrderCode <= 0)
            throw new BadRequestException("Webhook thiếu orderCode.");

        return Task.FromResult(evt);
    }

    private static bool SecureEquals(string? a, string? b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }
}

