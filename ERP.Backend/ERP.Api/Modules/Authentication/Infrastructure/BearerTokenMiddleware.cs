namespace ERP.Api.Modules.Authentication.Infrastructure;

public class BearerTokenMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, IConfiguration configuration)
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/api") || path.StartsWithSegments("/api/auth/login"))
        {
            await next(context);
            return;
        }

        var expected = configuration["Auth:Token"];
        var header = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(expected) && header == $"Bearer {expected}")
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid token." });
    }
}
