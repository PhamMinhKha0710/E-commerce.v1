using E_commerce.v1.Application.Shipping;

namespace E_commerce.v1.Application.Interfaces;

public interface IAhamoveClient
{
    Task<IReadOnlyList<AhamoveEstimateResultItem>> EstimateAsync(
        AhamoveEstimateRequest request,
        CancellationToken cancellationToken = default);

    Task<AhamoveCreateOrderResponse> CreateOrderAsync(
        AhamoveCreateOrderRequest request,
        CancellationToken cancellationToken = default);
}
