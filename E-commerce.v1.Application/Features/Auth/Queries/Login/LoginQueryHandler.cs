using E_commerce.v1.Application.DTOs.Auth;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Queries.Login;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginQueryHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponseDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(request.Email, cancellationToken);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new BadRequestException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        if (!user.IsActive)
        {
            throw new BadRequestException("Tài khoản này đã bị khóa.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtProvider.Generate(user, roles);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(user.Id, $"{user.FirstName} {user.LastName}".Trim(), user.Email, token, refreshToken);
    }
}
