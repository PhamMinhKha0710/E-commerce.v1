using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Features.Shipping.Commands.ProcessAhamoveWebhook;

public class ProcessAhamoveWebhookCommandHandler : IRequestHandler<ProcessAhamoveWebhookCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AhamoveOptions _options;

    public ProcessAhamoveWebhookCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOptions<AhamoveOptions> options)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<Unit> Handle(ProcessAhamoveWebhookCommand request, CancellationToken cancellationToken)
    {
        var root = request.Payload;

        if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
            return Unit.Value;

        if (!string.IsNullOrEmpty(_options.WebhookApiKey))
        {
            if (!root.TryGetProperty("api_key", out var keyEl) || keyEl.GetString() != _options.WebhookApiKey)
                throw new BadRequestException("Webhook không hợp lệ.");
        }

        if (!root.TryGetProperty("_id", out var idEl))
            return Unit.Value;

        var ahamoveId = idEl.GetString();
        if (string.IsNullOrEmpty(ahamoveId))
            return Unit.Value;

        var status = root.TryGetProperty("status", out var stEl) ? stEl.GetString() : null;

        var order = await _orderRepository.GetOrderByAhamoveOrderIdAsync(ahamoveId, cancellationToken);
        if (order == null)
            return Unit.Value;

        order.AhamoveLastStatus = status;
        var mapped = AhamoveStatusMapper.TryMapToOrderStatus(status);
        if (mapped.HasValue)
            order.Status = mapped.Value;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
