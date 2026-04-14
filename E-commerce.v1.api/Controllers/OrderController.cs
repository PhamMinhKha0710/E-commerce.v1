using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.Features.Order.Queries.GetMyOrders;
using E_commerce.v1.Application.Features.Order.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        var userId = GetUserIdFromToken();
        var result = await _mediator.Send(new GetMyOrdersQuery(userId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = GetUserIdFromToken();
        var result = await _mediator.Send(new GetOrderByIdQuery(userId, id));
        return Ok(result);
    }

    private Guid GetUserIdFromToken()
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("Unable to extract user ID from token.");

        return userId;
    }
}
