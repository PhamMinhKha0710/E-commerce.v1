using E_commerce.v1.Application.DTOs.Review;
using MediatR;

namespace E_commerce.v1.Application.Features.Reviews.Queries.GetProductReviews;

public record GetProductReviewsQuery(
    Guid ProductId,
    int? Page = null,
    int? PageNumber = null,
    int PageSize = 10) : IRequest<ProductReviewsSummaryDto>
{
    public int ResolvedPage
    {
        get
        {
            var page = Page ?? PageNumber;
            return page is > 0 ? page.Value : 1;
        }
    }
}
