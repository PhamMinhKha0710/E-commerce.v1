using E_commerce.v1.Application.Shipping;

namespace E_commerce.v1.Application.DTOs.Shipping;

public class GetShippingFeeRequest
{
    public double OrderTime { get; set; }
    public List<AhamovePathPoint> Path { get; set; } = new();
    public List<string> Services { get; set; } = new();
    public List<AhamovePackageItem>? Items { get; set; }
    public bool? RouteOptimized { get; set; }
}

public class CheckoutShippingInfo
{
    /// <summary>Ahamove service id, e.g. SGN-BIKE.</summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>Receiver (dropoff) point. Lat/Lng/Address required.</summary>
    public AhamovePathPoint Dropoff { get; set; } = new();

    public List<AhamovePackageItem>? Items { get; set; }
    public bool? RouteOptimized { get; set; }
    public string? Note { get; set; }
}

public class ShippingFeeResponse
{
    public List<ShippingFeeEstimateDto> Estimates { get; set; } = new();
}

public class ShippingFeeEstimateDto
{
    public string ServiceId { get; set; } = string.Empty;
    public decimal TotalFee { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public decimal DistanceFee { get; set; }
    public decimal RequestFee { get; set; }
}

public class CreateShipmentRequest
{
    public double OrderTime { get; set; }
    public List<AhamovePathPoint> Path { get; set; } = new();
    public string ServiceId { get; set; } = string.Empty;
    public List<AhamovePackageItem>? Items { get; set; }
    public bool? RouteOptimized { get; set; }

    /// <summary>Ahamove shipping payment: BALANCE, CASH, or CASH_BY_RECIPIENT.</summary>
    public string AhamovePaymentMethod { get; set; } = "BALANCE";

    public string? PromoCode { get; set; }
    public string? Remarks { get; set; }
}

public class CreateShipmentResponse
{
    public string AhamoveOrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SharedLink { get; set; }
}
