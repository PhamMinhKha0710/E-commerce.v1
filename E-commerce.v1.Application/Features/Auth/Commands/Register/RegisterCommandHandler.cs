using E_commerce.v1.Application.DTOs.Auth;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public RegisterCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Role> roleRepository,
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new E_commerce.v1.Domain.Exceptions.BadRequestException("Email đã tồn tại trong hệ thống.");
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

        var userRole = await _roleRepository.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole != null)
        {
            user.UserRoles.Add(new UserRole { User = user, Role = userRole });
        }

        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtProvider.Generate(user, roles);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(user.Id, $"{user.FirstName} {user.LastName}".Trim(), user.Email, token, refreshToken);
    }
}
