using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Reviews.Commands.PostReview;

public class PostReviewCommandHandler : IRequestHandler<PostReviewCommand, Guid>
{
    private readonly IReviewRepository _reviewRepository;

    public PostReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Guid> Handle(PostReviewCommand request, CancellationToken cancellationToken)
    {
        return await _reviewRepository.UpsertReviewAsync(
            request.UserId,
            request.ProductId,
            request.Rating,
            request.Comment,
            cancellationToken);
    }
}
