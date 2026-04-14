using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using E_commerce.v1.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace E_commerce.v1.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var sub = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public string GetActor()
    {
        if (!IsAuthenticated)
            return "system";

        return Email ?? UserId?.ToString() ?? "system";
    }
}
