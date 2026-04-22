using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Common.Shipping;

public static class AhamoveStatusMapper
{
    /// <summary>Map status string từ Ahamove → <see cref="OrderStatus"/> (chỉ map các status đã quy ước).</summary>
    public static OrderStatus? TryMapToOrderStatus(string? ahamoveStatus)
    {
        if (string.IsNullOrWhiteSpace(ahamoveStatus))
            return null;

        var s = ahamoveStatus.Trim().ToUpperInvariant();
        return s switch
        {
            "COMPLETED" or "FINISHED" or "SUCCESS" => OrderStatus.Completed,
            "CANCELLED" or "FAILED" or "CANCEL" => OrderStatus.Cancelled,
            "ASSIGNING" or "ACCEPTED" or "PENDING" => OrderStatus.Confirmed,
            "IN_PROCESS" or "PICKING" or "PICKED_UP" or "DELIVERING" or "ON_GOING" or "IN_TRANSIT" => OrderStatus.Delivering,
            _ => null
        };
    }
}

