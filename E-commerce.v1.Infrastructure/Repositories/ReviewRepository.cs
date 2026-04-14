using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Review;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken)
    {
        var productExists = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists) throw new NotFoundException("Sản phẩm không tồn tại.");

        var hasCompletedPurchase = await _context.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.ProductId == productId && oi.Order != null && oi.Order.UserId == userId && oi.Order.Status == OrderStatus.Completed, cancellationToken);
        if (!hasCompletedPurchase) throw new BadRequestException("Bạn chỉ có thể đánh giá sản phẩm đã mua và nhận thành công.");

        var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
        if (existingReview == null)
        {
            existingReview = new Review { ProductId = productId, UserId = userId, Rating = rating, Comment = comment?.Trim() };
            _context.Reviews.Add(existingReview);
        }
        else
        {
            existingReview.Rating = rating;
            existingReview.Comment = comment?.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existingReview.Id;
    }

    public async Task<ProductReviewsSummaryDto> GetProductReviewsAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var productExists = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists) throw new NotFoundException("Sản phẩm không tồn tại.");

        var reviewsQuery = _context.Reviews.AsNoTracking().Where(r => r.ProductId == productId);
        var totalReviews = await reviewsQuery.CountAsync(cancellationToken);
        var averageRatingRaw = totalReviews > 0 ? await reviewsQuery.AverageAsync(r => (double)r.Rating, cancellationToken) : 0d;

        var items = await reviewsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            Reviews = new PagedResult<ReviewItemDto> { Items = items, TotalCount = totalReviews, PageNumber = page, PageSize = pageSize }
        };
    }
}
