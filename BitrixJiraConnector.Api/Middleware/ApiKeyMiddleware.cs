namespace BitrixJiraConnector.Api.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ApiKeyConfigKey = "ApiKey";

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth for health and login endpoints
        if (context.Request.Path.StartsWithSegments("/api/health", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        string? configuredKey = configuration[ApiKeyConfigKey];

        if (string.IsNullOrEmpty(configuredKey))
        {
            // ApiKey not configured — block all requests to prevent accidental open access
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("API key not configured on server.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey)
            || providedKey != configuredKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized.");
            return;
        }

        await next(context);
    }
}
