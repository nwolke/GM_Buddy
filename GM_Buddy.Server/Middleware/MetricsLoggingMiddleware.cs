using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace GM_Buddy.Server.Middleware;

/// <summary>
/// Middleware that logs timing metrics for each HTTP request, including parameters and execution duration.
/// </summary>
public class MetricsLoggingMiddleware
{
    public const string MeterName = "GM_Buddy.Server.Diagnostics";

    public const string ActivitySourceName = "GM_Buddy.Server.RequestDiagnostics";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Histogram<double> RequestDurationMilliseconds = Meter.CreateHistogram<double>(
        "gm_buddy.request.duration",
        unit: "ms",
        description: "Duration of inbound HTTP requests.");
    private static readonly Histogram<long> RequestAllocatedBytes = Meter.CreateHistogram<long>(
        "gm_buddy.request.allocated_bytes",
        unit: "By",
        description: "Managed memory allocated while handling an HTTP request.");
    private static readonly Histogram<long> RequestWorkingSetBytes = Meter.CreateHistogram<long>(
        "gm_buddy.request.working_set_bytes",
        unit: "By",
        description: "Process working set memory sampled at the end of each HTTP request.");
    private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "gm_buddy.request.count",
        description: "Number of inbound HTTP requests.");
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsLoggingMiddleware> _logger;

    public MetricsLoggingMiddleware(RequestDelegate next, ILogger<MetricsLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var allocatedBytesAtStart = GC.GetTotalAllocatedBytes();
        var stopwatch = Stopwatch.StartNew();
        Exception? requestException = null;

        var activity = Activity.Current;
        try
        {
            // Continue processing the request
            await _next(context);
        }
        catch (Exception ex)
        {
            requestException = ex;
            activity?.SetTag("error.type", ex.GetType().FullName);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            var requestStatusCode = context.Response.StatusCode;
            var requestMethod = request.Method;
            var requestPath = request.Path.ToString();
            var allocatedBytes = Math.Max(0, GC.GetTotalAllocatedBytes() - allocatedBytesAtStart);
            var workingSetBytes = Environment.WorkingSet;
            var requestTags = new TagList
            {
                { "http.method", requestMethod },
                { "http.route", requestPath },
                { "http.status_code", requestStatusCode },
                { "error", requestException is not null }
            };

            RequestDurationMilliseconds.Record(elapsedMilliseconds, requestTags);
            RequestAllocatedBytes.Record(allocatedBytes, requestTags);
            RequestWorkingSetBytes.Record(workingSetBytes, requestTags);
            RequestCounter.Add(1, requestTags);

            activity?.SetTag("http.method", requestMethod);
            activity?.SetTag("http.route", requestPath);
            activity?.SetTag("http.status_code", requestStatusCode);
            activity?.SetTag("gm_buddy.request.duration_ms", elapsedMilliseconds);
            activity?.SetTag("gm_buddy.request.allocated_bytes", allocatedBytes);
            activity?.SetTag("gm_buddy.request.working_set_bytes", workingSetBytes);
            activity?.SetTag("error", requestException is not null);

            LogRequestMetrics(context, elapsedMilliseconds, allocatedBytes, workingSetBytes);
        }
    }

    private void LogRequestMetrics(HttpContext context, double elapsedMilliseconds, long allocatedBytes, long workingSetBytes)
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
            "Request Metrics: {Method} {Path} | Status: {StatusCode} | Duration: {DurationMs}ms | AllocatedBytes: {AllocatedBytes} | WorkingSetBytes: {WorkingSetBytes} | Parameters: {Parameters}",
            request.Method,
            request.Path,
            response.StatusCode,
            Math.Round(elapsedMilliseconds, 2),
            allocatedBytes,
            workingSetBytes,
            parameters);
    }

    private static bool IsSensitiveParameter(string parameterName)
    {
        var sensitiveKeys = new[] { "token", "password", "secret", "key", "auth", "authorization", "apikey", "api_key" };
        return sensitiveKeys.Any(sk => parameterName.Contains(sk, StringComparison.OrdinalIgnoreCase));
    }
}
