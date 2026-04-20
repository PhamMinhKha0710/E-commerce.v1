using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Shipping.Commands.SyncShipmentStatus;

public class SyncShipmentStatusCommandHandler : IRequestHandler<SyncShipmentStatusCommand, SyncShipmentStatusResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAhamoveClient _ahamoveClient;

    public SyncShipmentStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IAhamoveClient ahamoveClient)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _ahamoveClient = ahamoveClient;
    }

    public async Task<SyncShipmentStatusResponse> Handle(SyncShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Order not found.");

        if (request.UserId != Guid.Empty && order.UserId != request.UserId)
            throw new UnauthorizedAccessException("You don't have permission to sync this order.");

        if (string.IsNullOrWhiteSpace(order.AhamoveOrderId))
            throw new BadRequestException("This order has not been shipped via Ahamove yet.");

        var ahamoveResponse = await _ahamoveClient.GetOrderDetailsAsync(order.AhamoveOrderId, cancellationToken);
        var ahamoveStatus = ahamoveResponse.Status;

        var mappedDomainStatus = AhamoveStatusMapper.TryMapToOrderStatus(ahamoveStatus);

        bool updated = false;

        if (mappedDomainStatus.HasValue && order.Status != mappedDomainStatus.Value)
        {
            order.Status = mappedDomainStatus.Value;
            updated = true;
        }

        if (order.AhamoveLastStatus != ahamoveStatus)
        {
            order.AhamoveLastStatus = ahamoveStatus;
            updated = true;
        }

        if (updated)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new SyncShipmentStatusResponse
        {
            DomainStatus = order.Status.ToString(),
            RawAhamoveStatus = ahamoveStatus,
            SharedLink = ahamoveResponse.SharedLink
        };
    }
}
