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

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
    {
        return _context.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);
    }

    public Task<bool> HasCompletedPurchaseAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        return _context.OrderItems
            .AsNoTracking()
            .AnyAsync(
                oi => oi.ProductId == productId &&
                      oi.Order != null &&
                      oi.Order.UserId == userId &&
                      oi.Order.Status == OrderStatus.Completed,
                cancellationToken);
    }

    public async Task<Guid> UpsertReviewAsync(Guid userId, Guid productId, int rating, string? comment, CancellationToken cancellationToken)
    {
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

        return existingReview.Id;
    }
}
