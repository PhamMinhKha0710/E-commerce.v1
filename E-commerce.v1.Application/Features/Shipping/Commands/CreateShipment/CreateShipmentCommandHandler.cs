using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, CreateShipmentResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAhamoveClient _ahamoveClient;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShipmentCommandHandler(
        IOrderRepository orderRepository,
        IAhamoveClient ahamoveClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _ahamoveClient = ahamoveClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateShipmentResponse> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
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

        return new CreateShipmentResponse
        {
            AhamoveOrderId = result.OrderId,
            Status = result.Status,
            SharedLink = result.SharedLink
        };
    }
}
