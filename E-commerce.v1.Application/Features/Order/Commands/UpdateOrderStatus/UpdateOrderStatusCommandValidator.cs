using E_commerce.v1.Domain.Enums;
using FluentValidation;

namespace E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .Must(s => s is OrderStatus.Confirmed or OrderStatus.Cancelled or OrderStatus.Completed)
            .WithMessage("Trạng thái đơn hàng không hợp lệ.");
    }
}
