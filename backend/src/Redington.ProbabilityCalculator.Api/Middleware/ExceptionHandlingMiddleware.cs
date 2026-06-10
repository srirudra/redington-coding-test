using System.Text.Json;

namespace Redington.ProbabilityCalculator.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent 500 ProblemDetails-style payload
/// without leaking internal details to the client.
/// </summary>
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
            await _next(context);
        }
        catch (BadHttpRequestException ex)
        {
            // Malformed request body / binding failure — an expected client error, so
            // log a concise message (no stack trace) and surface as the intended 4xx.
            _logger.LogInformation(
                "Bad request processing {Path}: {Message}", context.Request.Path, ex.Message);

            context.Response.Clear();
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/problem+json";

            var badRequestPayload = JsonSerializer.Serialize(new
            {
                title = "The request was invalid.",
                status = ex.StatusCode
            });

            await context.Response.WriteAsync(badRequestPayload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}.", context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var payload = JsonSerializer.Serialize(new
            {
                title = "An unexpected error occurred.",
                status = StatusCodes.Status500InternalServerError
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
