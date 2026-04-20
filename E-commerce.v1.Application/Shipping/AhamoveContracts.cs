namespace E_commerce.v1.Application.Shipping;

using System.Text.Json.Serialization;

public class AhamovePathPoint
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Mobile { get; set; }
    public decimal? Cod { get; set; }
}

public class AhamovePackageItem
{
    public decimal Weight { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? Description { get; set; }
}

public class AhamoveEstimateRequest
{
    public double OrderTime { get; set; }
    public List<AhamovePathPoint> Path { get; set; } = new();

    /// <summary>
    /// Ahamove expects services as objects with "_id" + "requests" (even if empty).
    /// Example: [{ "_id": "SGN-BIKE", "requests": [] }]
    /// </summary>
    public List<AhamoveEstimateService> Services { get; set; } = new();

    /// <summary>BALANCE, CASH, or CASH_BY_RECIPIENT.</summary>
    public string PaymentMethod { get; set; } = "BALANCE";

    public List<AhamovePackageItem>? Items { get; set; }
    public bool? RouteOptimized { get; set; }
}

public class AhamoveEstimateService
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public List<AhamoveEstimateServiceRequest> Requests { get; set; } = new();
}

public class AhamoveEstimateServiceRequest
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public int Num { get; set; } = 1;
}

public class AhamoveEstimateData
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public decimal DistanceFee { get; set; }
    public decimal RequestFee { get; set; }
    public decimal StopFee { get; set; }
    public decimal VatFee { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalFee { get; set; }
}

public class AhamoveEstimateResultItem
{
    public string ServiceId { get; set; } = string.Empty;
    public AhamoveEstimateData? Data { get; set; }
}

public class AhamoveCreateOrderRequest
{
    public double OrderTime { get; set; }
    public List<AhamovePathPoint> Path { get; set; } = new();
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Special requests (can be empty). Example item: { "_id": "SGN-BIKE-TIP", "num": 1 }
    /// </summary>
    public List<AhamoveOrderRequestItem> Requests { get; set; } = new();

    public List<AhamovePackageItem>? Items { get; set; }
    public bool? RouteOptimized { get; set; }

    /// <summary>BALANCE, CASH, or CASH_BY_RECIPIENT.</summary>
    public string PaymentMethod { get; set; } = "BALANCE";

    public string? PromoCode { get; set; }
    public string? Remarks { get; set; }
}

public class AhamoveOrderRequestItem
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public int Num { get; set; } = 1;
}

public class AhamoveCreateOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SharedLink { get; set; }
}

public class AhamoveOrderDetailsResponse
{
    [JsonPropertyName("_id")]
    public string OrderId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? SharedLink { get; set; }
}
