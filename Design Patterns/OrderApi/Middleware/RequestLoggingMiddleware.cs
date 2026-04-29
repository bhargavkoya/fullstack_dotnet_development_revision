using Microsoft.AspNetCore.Http;

namespace OrderApi.Middleware;

/// <summary>[Pattern: Chain of Responsibility][SOLID: SRP] Logs incoming requests and outgoing responses with timing.</summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();

            var method = context.Request.Method;
            var path = context.Request.Path;
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("[{Method}] {Path} → {StatusCode} ({ElapsedMs}ms)", method, path, statusCode, elapsedMs);
        }
    }
}
