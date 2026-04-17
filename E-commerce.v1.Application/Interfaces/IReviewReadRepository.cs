using E_commerce.v1.Application.DTOs.Review;

namespace E_commerce.v1.Application.Interfaces;

public interface IReviewReadRepository
{
    Task<ProductReviewsSummaryDto> GetProductReviewsAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

