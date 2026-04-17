using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Features.Shipping.Queries.GetShippingFee;
using E_commerce.v1.api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/shipping")]
[ApiController]
[Authorize]
public class ShippingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("fee")]
    public async Task<ActionResult<ShippingFeeResponse>> GetFee([FromBody] GetShippingFeeRequest body)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new GetShippingFeeQuery(userId, body));
        return Ok(result);
    }
}
