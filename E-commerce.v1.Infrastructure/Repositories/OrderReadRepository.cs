using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly AppDbContext _context;

    public OrderReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId);

        if (userId.HasValue && userId.Value != Guid.Empty)
            query = query.Where(o => o.UserId == userId.Value);

        return await query
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                Subtotal = o.Subtotal,
                DiscountTotal = o.DiscountTotal,
                GrandTotal = o.GrandTotal,
                PromotionRuleId = o.PromotionRuleId,
                PromotionDiscount = o.PromotionDiscount,
                PromotionSummary = o.PromotionSummary,
                CouponDiscount = o.CouponDiscount,
                RankDiscount = o.RankDiscount,
                CouponCode = o.CouponCode,
                RankAtCheckout = o.RankAtCheckout,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductNameSnapshot = i.ProductNameSnapshot,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                Subtotal = o.Subtotal,
                DiscountTotal = o.DiscountTotal,
                GrandTotal = o.GrandTotal,
                PromotionRuleId = o.PromotionRuleId,
                PromotionDiscount = o.PromotionDiscount,
                PromotionSummary = o.PromotionSummary,
                CouponDiscount = o.CouponDiscount,
                RankDiscount = o.RankDiscount,
                CouponCode = o.CouponCode,
                RankAtCheckout = o.RankAtCheckout,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductNameSnapshot = i.ProductNameSnapshot,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}

