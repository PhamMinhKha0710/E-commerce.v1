using E_commerce.v1.Application.DTOs.Auth;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Queries.Login;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponseDto>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginQueryHandler(IGenericRepository<User> userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponseDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, "UserRoles.Role");

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new E_commerce.v1.Domain.Exceptions.BadRequestException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        if (!user.IsActive)
        {
            throw new E_commerce.v1.Domain.Exceptions.BadRequestException("Tài khoản này đã bị khóa.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtProvider.Generate(user, roles);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return new AuthResponseDto(user.Id, $"{user.FirstName} {user.LastName}".Trim(), user.Email, token, refreshToken);
    }
}
