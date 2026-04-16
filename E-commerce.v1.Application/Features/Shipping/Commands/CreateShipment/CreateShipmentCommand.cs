using E_commerce.v1.Application.DTOs.Shipping;
using MediatR;

namespace E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;

public record CreateShipmentCommand(Guid UserId, Guid OrderId, CreateShipmentRequest Body) : IRequest<CreateShipmentResponse>;
