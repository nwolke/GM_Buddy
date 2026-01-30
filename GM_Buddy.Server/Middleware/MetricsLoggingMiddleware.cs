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
        
        // Add query string parameters
        if (request.QueryString.HasValue)
        {
            parametersBuilder.Append($"QueryString: {request.QueryString.Value}");
        }

        // Add route values
        if (request.RouteValues?.Count > 0)
        {
            if (parametersBuilder.Length > 0)
                parametersBuilder.Append(", ");
            
            var routeParams = string.Join(", ", request.RouteValues
                .Where(rv => rv.Key != "controller" && rv.Key != "action")
                .Select(rv => $"{rv.Key}={rv.Value}"));
            
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
}
