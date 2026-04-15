using FluentValidation;

namespace E_commerce.v1.Application.Features.Variants.Commands.UpdateVariant;

public class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
{
    public UpdateVariantCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id không được để trống.");

        RuleFor(v => v.Sku)
            .NotEmpty().WithMessage("SKU không được để trống.")
            .MaximumLength(64).WithMessage("SKU tối đa 64 ký tự.");

        RuleFor(v => v.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price phải >= 0.");

        RuleFor(v => v.Inventory)
            .GreaterThanOrEqualTo(0).WithMessage("Inventory phải >= 0.");

        RuleForEach(v => v.Options).ChildRules(opt =>
        {
            opt.RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Option key không được để trống.")
                .MaximumLength(64).WithMessage("Option key tối đa 64 ký tự.");
            opt.RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Option value không được để trống.")
                .MaximumLength(128).WithMessage("Option value tối đa 128 ký tự.");
        });
    }
}

