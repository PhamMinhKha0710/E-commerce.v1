using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<(List<Order> Items, int Total)> GetMyOrdersAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Order?> UpdateOrderStatusAsync(Guid orderId, int newStatus, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order != null)
        {
            order.Status = (OrderStatus)newStatus;
        }
        return order;
    }
}
