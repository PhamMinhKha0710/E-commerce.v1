using E_commerce.v1.Application.DTOs.Product;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Commands.UpdateVariant;

public record UpdateVariantCommand(
    Guid Id,
    string Sku,
    decimal Price,
    int Inventory,
    bool IsActive,
    IReadOnlyList<ProductVariantOptionDto>? Options = null) : IRequest;

