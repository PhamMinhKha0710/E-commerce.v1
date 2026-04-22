using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Features.Order.Commands.CancelOrder;
using E_commerce.v1.Application.Features.Order.Queries.GetMyOrders;
using E_commerce.v1.Application.Features.Order.Queries.GetOrderById;
using E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;
using E_commerce.v1.api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/orders")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new GetMyOrdersQuery(userId, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new GetOrderByIdQuery(userId, id));
        return Ok(result);
    }

    [HttpPost("{orderId:guid}/shipment")]
    public async Task<ActionResult<CreateShipmentResponse>> CreateShipment(Guid orderId, [FromBody] CreateShipmentRequest body)
        => await CreateShipmentInternal(orderId, body);

    private async Task<ActionResult<CreateShipmentResponse>> CreateShipmentInternal(Guid orderId, CreateShipmentRequest body)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new CreateShipmentCommand(userId, orderId, body));
        return Ok(result);
    }

    /// <summary>
    /// Huỷ đơn: chỉ owner mới được huỷ, và chỉ cho phép khi đơn ở trạng thái
    /// Pending/Confirmed và chưa có payment thành công (business rule).
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new CancelOrderCommand(userId, id));
        return NoContent();
    }
}
