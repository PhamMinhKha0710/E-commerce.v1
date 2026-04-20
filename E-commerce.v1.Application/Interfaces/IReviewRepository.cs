using E_commerce.v1.Application.DTOs.Review;

namespace E_commerce.v1.Application.Interfaces;

public interface IReviewRepository
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken);

    Task<bool> HasCompletedPurchaseAsync(Guid userId, Guid productId, CancellationToken cancellationToken);

    Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken);
}
