using E_commerce.v1.Application.DTOs.Auth;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;
