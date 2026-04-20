using E_commerce.v1.Domain.Enums;
using FluentValidation;

namespace E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrders;

public class GetAdminOrdersQueryValidator : AbstractValidator<GetAdminOrdersQuery>
{
    public GetAdminOrdersQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is null || Enum.IsDefined(typeof(OrderStatus), s.Value))
            .WithMessage("Trạng thái đơn hàng không hợp lệ.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(x => x)
            .Must(q => !q.FromDate.HasValue || !q.ToDate.HasValue || q.FromDate.Value <= q.ToDate.Value)
            .WithMessage("FromDate phải nhỏ hơn hoặc bằng ToDate.");
    }
}
