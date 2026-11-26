using ERP.Api.Data;
using ERP.Api.Modules.Authentication.Application;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Modules.Authentication.Presentation;

[ApiController]
[Route("api/auth")]
public class AuthController(ErpDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var configuredUser = configuration["Auth:Username"];
        var token = configuration["Auth:Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Auth token is not configured." });
        }

        if (request.Password != token)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        if (string.Equals(request.Username, configuredUser, StringComparison.Ordinal))
        {
            return Ok(new { token });
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);
        if (user is null)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        return Ok(new { token });
    }
}
