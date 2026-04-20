using FluentValidation;

namespace E_commerce.v1.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("Tên không được để trống.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Họ không được để trống.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải dài ít nhất 6 ký tự.");

        RuleFor(v => v.ConfirmPassword)
            .NotEmpty().WithMessage("Yêu cầu nhập lại mật khẩu xác nhận.")
            .Equal(v => v.Password).WithMessage("Mật khẩu xác nhận không khớp.");
    }
}
