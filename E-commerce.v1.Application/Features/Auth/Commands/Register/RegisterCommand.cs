using E_commerce.v1.Application.DTOs.Auth;
using MediatR;

namespace E_commerce.v1.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string ConfirmPassword) : IRequest<AuthResponseDto>;
