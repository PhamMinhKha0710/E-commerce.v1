using E_commerce.v1.Application.DTOs.Auth;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Queries.Login;

public record LoginQuery(string Email, string Password) : IRequest<AuthResponseDto>;
