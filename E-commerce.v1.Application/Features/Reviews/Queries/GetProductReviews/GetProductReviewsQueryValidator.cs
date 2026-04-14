using FluentValidation;

namespace E_commerce.v1.Application.Features.Reviews.Queries.GetProductReviews;

public class GetProductReviewsQueryValidator : AbstractValidator<GetProductReviewsQuery>
{
    public GetProductReviewsQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID không hợp lệ.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize phải trong khoảng từ 1 đến 100.");
    }
}
