using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;

namespace E_commerce.v1.Application.Interfaces;

public interface IProductReadRepository
{
    Task<PagedResult<ProductDto>> GetListAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        string? searchTerm,
        CancellationToken cancellationToken);

    Task<ProductDetailDto?> GetDetailAsync(Guid productId, CancellationToken cancellationToken);
}

