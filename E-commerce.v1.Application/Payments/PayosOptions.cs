namespace E_commerce.v1.Application.Payments;

public class PayosOptions
{
    public const string SectionName = "Payos";

    /// <summary>Base URL without trailing slash, e.g. https://api-merchant.payos.vn</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;

    /// <summary>Absolute URL for PayOS redirect on success.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>Absolute URL for PayOS redirect on cancel.</summary>
    public string CancelUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>Default reservation TTL for online payments (minutes).</summary>
    public int ReservationTtlMinutes { get; set; } = 15;
}

