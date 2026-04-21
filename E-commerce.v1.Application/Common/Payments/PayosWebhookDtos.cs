namespace E_commerce.v1.Application.Common.Payments;

public sealed class PayosWebhookEvent
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentLinkId { get; set; }
    public string? RawDataId { get; set; }
}

