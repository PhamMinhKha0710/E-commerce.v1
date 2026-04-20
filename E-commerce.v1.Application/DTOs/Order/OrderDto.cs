using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PayosPaymentLinkId { get; set; }
    public long? PayosOrderCode { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public Guid? PromotionRuleId { get; set; }
    public decimal PromotionDiscount { get; set; }
    public string? PromotionSummary { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal RankDiscount { get; set; }
    public string? CouponCode { get; set; }
    public LoyaltyRank RankAtCheckout { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AhamoveOrderId { get; set; }
    public string? AhamoveLastStatus { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
