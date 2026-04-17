using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.DTOs.Shipping;
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
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new GetMyOrdersQuery(userId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new GetOrderByIdQuery(userId, id));
        return Ok(result);
    }

    [HttpPost("{orderId:guid}/create-shipment")]
    public async Task<ActionResult<CreateShipmentResponse>> CreateShipment(Guid orderId, [FromBody] CreateShipmentRequest body)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new CreateShipmentCommand(userId, orderId, body));
        return Ok(result);
    }
}
