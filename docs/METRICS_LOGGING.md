# Metrics Logging

## Overview
This implementation adds stopwatch-based timing metrics to all HTTP requests in the GM Buddy API.

## Implementation Details

### MetricsLoggingMiddleware
Located in `GM_Buddy.Server/Middleware/MetricsLoggingMiddleware.cs`

This middleware:
- Uses `System.Diagnostics.Stopwatch` to measure request execution time
- Captures query string parameters (with sensitive parameter filtering)
- Captures route parameters (excluding controller/action)
- Logs all metrics via `ILogger<T>` at Information level
- Executes in the finally block to ensure metrics are logged even if errors occur

### Security Features
The middleware includes built-in protection against logging sensitive data:
- Filters out query parameters with sensitive names (token, password, secret, key, auth, authorization, apikey, api_key)
- Handles null route parameter values gracefully
- Prevents accidental exposure of credentials in application logs

### Registration
The middleware is registered in `Program.cs` after response compression but before CORS:

```csharp
app.UseResponseCompression();
app.UseMiddleware<MetricsLoggingMiddleware>();
app.UseCors("AllowSpecificOrigins");
```

## Example Log Output

### Simple GET request
```
Request Metrics: GET /npcs | Status: 200 | Duration: 45ms | Parameters: None
```

### GET with query parameters
```
Request Metrics: GET /npcs | Status: 200 | Duration: 52ms | Parameters: QueryString: ?campaignId=123
```

### GET with route parameters
```
Request Metrics: GET /npcs/42 | Status: 200 | Duration: 38ms | Parameters: RouteParams: id=42
```

### POST request
```
Request Metrics: POST /npcs | Status: 201 | Duration: 127ms | Parameters: None
```

### Request with sensitive parameters filtered
```
Request Metrics: GET /api/data | Status: 200 | Duration: 41ms | Parameters: QueryString: ?userId=123&includeDetails=true
```
Note: A request like `/api/data?userId=123&token=abc123&includeDetails=true` would have the `token` parameter filtered out.

### Request with both query and route parameters
```
Request Metrics: GET /campaigns/5/npcs | Status: 200 | Duration: 63ms | Parameters: QueryString: ?includeInactive=true, RouteParams: id=5
```

## Future Enhancements

The current implementation provides basic stopwatch logging as requested in GM-56. Future enhancements could include:

- Integration with Application Insights or New Relic for advanced analytics
- AWS CloudWatch integration for cloud-based monitoring
- Custom performance counters for specific endpoint tracking
- Metric aggregation and reporting
- Alerting for slow requests (threshold-based)
- Request correlation IDs for distributed tracing
