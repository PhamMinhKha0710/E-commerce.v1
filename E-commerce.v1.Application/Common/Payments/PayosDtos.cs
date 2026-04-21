namespace E_commerce.v1.Application.Common.Payments;

public sealed class PayosCreatePaymentLinkRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public sealed class PayosCreatePaymentLinkResult
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? PaymentLinkId { get; set; }
    public long? OrderCode { get; set; }
}

