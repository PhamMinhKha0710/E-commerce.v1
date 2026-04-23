using E_commerce.v1.Application.Features.Shipping.Commands.ProcessAhamoveWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/webhooks/ahamove")]
[ApiController]
[AllowAnonymous]
public class AhamoveWebhookController : ControllerBase
{
    private readonly IMediator _mediator;

    public AhamoveWebhookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [EnableRateLimiting("WebhooksPerIp")]
    [RequestSizeLimit(128 * 1024)] // 128 KB
    public async Task<IActionResult> Post([FromBody] JsonElement payload)
    {
        await _mediator.Send(new ProcessAhamoveWebhookCommand(payload));
        return Ok();
    }
}
