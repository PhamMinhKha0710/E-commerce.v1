using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Review;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Reviews.Queries.GetProductReviews;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, ProductReviewsSummaryDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetProductReviewsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ProductReviewsSummaryDto> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var page = request.ResolvedPage;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        return await _reviewRepository.GetProductReviewsAsync(request.ProductId, page, pageSize, cancellationToken);
    }
}
