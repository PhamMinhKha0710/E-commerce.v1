using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Review;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class ReviewReadRepository : IReviewReadRepository
{
    private readonly AppDbContext _context;

    public ReviewReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductReviewsSummaryDto> GetProductReviewsAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var productExists = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists) throw new NotFoundException("Sản phẩm không tồn tại.");

        var resolvedPage = page > 0 ? page : 1;
        var resolvedPageSize = pageSize > 0 ? pageSize : 10;

        var reviewsQuery = _context.Reviews.AsNoTracking().Where(r => r.ProductId == productId);
        var totalReviews = await reviewsQuery.CountAsync(cancellationToken);
        var averageRatingRaw = totalReviews > 0
            ? await reviewsQuery.AverageAsync(r => (double)r.Rating, cancellationToken)
            : 0d;

        var items = await reviewsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((resolvedPage - 1) * resolvedPageSize)
            .Take(resolvedPageSize)
            .Select(r => new ReviewItemDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ReviewerName = ((r.User.FirstName ?? string.Empty) + " " + (r.User.LastName ?? string.Empty)).Trim(),
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new ProductReviewsSummaryDto
        {
            AverageRating = Math.Round((decimal)averageRatingRaw, 2, MidpointRounding.AwayFromZero),
            TotalReviews = totalReviews,
            Reviews = new PagedResult<ReviewItemDto>
            {
                Items = items,
                TotalCount = totalReviews,
                PageNumber = resolvedPage,
                PageSize = resolvedPageSize
            }
        };
    }
}

