using E_commerce.v1.Domain.Common;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public PaymentProvider Provider { get; set; } = PaymentProvider.PayOS;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";

    public string? ProviderPaymentLinkId { get; set; }
    public long? ProviderOrderCode { get; set; }

    public DateTime? LastEventAt { get; set; }
    public string? LastEventIdempotencyKey { get; set; }

    public string? RawLastWebhookPayload { get; set; }
}

