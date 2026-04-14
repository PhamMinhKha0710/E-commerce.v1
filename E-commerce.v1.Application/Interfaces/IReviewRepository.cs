using E_commerce.v1.Application.DTOs.Review;

namespace E_commerce.v1.Application.Interfaces;

public interface IReviewRepository
{
    Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken);
    Task<ProductReviewsSummaryDto> GetProductReviewsAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
}
