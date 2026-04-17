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
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var lockedProducts = await _checkoutRepository.LockProductsForCheckoutAsync(productIds, cancellationToken);

        foreach (var item in order.Items)
        {
            if (!lockedProducts.TryGetValue(item.ProductId, out var product))
                throw new NotFoundException("Sản phẩm không tồn tại.");
            if (!product.IsActive)
                throw new BadRequestException($"Sản phẩm '{product.Name}' đã ngừng bán.");
            if (product.Stock < item.Quantity)
                throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");

            product.Stock -= item.Quantity;

            _context.StockReservations.Add(new StockReservation
            {
                OrderId = order.Id,
                ProductId = product.Id,
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

        var productIds = reservations.Select(r => r.ProductId).Distinct().ToList();
        var lockedProducts = await _checkoutRepository.LockProductsForCheckoutAsync(productIds, cancellationToken);

        foreach (var r in reservations)
        {
            if (lockedProducts.TryGetValue(r.ProductId, out var product))
                product.Stock += r.Quantity;

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

