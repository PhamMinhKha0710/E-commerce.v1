namespace E_commerce.v1.Application.Common.Shipping;

public class AhamoveOptions
{
    public const string SectionName = "Ahamove";

    /// <summary>Base URL without trailing slash, e.g. https://partner-apistg.ahamove.com</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Partner JWT (preferred when set).</summary>
    public string? BearerToken { get; set; }

    /// <summary>Alternative to Bearer; sent as apikey header when BearerToken is empty.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Expected api_key in webhook payload (staging sample includes this field).</summary>
    public string? WebhookApiKey { get; set; }

    public int TimeoutSeconds { get; set; } = 60;

    public AhamovePickupOptions Pickup { get; set; } = new();
}

public class AhamovePickupOptions
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
}

