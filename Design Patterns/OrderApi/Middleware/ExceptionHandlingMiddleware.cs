using Microsoft.AspNetCore.Http;

namespace OrderApi.Middleware;

/// <summary>[Pattern: Chain of Responsibility][SOLID: SRP] Catches all unhandled exceptions and returns a standardized error response.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionMessage}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCode(ex);

            var response = new
            {
                message = ex.Message,
                type = ex.GetType().Name
            };

            await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
        }
    }

    private static int GetStatusCode(Exception ex) => ex switch
    {
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        ArgumentException => StatusCodes.Status400BadRequest,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };
}
