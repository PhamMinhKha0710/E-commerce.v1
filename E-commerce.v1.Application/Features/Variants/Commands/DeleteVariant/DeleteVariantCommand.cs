using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Commands.DeleteVariant;

public record DeleteVariantCommand(Guid Id) : IRequest;

