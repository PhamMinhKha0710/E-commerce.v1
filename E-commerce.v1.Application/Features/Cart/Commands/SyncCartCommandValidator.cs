using FluentValidation;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class SyncCartCommandValidator : AbstractValidator<SyncCartCommand>
{
    public SyncCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID không được để trống.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Danh sách sản phẩm không được null.");

        RuleForEach(x => x.Items)
            .ChildRules(line =>
            {
                line.RuleFor(l => l.ProductId)
                    .NotEmpty().WithMessage("ProductId không được để trống.");
                line.RuleFor(l => l.Quantity)
                    .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
            });
    }
}
