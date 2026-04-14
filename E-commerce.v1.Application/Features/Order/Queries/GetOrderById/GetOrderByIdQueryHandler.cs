using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IAppDbContext _context;

    public GetOrderByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Id == request.OrderId && o.UserId == request.UserId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                Subtotal = o.Subtotal,
                DiscountTotal = o.DiscountTotal,
                GrandTotal = o.GrandTotal,
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

        if (order == null)
            throw new NotFoundException("Không tìm thấy đơn hàng.");

        return order;
    }
}
