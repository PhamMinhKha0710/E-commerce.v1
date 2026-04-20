using E_commerce.v1.Application.DTOs.Shipping;
using MediatR;

namespace E_commerce.v1.Application.Features.Shipping.Commands.SyncShipmentStatus;

public record SyncShipmentStatusCommand(Guid UserId, Guid OrderId) : IRequest<SyncShipmentStatusResponse>;
