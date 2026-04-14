using E_commerce.v1.Application.Features.Coupons.Commands.CreateCoupon;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/admin/coupons")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminCouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCoupon([FromBody] CreateCouponCommand command)
    {
        var couponId = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateCoupon), new { id = couponId }, couponId);
    }
}
