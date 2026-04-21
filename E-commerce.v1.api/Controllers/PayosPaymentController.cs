using E_commerce.v1.Application.Features.Payment.Commands.CreatePayosPaymentLink;
using E_commerce.v1.api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/payment/payos")]
[ApiController]
[Authorize]
public class PayosPaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayosPaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePayosPaymentLinkResponse>> Create([FromBody] CreatePayosPaymentLinkRequest request)
        => await CreateInternal(request);

    /// <summary>[Deprecated] Dùng <c>POST api/v1/payment/payos</c>.</summary>
    [HttpPost("create")]
    [Obsolete("Use POST api/v1/payment/payos instead.")]
    public async Task<ActionResult<CreatePayosPaymentLinkResponse>> CreateLegacy([FromBody] CreatePayosPaymentLinkRequest request)
        => await CreateInternal(request);

    private async Task<ActionResult<CreatePayosPaymentLinkResponse>> CreateInternal(CreatePayosPaymentLinkRequest request)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new CreatePayosPaymentLinkCommand(
            userId,
            request.OrderId,
            request.TotalAmount,
            request.Description));
        return Ok(result);
    }
}

public sealed class CreatePayosPaymentLinkRequest
{
    public Guid OrderId { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Description { get; set; }
}

