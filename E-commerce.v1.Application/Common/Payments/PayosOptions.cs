namespace E_commerce.v1.Application.Common.Payments;

public class PayosOptions
{
    public const string SectionName = "Payos";

    /// <summary>Base URL (không có dấu <c>/</c> cuối), ví dụ <c>https://api-merchant.payos.vn</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;

    /// <summary>URL tuyệt đối PayOS redirect khi thanh toán thành công.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>URL tuyệt đối PayOS redirect khi user hủy/thoát flow thanh toán.</summary>
    public string CancelUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>TTL giữ tồn kho tạm cho payment online (phút).</summary>
    public int ReservationTtlMinutes { get; set; } = 15;
}

