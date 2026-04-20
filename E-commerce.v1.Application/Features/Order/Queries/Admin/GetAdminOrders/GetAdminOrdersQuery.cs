using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Domain.Enums;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrders;

public record GetAdminOrdersQuery(
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<OrderDto>>;
