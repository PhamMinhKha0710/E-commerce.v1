using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

/// <summary>
/// Dev-only sanity-check endpoints. Not available in non-Development environments.
/// </summary>
[ApiController]
[Route("api/v1/dev/sanity")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DevSanityController : ControllerBase
{
    private readonly IHostEnvironment _env;

    public DevSanityController(IHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>Sanity check for Admin role authorization.</summary>
    [HttpGet("admin-profile")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminProfile()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        return Ok(new { message = "Chào mừng Admin! Dữ liệu tuyệt mật của hệ thống đã được cấp." });
    }
}
