using E_commerce.v1.Application.DTOs.Product;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Commands.CreateVariant;

public record CreateVariantCommand(
    Guid ProductId,
    string Sku,
    decimal Price,
    int Inventory,
    bool IsActive,
    IReadOnlyList<ProductVariantOptionDto>? Options = null) : IRequest<Guid>;

