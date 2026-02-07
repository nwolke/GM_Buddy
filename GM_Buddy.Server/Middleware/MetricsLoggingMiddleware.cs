using System.Diagnostics;
using System.Text;
using GM_Buddy.Server.Services;

namespace GM_Buddy.Server.Middleware;

/// <summary>
/// Middleware that logs timing metrics for each HTTP request, including parameters, execution duration,
/// and memory usage. Integrates with MetricsService for comprehensive observability.
/// </summary>
public class MetricsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsLoggingMiddleware> _logger;
    private readonly MetricsService? _metricsService;

    public MetricsLoggingMiddleware(
        RequestDelegate next, 
        ILogger<MetricsLoggingMiddleware> logger,
        MetricsService? metricsService = null)
    {
        _next = next;
        _logger = logger;
        _metricsService = metricsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);
        
        try
        {
            // Continue processing the request
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception and record error metric
            _logger.LogError(ex, "Unhandled exception in request {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            _metricsService?.RecordError(ex.GetType().Name, context.Request.Path);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = memoryAfter - memoryBefore;
            
            LogRequestMetrics(context, stopwatch.ElapsedMilliseconds, memoryUsed);
            
            // Record metrics in MetricsService
            _metricsService?.RecordRequest(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private void LogRequestMetrics(HttpContext context, long elapsedMilliseconds, long memoryUsed)
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
        var memoryUsedKb = memoryUsed / 1024.0;

        _logger.LogInformation(
            "Request Metrics: {Method} {Path} | Status: {StatusCode} | Duration: {DurationMs}ms | Memory: {MemoryKb:F2}KB | Parameters: {Parameters}",
            request.Method,
            request.Path,
            response.StatusCode,
            elapsedMilliseconds,
            memoryUsedKb,
            parameters);
    }

    private static bool IsSensitiveParameter(string parameterName)
    {
        var sensitiveKeys = new[] { "token", "password", "secret", "key", "auth", "authorization", "apikey", "api_key" };
        return sensitiveKeys.Any(sk => parameterName.Contains(sk, StringComparison.OrdinalIgnoreCase));
    }
}
