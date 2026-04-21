using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Shipping;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, CreateShipmentResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAhamoveClient _ahamoveClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AhamoveOptions _options;
    private readonly ILogger<CreateShipmentCommandHandler> _logger;

    public CreateShipmentCommandHandler(
        IOrderRepository orderRepository,
        IAhamoveClient ahamoveClient,
        IUnitOfWork unitOfWork,
        Microsoft.Extensions.Options.IOptions<AhamoveOptions> options,
        ILogger<CreateShipmentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _ahamoveClient = ahamoveClient;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CreateShipmentResponse> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateShipment requested. OrderId={OrderId}, UserId={UserId}", request.OrderId, request.UserId);
        var order = await _orderRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Không tìm thấy đơn hàng.");

        if (order.UserId != request.UserId)
            throw new NotFoundException("Không tìm thấy đơn hàng.");

        if (!string.IsNullOrEmpty(order.AhamoveOrderId))
            throw new BadRequestException("Đơn hàng đã có vận đơn Ahamove.");

        var path = request.Body.Path
            .Select(p => new AhamovePathPoint
            {
                Lat = p.Lat,
                Lng = p.Lng,
                Address = p.Address,
                Name = p.Name,
                Mobile = p.Mobile,
                Cod = p.Cod
            })
            .ToList();

        // FE có thể chỉ gửi điểm dropoff; server tự prepend pickup cấu hình sẵn để tạo path hợp lệ.
        if (path.Count == 1)
        {
            var p = _options.Pickup;
            path.Insert(0, new AhamovePathPoint
            {
                Lat = p.Lat,
                Lng = p.Lng,
                Address = p.Address,
                Name = p.Name,
                Mobile = p.Mobile
            });
        }

        if (order.PaymentMethod == PaymentMethod.Cod && path.Count >= 2)
            path[^1].Cod = order.GrandTotal;

        var createRequest = new AhamoveCreateOrderRequest
        {
            OrderTime = request.Body.OrderTime,
            Path = path,
            ServiceId = request.Body.ServiceId,
            Items = request.Body.Items,
            RouteOptimized = request.Body.RouteOptimized,
            PaymentMethod = request.Body.AhamovePaymentMethod,
            PromoCode = request.Body.PromoCode,
            Remarks = string.IsNullOrWhiteSpace(request.Body.Remarks)
                ? $"Order {order.OrderNumber}"
                : $"{request.Body.Remarks} | {order.OrderNumber}"
        };

        var result = await _ahamoveClient.CreateOrderAsync(createRequest, cancellationToken);

        order.AhamoveOrderId = result.OrderId;
        order.AhamoveLastStatus = result.Status;
        var mapped = AhamoveStatusMapper.TryMapToOrderStatus(result.Status);
        if (mapped.HasValue)
            order.Status = mapped.Value;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Shipment created via Ahamove. OrderId={OrderId}, AhamoveOrderId={AhamoveOrderId}", order.Id, result.OrderId);

        return new CreateShipmentResponse
        {
            AhamoveOrderId = result.OrderId,
            Status = result.Status,
            SharedLink = result.SharedLink
        };
    }
}
