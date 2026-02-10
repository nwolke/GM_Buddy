using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GM_Buddy.Server.Middleware;

/// <summary>
/// Middleware that catches all unhandled exceptions and converts them to
/// structured ProblemDetails responses with correlation IDs for log tracing.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var (statusCode, title) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}",
                correlationId, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Request failed ({StatusCode}). CorrelationId: {CorrelationId}, Path: {Path}, Message: {Message}",
                statusCode, correlationId, context.Request.Path, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode < 500 || _environment.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred.",
        };
        problemDetails.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (int statusCode, string title) MapException(Exception exception) => exception switch
    {
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        InvalidOperationException => (StatusCodes.Status404NotFound, "Not Found"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
}
