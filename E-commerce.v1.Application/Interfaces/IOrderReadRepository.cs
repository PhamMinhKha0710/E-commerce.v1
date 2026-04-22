using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Interfaces;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetByIdAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken);

    Task<PagedResult<OrderDto>> SearchMyOrdersAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PagedResult<OrderDto>> SearchAsync(
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

