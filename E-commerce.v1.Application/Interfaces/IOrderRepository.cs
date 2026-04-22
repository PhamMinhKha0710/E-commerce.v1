using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<Order?> GetOrderByAhamoveOrderIdAsync(string ahamoveOrderId, CancellationToken cancellationToken);

    Task<Order?> GetOrderByPayosOrderCodeAsync(long orderCode, CancellationToken cancellationToken);

    Task<(List<Order> Items, int Total)> GetMyOrdersAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<Order?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken cancellationToken);
}
