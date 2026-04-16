using E_commerce.v1.Application.DTOs.Order;

namespace E_commerce.v1.Application.Interfaces;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetByIdAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken);

    Task<List<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken);
}

