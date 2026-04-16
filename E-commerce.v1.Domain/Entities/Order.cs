using E_commerce.v1.Domain.Common;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public Guid? PromotionRuleId { get; set; }
    public PromotionRule? PromotionRule { get; set; }
    public decimal PromotionDiscount { get; set; }
    public string? PromotionSummary { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal RankDiscount { get; set; }
    public string? CouponCode { get; set; }
    public LoyaltyRank RankAtCheckout { get; set; } = LoyaltyRank.Silver;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cod;

    /// <summary>Ahamove order id after create-shipment succeeds.</summary>
    public string? AhamoveOrderId { get; set; }

    /// <summary>Last raw status string from Ahamove webhook (debugging).</summary>
    public string? AhamoveLastStatus { get; set; }

    // Shipping snapshot (for carrier integration)
    public string? ShippingReceiverName { get; set; }
    public string? ShippingReceiverPhone { get; set; }
    public string? ShippingAddressLine { get; set; }
    public double? ShippingLat { get; set; }
    public double? ShippingLng { get; set; }
    public string? ShippingNote { get; set; }
    public string? ShippingServiceId { get; set; }
    public decimal ShippingFee { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}