namespace E_commerce.v1.Application.Common.Shipping;

public class AhamoveOptions
{
    public const string SectionName = "Ahamove";

    /// <summary>Base URL (không có dấu <c>/</c> cuối), ví dụ <c>https://partner-apistg.ahamove.com</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Partner JWT (ưu tiên dùng nếu có).</summary>
    public string? BearerToken { get; set; }

    /// <summary>Phương án thay thế Bearer; gửi header <c>apikey</c> khi <c>BearerToken</c> rỗng.</summary>
    public string? ApiKey { get; set; }

    /// <summary><c>api_key</c> mong đợi trong webhook payload (nếu Ahamove có gửi field này).</summary>
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

