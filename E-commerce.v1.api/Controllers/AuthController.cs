using E_commerce.v1.Application.Features.Auth.Commands.Register;
using E_commerce.v1.Application.Features.Auth.Queries.Login;
using E_commerce.v1.Application.Features.Auth.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    
    /// <summary>Sanity-check endpoint (deprecated, dùng GET api/v1/dev/sanity/admin-profile trong môi trường Development).</summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpGet("admin-profile")]
    [Obsolete("Use GET api/v1/dev/sanity/admin-profile instead (Development only).")]
    public IActionResult AdminProfile()
    {
        return Ok(new { message = "Chào mừng Admin! Dữ liệu tuyệt mật của hệ thống đã được cấp." });
    }
}
