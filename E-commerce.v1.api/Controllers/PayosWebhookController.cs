using System.Text;
using System.Text.Json;
using E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[ApiController]
[AllowAnonymous]
public class PayosWebhookController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayosWebhookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Webhook callback từ PayOS.</summary>
    [HttpPost("/api/v1/webhooks/payos")]
    public Task<IActionResult> PostNew([FromBody] JsonElement payload) => PostInternal(payload);

    /// <summary>[Deprecated] Dùng <c>POST /api/v1/webhooks/payos</c>.</summary>
    [HttpPost("/api/v1/payment/payos/webhook")]
    [Obsolete("Use POST /api/v1/webhooks/payos instead.")]
    public Task<IActionResult> Post([FromBody] JsonElement payload) => PostInternal(payload);

    private async Task<IActionResult> PostInternal(JsonElement payload)
    {
        // Cần raw JSON để verify signature; tránh re-serialize làm sai hash.
        var raw = payload.ValueKind == JsonValueKind.Undefined ? string.Empty : payload.GetRawText();
        if (string.IsNullOrWhiteSpace(raw))
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            raw = await reader.ReadToEndAsync();
        }
        await _mediator.Send(new ProcessPayosWebhookCommand(raw));
        return Ok();
    }
}
