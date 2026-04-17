using FluentValidation;

namespace E_commerce.v1.Application.Features.Shipping.Queries.GetShippingFee;

public class GetShippingFeeQueryValidator : AbstractValidator<GetShippingFeeQuery>
{
    public GetShippingFeeQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Body).NotNull();
        RuleFor(x => x.Body.Path.Count)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Cần ít nhất 1 điểm giao hàng (dropoff).");
        RuleFor(x => x.Body.Services.Count)
            .GreaterThan(0)
            .WithMessage("Cần ít nhất một dịch vụ (services).");
        RuleForEach(x => x.Body.Path).ChildRules(p =>
        {
            p.RuleFor(x => x.Address).NotEmpty();
        });
    }
}
