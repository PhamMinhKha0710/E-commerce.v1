using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;

namespace E_commerce.v1.Application.Interfaces;

public interface IVariantReadRepository
{
    Task<PagedResult<ProductVariantDto>> SearchAsync(
        Guid? productId,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductVariantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
