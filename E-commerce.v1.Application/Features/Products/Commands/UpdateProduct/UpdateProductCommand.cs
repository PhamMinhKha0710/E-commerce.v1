using E_commerce.v1.Domain.Enums;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    string? NameEn = null,
    string? DescriptionEn = null,
    string? Sku = null,
    string? Slug = null,
    decimal? Discount = null,
    string? Unit = null,
    string? UnitEn = null,
    bool? IsActive = null,
    ProductType? ProductType = null,
    ProductKind? Kind = null,
    BuyType? BuyType = null,
    int? Priority = null,
    IReadOnlyList<string>? DocumentIds = null,
    int? StoreId = null) : IRequest<bool>;
