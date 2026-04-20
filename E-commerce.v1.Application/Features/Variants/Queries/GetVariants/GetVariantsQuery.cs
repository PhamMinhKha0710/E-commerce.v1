using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Queries.GetVariants;

public record GetVariantsQuery(
    Guid? ProductId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ProductVariantDto>>;
