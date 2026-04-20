namespace E_commerce.v1.Application.DTOs.Auth;

public record AuthResponseDto(Guid UserId, string FullName, string Email, string Token, string RefreshToken);
