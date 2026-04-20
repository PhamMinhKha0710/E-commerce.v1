using FluentValidation;

namespace E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Body).NotNull();
        RuleFor(x => x.Body.Path.Count)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Cần ít nhất 1 điểm giao hàng.");
        RuleFor(x => x.Body.ServiceId)
            .NotEmpty()
            .WithMessage("serviceId là bắt buộc.");
        RuleForEach(x => x.Body.Path).ChildRules(p =>
        {
            p.RuleFor(x => x.Address).NotEmpty();
        });
    }
}
