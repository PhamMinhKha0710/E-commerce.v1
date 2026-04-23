using System.Data;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Infrastructure.BackgroundServices;

/// <summary>
/// Releases expired stock reservations to avoid "stuck" inventory when provider webhook never arrives.
/// </summary>
public sealed class ExpiredStockReservationReaper : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);
    private const int BatchSize = 200;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredStockReservationReaper> _logger;

    public ExpiredStockReservationReaper(IServiceScopeFactory scopeFactory, ILogger<ExpiredStockReservationReaper> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReapOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Expired reservation reaper failed.");
            }

            try
            {
                await Task.Delay(TickInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }
    }

    private async Task ReapOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var expired = await db.StockReservations
            .Where(r => r.Status == StockReservationStatus.Reserved && r.ExpiresAt <= now)
            .OrderBy(r => r.ExpiresAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            await tx.CommitAsync(cancellationToken);
            return;
        }

        var orderIds = expired.Select(r => r.OrderId).Distinct().ToList();
        var orders = await db.Orders
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        // Release stock for each reservation (atomic SQL update), then mark reservation as released.
        foreach (var r in expired)
        {
            // If a reservation was already converted by a successful payment webhook, it would not be in this list.
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Products SET Stock = Stock + {r.Quantity} WHERE Id = {r.ProductId}",
                cancellationToken);

            r.Status = StockReservationStatus.Released;
            r.ReleasedAt = now;
        }

        // Best-effort: mark orders as Expired if they are still pending payment (prevents endless Pending).
        foreach (var o in orders)
        {
            if (o.PaymentMethod == PaymentMethod.EWallet && o.PaymentStatus == PaymentStatus.Pending)
                o.PaymentStatus = PaymentStatus.Expired;
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        _logger.LogInformation("Released {Count} expired stock reservations.", expired.Count);
    }
}

