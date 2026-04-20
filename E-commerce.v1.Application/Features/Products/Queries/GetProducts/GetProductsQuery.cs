using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Domain.Enums;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int? Page = null,
    int? PageNumber = null,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? CategorySlug = null,
    string? Name = null,
    int? StoreId = null,
    bool? IsActive = null,
    ProductKind? Kind = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SearchTerm = null) : IRequest<PagedResult<ProductDto>>
{
    public int ResolvedPage
    {
        get
        {
            var p = Page ?? PageNumber;
            return p is > 0 ? p.Value : 1;
        }
    }
}
