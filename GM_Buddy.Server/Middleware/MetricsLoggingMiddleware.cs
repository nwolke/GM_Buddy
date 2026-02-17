using System.Diagnostics;
using System.Text;

namespace GM_Buddy.Server.Middleware;

/// <summary>
/// Middleware that logs timing metrics for each HTTP request, including parameters and execution duration.
/// </summary>
public class MetricsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsLoggingMiddleware> _logger;

    public MetricsLoggingMiddleware(RequestDelegate next, ILogger<MetricsLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Continue processing the request
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            LogRequestMetrics(context, stopwatch.ElapsedMilliseconds);
        }
    }

    private void LogRequestMetrics(HttpContext context, long elapsedMilliseconds)
    {
        var request = context.Request;
        var response = context.Response;

        // Build parameters string
        var parametersBuilder = new StringBuilder();

        // Add query string parameters (sanitize sensitive keys)
        if (request.QueryString.HasValue && request.Query.Count > 0)
        {
            var sanitizedParams = request.Query
                .Where(q => !IsSensitiveParameter(q.Key))
                .Select(q => $"{q.Key}={q.Value}");

            if (sanitizedParams.Any())
            {
                parametersBuilder.Append($"QueryString: ?{string.Join("&", sanitizedParams)}");
            }
        }

        // Add route values (filter out controller/action and handle nulls)
        if (request.RouteValues?.Count > 0)
        {
            if (parametersBuilder.Length > 0)
                parametersBuilder.Append(", ");

            var routeParams = string.Join(", ", request.RouteValues
                .Where(rv => rv.Key != "controller" && rv.Key != "action")
                .Select(rv => $"{rv.Key}={rv.Value ?? "null"}"));

            if (!string.IsNullOrEmpty(routeParams))
            {
                parametersBuilder.Append($"RouteParams: {routeParams}");
            }
        }

        var parameters = parametersBuilder.Length > 0 ? parametersBuilder.ToString() : "None";

        _logger.LogInformation(
            "Request Metrics: {Method} {Path} | Status: {StatusCode} | Duration: {DurationMs}ms | Parameters: {Parameters}",
            request.Method,
            request.Path,
            response.StatusCode,
            elapsedMilliseconds,
            parameters);
    }

    private static bool IsSensitiveParameter(string parameterName)
    {
        var sensitiveKeys = new[] { "token", "password", "secret", "key", "auth", "authorization", "apikey", "api_key" };
        return sensitiveKeys.Any(sk => parameterName.Contains(sk, StringComparison.OrdinalIgnoreCase));
    }
}
