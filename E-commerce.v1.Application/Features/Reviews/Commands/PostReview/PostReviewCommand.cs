using MediatR;

namespace E_commerce.v1.Application.Features.Reviews.Commands.PostReview;

public class PostReviewCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public Guid UserId { get; set; }
}
