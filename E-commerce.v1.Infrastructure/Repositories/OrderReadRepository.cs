using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
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
                PaymentStatus = o.PaymentStatus,
                PaidAt = o.PaidAt,
                PayosPaymentLinkId = o.PayosPaymentLinkId,
                PayosOrderCode = o.PayosOrderCode,
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
                AhamoveOrderId = o.AhamoveOrderId,
                AhamoveLastStatus = o.AhamoveLastStatus,
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

    public async Task<PagedResult<OrderDto>> SearchMyOrdersAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                PaidAt = o.PaidAt,
                PayosPaymentLinkId = o.PayosPaymentLinkId,
                PayosOrderCode = o.PayosOrderCode,
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
                AhamoveOrderId = o.AhamoveOrderId,
                AhamoveLastStatus = o.AhamoveLastStatus,
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

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }

    public async Task<PagedResult<OrderDto>> SearchAsync(
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Orders.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);
        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(o => o.CreatedAt <= toDate.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                PaidAt = o.PaidAt,
                PayosPaymentLinkId = o.PayosPaymentLinkId,
                PayosOrderCode = o.PayosOrderCode,
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
                AhamoveOrderId = o.AhamoveOrderId,
                AhamoveLastStatus = o.AhamoveLastStatus,
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

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }
}

