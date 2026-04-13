using FluentValidation;

namespace E_commerce.v1.Application.Features.Auth.Queries.Login;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Bạn chưa nhập Email.")
            .EmailAddress().WithMessage("Định dạng Email không hợp lệ.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu.");
    }
}
