using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    private readonly ICheckoutRepository _checkoutRepository;

    public PaymentRepository(AppDbContext context, ICheckoutRepository checkoutRepository)
    {
        _context = context;
        _checkoutRepository = checkoutRepository;
    }

    public Task<bool> HasActiveReservationAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return _context.StockReservations.AnyAsync(
            r => r.OrderId == orderId &&
                 r.Status == StockReservationStatus.Reserved &&
                 r.ExpiresAt > now,
            cancellationToken);
    }

    public async Task ReserveStockForOrderAsync(
        Order order,
        DateTime nowUtc,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var existing = await _context.StockReservations
            .Where(r => r.OrderId == order.Id && r.Status == StockReservationStatus.Reserved && r.ExpiresAt > nowUtc)
            .ToListAsync(cancellationToken);
        if (existing.Count > 0)
            return;

        var expiresAt = nowUtc.Add(ttl);

        // Atomic stock decrement to avoid oversell under concurrency.
        foreach (var item in order.Items)
        {
            // Only reserve active products and ensure stock is sufficient.
            var affected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Products SET Stock = Stock - {item.Quantity} WHERE Id = {item.ProductId} AND IsActive = 1 AND Stock >= {item.Quantity}",
                cancellationToken);

            if (affected != 1)
                throw new BadRequestException("Một số sản phẩm không đủ tồn kho hoặc đã ngừng bán.");

            _context.StockReservations.Add(new StockReservation
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Status = StockReservationStatus.Reserved,
                ExpiresAt = expiresAt
            });
        }
    }

    public async Task ReleaseReservedStockAsync(Guid orderId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var reservations = await _context.StockReservations
            .Where(r => r.OrderId == orderId && r.Status == StockReservationStatus.Reserved)
            .ToListAsync(cancellationToken);
        if (reservations.Count == 0)
            return;

        foreach (var r in reservations)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Products SET Stock = Stock + {r.Quantity} WHERE Id = {r.ProductId}",
                cancellationToken);

            r.Status = StockReservationStatus.Released;
            r.ReleasedAt = nowUtc;
        }
    }

    public async Task ConvertReservationsAsync(Guid orderId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var reservations = await _context.StockReservations
            .Where(r => r.OrderId == orderId && r.Status == StockReservationStatus.Reserved)
            .ToListAsync(cancellationToken);
        if (reservations.Count == 0)
            return;

        foreach (var r in reservations)
        {
            r.Status = StockReservationStatus.Converted;
            r.ConvertedAt = nowUtc;
        }
    }

    public Task<PaymentTransaction?> GetLatestTransactionAsync(
        Guid orderId,
        PaymentProvider provider,
        CancellationToken cancellationToken)
    {
        return _context.PaymentTransactions
            .Where(t => t.OrderId == orderId && t.Provider == provider)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken cancellationToken)
    {
        _context.PaymentTransactions.Add(transaction);
        return Task.CompletedTask;
    }
}

