using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductReadRepository _productReadRepository;

    public GetProductsQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productReadRepository.GetListAsync(
            page: request.ResolvedPage,
            pageSize: request.PageSize,
            categoryId: request.CategoryId,
            searchTerm: !string.IsNullOrWhiteSpace(request.Name) ? request.Name : request.SearchTerm,
            cancellationToken: cancellationToken);
    }
}
