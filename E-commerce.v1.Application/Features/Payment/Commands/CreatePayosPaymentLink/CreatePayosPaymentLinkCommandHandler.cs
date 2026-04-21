using System.Data;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Payments;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Features.Payment.Commands.CreatePayosPaymentLink;

public class CreatePayosPaymentLinkCommandHandler : IRequestHandler<CreatePayosPaymentLinkCommand, CreatePayosPaymentLinkResponse>
{
    private const long PayosMaxSafeOrderCode = 9_007_199_254_740_991L;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayosClient _payosClient;
    private readonly PayosOptions _payosOptions;
    private readonly ILogger<CreatePayosPaymentLinkCommandHandler> _logger;

    public CreatePayosPaymentLinkCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IPayosClient payosClient,
        IOptions<PayosOptions> payosOptions,
        ILogger<CreatePayosPaymentLinkCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _payosClient = payosClient;
        _payosOptions = payosOptions.Value;
        _logger = logger;
    }

    public async Task<CreatePayosPaymentLinkResponse> Handle(CreatePayosPaymentLinkCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreatePayosPaymentLink requested. OrderId={OrderId}, UserId={UserId}", request.OrderId, request.UserId);
        var order = await _orderRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        if (order.UserId != request.UserId) throw new NotFoundException("Không tìm thấy đơn hàng.");

        if (order.PaymentMethod != PaymentMethod.EWallet)
            throw new BadRequestException("Đơn hàng này không hỗ trợ thanh toán PayOS.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new BadRequestException("Đơn hàng đã được thanh toán.");

        if (request.ClientTotalAmount.HasValue && request.ClientTotalAmount.Value != order.GrandTotal)
            throw new BadRequestException("Tổng tiền không khớp với đơn hàng.");

        if (!string.IsNullOrWhiteSpace(order.PayosPaymentLinkId))
        {
            // If a link already exists, return it as-is (client can decide to reuse).
            _logger.LogInformation("PayOS payment link already exists. OrderId={OrderId}", order.Id);
            return new CreatePayosPaymentLinkResponse
            {
                OrderId = order.Id,
                PaymentUrl = string.Empty,
                PaymentLinkId = order.PayosPaymentLinkId,
                OrderCode = order.PayosOrderCode
            };
        }

        var now = DateTime.UtcNow;
        var orderCode = (order.PayosOrderCode is > 0 and <= PayosMaxSafeOrderCode)
            ? order.PayosOrderCode.Value
            : GenerateOrderCode(now, order.Id);
        order.PayosOrderCode = orderCode;

        // 1) Reserve stock & persist reservation rows (Serializable transaction).
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _paymentRepository.ReserveStockForOrderAsync(
                order,
                now,
                TimeSpan.FromMinutes(Math.Max(1, _payosOptions.ReservationTtlMinutes)),
                ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, IsolationLevel.Serializable, cancellationToken);

        // 2) Create PayOS payment link.
        PayosCreatePaymentLinkResult payosResult;
        try
        {
            payosResult = await _payosClient.CreatePaymentLinkAsync(new PayosCreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)decimal.Round(order.GrandTotal, 0, MidpointRounding.AwayFromZero),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? $"Thanh toán {order.OrderNumber}"
                    : request.Description.Trim(),
                ReturnUrl = _payosOptions.ReturnUrl,
                CancelUrl = _payosOptions.CancelUrl
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PayOS payment link. OrderId={OrderId}", order.Id);
            // Compensate: release reservation & restore stock if PayOS call fails.
            await ReleaseReservationsAndRestoreStockAsync(order.Id, now, cancellationToken);
            throw;
        }

        if (string.IsNullOrWhiteSpace(payosResult.CheckoutUrl))
        {
            await ReleaseReservationsAndRestoreStockAsync(order.Id, now, cancellationToken);
            throw new BadRequestException("Không tạo được link thanh toán PayOS.");
        }

        // 3) Persist payment transaction + link identifiers.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var o = await _orderRepository.GetOrderByIdAsync(order.Id, ct);
            if (o == null) throw new NotFoundException("Không tìm thấy đơn hàng.");

            o.PayosPaymentLinkId = payosResult.PaymentLinkId;
            o.PayosOrderCode = payosResult.OrderCode ?? orderCode;
            o.PaymentStatus = PaymentStatus.Pending;

            await _paymentRepository.AddTransactionAsync(new PaymentTransaction
            {
                OrderId = o.Id,
                Provider = PaymentProvider.PayOS,
                Status = PaymentStatus.Pending,
                Amount = o.GrandTotal,
                Currency = "VND",
                ProviderPaymentLinkId = payosResult.PaymentLinkId,
                ProviderOrderCode = payosResult.OrderCode ?? orderCode,
                LastEventAt = now
            }, ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }, IsolationLevel.ReadCommitted, cancellationToken);

        return new CreatePayosPaymentLinkResponse
        {
            OrderId = order.Id,
            PaymentUrl = payosResult.CheckoutUrl,
            PaymentLinkId = payosResult.PaymentLinkId,
            OrderCode = payosResult.OrderCode ?? orderCode
        };
    }

    private static long GenerateOrderCode(DateTime utcNow, Guid orderId)
    {
        // PayOS SDK enforces JS "max safe integer" limit (<= 9,007,199,254,740,991).
        // Keep it well below that while retaining time ordering + low collision risk.
        //
        // 10-digit timePart (yyMMddHHmm) * 100000 + 5-digit suffix => up to ~ 9.9e14.
        var timePart = long.Parse(utcNow.ToString("yyMMddHHmm"));
        var suffix = (long)(Math.Abs(orderId.GetHashCode()) % 100000);
        return (timePart * 100000L) + suffix;
    }

    private async Task ReleaseReservationsAndRestoreStockAsync(Guid orderId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _paymentRepository.ReleaseReservedStockAsync(orderId, nowUtc, ct);

            // Best-effort: mark order payment as cancelled/failed to prevent loops.
            var order = await _orderRepository.GetOrderByIdAsync(orderId, ct);
            if (order != null && order.PaymentStatus == PaymentStatus.Pending)
                order.PaymentStatus = PaymentStatus.Cancelled;

            await _unitOfWork.SaveChangesAsync(ct);
        }, IsolationLevel.Serializable, cancellationToken);
    }
}

