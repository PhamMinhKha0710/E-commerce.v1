using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;

namespace E_commerce.v1.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken)
    {
        var productExists = await _reviewRepository.ProductExistsAsync(productId, cancellationToken);
        if (!productExists) throw new NotFoundException("Sản phẩm không tồn tại.");

        var hasCompletedPurchase = await _reviewRepository.HasCompletedPurchaseAsync(userId, productId, cancellationToken);
        if (!hasCompletedPurchase) throw new BadRequestException("Bạn chỉ có thể đánh giá sản phẩm đã mua và nhận thành công.");

        var id = await _reviewRepository.UpsertReviewAsync(userId, productId, rating, comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return id;
    }
}

