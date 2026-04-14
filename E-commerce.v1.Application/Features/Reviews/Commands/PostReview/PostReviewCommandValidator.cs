using FluentValidation;

namespace E_commerce.v1.Application.Features.Reviews.Commands.PostReview;

public class PostReviewCommandValidator : AbstractValidator<PostReviewCommand>
{
    public PostReviewCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID không hợp lệ.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID không hợp lệ.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating phải trong khoảng từ 1 đến 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Bình luận không được vượt quá 1000 ký tự.");
    }
}
