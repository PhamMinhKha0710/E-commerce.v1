using E_commerce.v1.Application.DTOs.Auth;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace E_commerce.v1.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IAppDbContext _dbContext;

    public RefreshTokenCommandHandler(IGenericRepository<User> userRepository, IJwtProvider jwtProvider, IAppDbContext dbContext)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _dbContext = dbContext;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new Exception("Invalid access token or refresh token");

        var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            throw new Exception("Invalid token claims");

        // We need UserRoles as well when generating the new token
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId, "UserRoles.Role");

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new Exception("Invalid access token or refresh token");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _jwtProvider.Generate(user, roles);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        await _userRepository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(user.Id, $"{user.FirstName} {user.LastName}".Trim(), user.Email, newAccessToken, newRefreshToken);
    }
}
