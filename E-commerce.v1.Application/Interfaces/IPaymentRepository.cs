using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Interfaces;

public interface IPaymentRepository
{
    Task<bool> HasActiveReservationAsync(Guid orderId, CancellationToken cancellationToken);

    Task ReserveStockForOrderAsync(
        Order order,
        DateTime nowUtc,
        TimeSpan ttl,
        CancellationToken cancellationToken);

    Task ReleaseReservedStockAsync(
        Guid orderId,
        DateTime nowUtc,
        CancellationToken cancellationToken);

    Task ConvertReservationsAsync(
        Guid orderId,
        DateTime nowUtc,
        CancellationToken cancellationToken);

    Task<PaymentTransaction?> GetLatestTransactionAsync(
        Guid orderId,
        PaymentProvider provider,
        CancellationToken cancellationToken);

    Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken cancellationToken);
}

