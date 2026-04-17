using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;
using E_commerce.v1.Application.Features.Coupons.Commands.CreateCoupon;
using E_commerce.v1.Application.Features.Coupons.Queries.GetCouponById;
using E_commerce.v1.Application.Features.Coupons.Queries.GetCoupons;
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

    [HttpGet]
    public async Task<ActionResult<PagedResult<CouponDto>>> List(
        [FromQuery] string? code = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetCouponsQuery(code, isActive, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CouponDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCouponByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCoupon([FromBody] CreateCouponCommand command)
    {
        var couponId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = couponId }, couponId);
    }
}
