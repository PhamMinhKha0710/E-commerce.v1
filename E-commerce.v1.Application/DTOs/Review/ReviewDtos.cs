using E_commerce.v1.Application.DTOs.Common;

namespace E_commerce.v1.Application.DTOs.Review;

public class ReviewItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductReviewsSummaryDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public PagedResult<ReviewItemDto> Reviews { get; set; } = new();
}
