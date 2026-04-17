using System.Text.Json;
using E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;
using E_commerce.v1.Application.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayOS.Crypto;

namespace E_commerce.v1.api.Controllers;

/// <summary>
/// Dev-only helper to generate a valid signed PayOS webhook body for local testing
/// and optionally trigger the webhook pipeline in one step.
/// Available only in Development environment.
/// </summary>
[ApiController]
[Route("api/v1/dev/payos/webhook-simulator")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DevPayosWebhookSimulatorController : ControllerBase
{
    private readonly IHostEnvironment _env;
    private readonly PayosOptions _options;
    private readonly IMediator _mediator;

    public DevPayosWebhookSimulatorController(
        IHostEnvironment env,
        IOptions<PayosOptions> options,
        IMediator mediator)
    {
        _env = env;
        _options = options.Value;
        _mediator = mediator;
    }

    [HttpPost("generate")]
    public IActionResult Generate([FromBody] DevPayosWebhookGenerateRequest request)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        if (request.OrderCode <= 0)
            return BadRequest(new { Message = "orderCode phải > 0" });

        var (data, signature) = BuildSignedEnvelope(request);
        return Ok(new { data, signature });
    }

    /// <summary>
    /// One-step helper: takes flat fields, signs them internally with Payos:ChecksumKey,
    /// then runs the normal webhook pipeline. Intended for local/dev testing only.
    /// </summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(
        [FromBody] DevPayosWebhookGenerateRequest request,
        CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        if (request.OrderCode <= 0)
            return BadRequest(new { Message = "orderCode phải > 0" });
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(new { Message = "status là bắt buộc" });

        var (data, signature) = BuildSignedEnvelope(request);
        var raw = JsonSerializer.Serialize(new { data, signature });

        await _mediator.Send(new ProcessPayosWebhookCommand(raw), cancellationToken);

        return Ok(new
        {
            triggered = true,
            sent = new { data, signature }
        });
    }

    private (JsonElement Data, string Signature) BuildSignedEnvelope(DevPayosWebhookGenerateRequest request)
    {
        var data = JsonSerializer.SerializeToElement(new
        {
            orderCode = request.OrderCode,
            amount = request.Amount,
            status = request.Status?.Trim() ?? string.Empty,
            paymentLinkId = request.PaymentLinkId,
            id = string.IsNullOrWhiteSpace(request.Id) ? $"evt-dev-{Guid.NewGuid():N}" : request.Id.Trim()
        });

        var signature = new CryptoProvider().CreateSignatureFromObject(data, _options.ChecksumKey) ?? string.Empty;
        return (data, signature);
    }
}

public sealed class DevPayosWebhookGenerateRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Status { get; set; } = "PAID";
    public string? PaymentLinkId { get; set; }
    public string? Id { get; set; }
}
