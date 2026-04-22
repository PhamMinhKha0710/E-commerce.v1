using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Application.Common.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpsertReview requested. UserId={UserId}, ProductId={ProductId}, Rating={Rating}", userId, productId, rating);
        var productExists = await _reviewRepository.ProductExistsAsync(productId, cancellationToken);
        if (!productExists) throw new NotFoundException("Sản phẩm không tồn tại.");

        var hasCompletedPurchase = await _reviewRepository.HasCompletedPurchaseAsync(userId, productId, cancellationToken);
        if (!hasCompletedPurchase) throw new BadRequestException("Bạn chỉ có thể đánh giá sản phẩm đã mua và nhận thành công.");

        var id = await _reviewRepository.UpsertReviewAsync(userId, productId, rating, comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UpsertReview completed. ReviewId={ReviewId}", id);
        return id;
    }
}

