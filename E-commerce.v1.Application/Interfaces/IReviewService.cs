namespace E_commerce.v1.Application.Interfaces;

public interface IReviewService
{
    Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken);
}

