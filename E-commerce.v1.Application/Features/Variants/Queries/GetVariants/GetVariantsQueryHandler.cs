using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Queries.GetVariants;

public class GetVariantsQueryHandler : IRequestHandler<GetVariantsQuery, PagedResult<ProductVariantDto>>
{
    private readonly IVariantReadRepository _repository;

    public GetVariantsQueryHandler(IVariantReadRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<ProductVariantDto>> Handle(GetVariantsQuery request, CancellationToken cancellationToken)
    {
        return _repository.SearchAsync(request.ProductId, request.IsActive, request.PageNumber, request.PageSize, cancellationToken);
    }
}
