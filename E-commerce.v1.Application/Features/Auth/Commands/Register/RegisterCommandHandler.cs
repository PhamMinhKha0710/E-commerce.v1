using E_commerce.v1.Application.DTOs.Auth;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (existingUser)
        {
            throw new BadRequestException("Email đã tồn tại trong hệ thống.");
        }

        var hash = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = hash,
            IsActive = true
        };

        var userRole = await _userRepository.GetRoleByNameAsync("User", cancellationToken);
        if (userRole != null)
        {
            user.UserRoles.Add(new UserRole { User = user, Role = userRole });
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtProvider.Generate(user, roles);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(user.Id, $"{user.FirstName} {user.LastName}".Trim(), user.Email, token, refreshToken);
    }
}
