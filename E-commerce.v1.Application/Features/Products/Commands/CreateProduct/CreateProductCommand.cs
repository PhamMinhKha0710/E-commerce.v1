using E_commerce.v1.Domain.Enums;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    string? NameEn = null,
    string? DescriptionEn = null,
    string? Sku = null,
    string? Slug = null,
    decimal Discount = 0,
    string? Unit = null,
    string? UnitEn = null,
    bool IsActive = true,
    ProductType ProductType = ProductType.Single,
    ProductKind Kind = ProductKind.Sale,
    BuyType BuyType = BuyType.All,
    int Priority = 0,
    IReadOnlyList<string>? DocumentIds = null,
    int? StoreId = null) : IRequest<Guid>;
