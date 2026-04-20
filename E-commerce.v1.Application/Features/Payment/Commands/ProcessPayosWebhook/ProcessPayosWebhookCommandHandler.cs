using System.Data;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;

public class ProcessPayosWebhookCommandHandler : IRequestHandler<ProcessPayosWebhookCommand, Unit>
{
    private readonly IPayosWebhookVerifier _verifier;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAhamoveClient _ahamoveClient;
    private readonly AhamoveOptions _ahamoveOptions;

    public ProcessPayosWebhookCommandHandler(
        IPayosWebhookVerifier verifier,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IAhamoveClient ahamoveClient,
        IOptions<AhamoveOptions> ahamoveOptions)
    {
        _verifier = verifier;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _ahamoveClient = ahamoveClient;
        _ahamoveOptions = ahamoveOptions.Value;
    }

    public async Task<Unit> Handle(ProcessPayosWebhookCommand request, CancellationToken cancellationToken)
    {
        var evt = await _verifier.VerifyAndParseAsync(request.RawBody, cancellationToken);

        var order = await _orderRepository.GetOrderByPayosOrderCodeAsync(evt.OrderCode, cancellationToken);
        if (order == null)
            return Unit.Value;

        var newStatus = MapPayosStatus(evt.Status);
        if (!newStatus.HasValue)
            return Unit.Value;

        var now = DateTime.UtcNow;
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var o = await _orderRepository.GetOrderByPayosOrderCodeAsync(evt.OrderCode, ct);
            if (o == null) return;

            if (IsTerminal(o.PaymentStatus))
                return;

            // Amount validation (best-effort).
            var expected = (int)decimal.Round(o.GrandTotal, 0, MidpointRounding.AwayFromZero);
            if (evt.Amount > 0 && evt.Amount != expected)
                return;

            o.PaymentStatus = newStatus.Value;
            if (newStatus.Value == PaymentStatus.Paid)
                o.PaidAt = now;

            await _paymentRepository.AddTransactionAsync(new PaymentTransaction
            {
                OrderId = o.Id,
                Provider = PaymentProvider.PayOS,
                Status = newStatus.Value,
                Amount = o.GrandTotal,
                Currency = "VND",
                ProviderPaymentLinkId = evt.PaymentLinkId ?? o.PayosPaymentLinkId,
                ProviderOrderCode = evt.OrderCode,
                LastEventAt = now,
                LastEventIdempotencyKey = evt.RawDataId,
                RawLastWebhookPayload = request.RawBody
            }, ct);

            if (newStatus.Value == PaymentStatus.Paid)
            {
                await _paymentRepository.ConvertReservationsAsync(o.Id, now, ct);
            }
            else
            {
                await _paymentRepository.ReleaseReservedStockAsync(o.Id, now, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }, IsolationLevel.Serializable, cancellationToken);

        // Outside transaction: try to auto-create shipment if paid.
        if (newStatus.Value == PaymentStatus.Paid)
        {
            await TryAutoCreateShipmentAsync(order.Id, cancellationToken);
        }

        return Unit.Value;
    }

    private static PaymentStatus? MapPayosStatus(string status)
    {
        var s = (status ?? string.Empty).Trim().ToUpperInvariant();
        return s switch
        {
            "PAID" => PaymentStatus.Paid,
            "CANCELLED" => PaymentStatus.Cancelled,
            "CANCELED" => PaymentStatus.Cancelled,
            "EXPIRED" => PaymentStatus.Expired,
            "FAILED" => PaymentStatus.Failed,
            _ => null
        };
    }

    private static bool IsTerminal(PaymentStatus s) =>
        s is PaymentStatus.Paid or PaymentStatus.Failed or PaymentStatus.Expired or PaymentStatus.Cancelled;

    private async Task TryAutoCreateShipmentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
            if (order == null)
                return;
            if (!string.IsNullOrWhiteSpace(order.AhamoveOrderId))
                return;
            if (string.IsNullOrWhiteSpace(order.ShippingServiceId) ||
                order.ShippingLat == null || order.ShippingLng == null ||
                string.IsNullOrWhiteSpace(order.ShippingAddressLine))
                return;

            var pickup = _ahamoveOptions.Pickup;
            var path = new List<AhamovePathPoint>
            {
                new()
                {
                    Lat = pickup.Lat,
                    Lng = pickup.Lng,
                    Address = pickup.Address,
                    Name = pickup.Name,
                    Mobile = pickup.Mobile
                },
                new()
                {
                    Lat = order.ShippingLat.Value,
                    Lng = order.ShippingLng.Value,
                    Address = order.ShippingAddressLine,
                    Name = order.ShippingReceiverName,
                    Mobile = order.ShippingReceiverPhone,
                    Cod = order.PaymentMethod == PaymentMethod.Cod ? order.GrandTotal : null
                }
            };

            var result = await _ahamoveClient.CreateOrderAsync(new AhamoveCreateOrderRequest
            {
                OrderTime = 0,
                Path = path,
                ServiceId = order.ShippingServiceId,
                PaymentMethod = "BALANCE",
                Remarks = string.IsNullOrWhiteSpace(order.ShippingNote)
                    ? $"Order {order.OrderNumber}"
                    : $"{order.ShippingNote} | {order.OrderNumber}"
            }, cancellationToken);

            order.AhamoveOrderId = result.OrderId;
            order.AhamoveLastStatus = result.Status;

            var mapped = AhamoveStatusMapper.TryMapToOrderStatus(result.Status);
            if (mapped.HasValue)
                order.Status = mapped.Value;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // do not fail webhook due to shipping provider issues
        }
    }
}

