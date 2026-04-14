using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Order.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    private readonly IAppDbContext _context;

    public GetMyOrdersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == request.UserId)
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

        return orders;
    }
}
