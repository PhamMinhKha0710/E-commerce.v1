using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;
using E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrderById;
using E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrders;
using E_commerce.v1.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/admin/orders")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> List(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new GetAdminOrdersQuery(status, fromDate, toDate, pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Chi tiết đơn cho admin: không filter theo <c>UserId</c> như endpoint public
    /// <c>api/v1/orders/{id}</c>, nên admin xem được đơn của bất kỳ khách nào.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAdminOrderByIdQuery(id));
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        await _mediator.Send(new UpdateOrderStatusCommand(id, request.Status));
        return NoContent();
    }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
