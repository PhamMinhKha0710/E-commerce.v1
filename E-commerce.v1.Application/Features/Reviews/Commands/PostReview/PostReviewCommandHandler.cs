using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Reviews.Commands.PostReview;

public class PostReviewCommandHandler : IRequestHandler<PostReviewCommand, Guid>
{
    private readonly IReviewService _reviewService;

    public PostReviewCommandHandler(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public async Task<Guid> Handle(PostReviewCommand request, CancellationToken cancellationToken)
    {
        return await _reviewService.UpsertReviewAsync(
            request.UserId,
            request.ProductId,
            request.Rating,
            request.Comment,
            cancellationToken);
    }
}
