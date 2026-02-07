# Observability and Metrics Tracking Guide

This document provides a comprehensive guide to the observability, logging, and metrics tracking capabilities implemented in GM_Buddy.

## Table of Contents

- [Overview](#overview)
- [Logging Infrastructure](#logging-infrastructure)
- [Metrics Collection](#metrics-collection)
- [Health Checks](#health-checks)
- [AWS CloudWatch Integration](#aws-cloudwatch-integration)
- [Configuration](#configuration)
- [Monitoring Dashboards](#monitoring-dashboards)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Overview

GM_Buddy implements a comprehensive observability stack that includes:

- **Structured Logging** with Serilog (console and file-based)
- **.NET Metrics API** for custom application metrics
- **AWS CloudWatch** integration for logs and metrics
- **Health Check Endpoints** for container orchestration
- **Request/Response Tracking** with performance metrics
- **System Metrics** including memory, CPU, and garbage collection

## Logging Infrastructure

### Serilog Configuration

The application uses **Serilog** for structured logging with multiple outputs:

#### Development Environment
- **Console Logging**: Human-readable format with timestamps and log levels
- **File Logging**: Rolling daily files in `./logs/gm-buddy-YYYYMMDD.log`
- **Log Level**: Information for application code, Warning for Microsoft libraries
- **Retention**: 30 days of log files

#### Production Environment
- **Console Logging**: Compact format for container logs
- **File Logging**: Rolling daily files in `/var/log/gm-buddy/application-YYYYMMDD.log`
- **CloudWatch Logging**: Optional integration with AWS CloudWatch Logs
- **Log Level**: Warning for most code, Information for critical paths
- **Retention**: 7 days of local files (CloudWatch retention configured separately)

### Log File Format

Logs are written in a structured format:

```
2026-02-07 17:19:06.123 +00:00 [INF] [GM_Buddy.Server.Middleware.MetricsLoggingMiddleware] Request Metrics: GET /api/campaigns | Status: 200 | Duration: 45.23ms | Memory: 12.34KB | Parameters: None
```

### Request Logging

Every HTTP request is automatically logged with:
- HTTP method and path
- Response status code
- Execution duration in milliseconds
- Memory allocation during request
- Query parameters and route values (sensitive data sanitized)
- User agent and host information

## Metrics Collection

### Built-in Metrics

The application automatically collects the following metrics:

#### Request Metrics
- `gm_buddy.requests.total` - Total HTTP requests (with method, path, status tags)
- `gm_buddy.request.duration` - Request duration histogram in milliseconds
- `gm_buddy.errors.total` - Total errors (with error type and path tags)

#### System Metrics
- `gm_buddy.memory.used` - Current memory usage in bytes
- `gm_buddy.cpu.usage` - CPU usage percentage
- `gm_buddy.threads.count` - Active thread count
- `gm_buddy.gc.gen0` - Generation 0 garbage collections
- `gm_buddy.gc.gen1` - Generation 1 garbage collections
- `gm_buddy.gc.gen2` - Generation 2 garbage collections

### Accessing Metrics

Metrics can be accessed through:

1. **.NET Metrics API** - Use tools like `dotnet-counters` to monitor metrics in real-time:
   ```bash
   dotnet-counters monitor -p <process-id> --counters GMBuddy.Application
   ```

2. **AWS CloudWatch** - When enabled, metrics are sent to CloudWatch under the namespace `GMBuddy/Application`

3. **Prometheus/Grafana** - Metrics can be exported using .NET metrics exporters (not included by default)

### Custom Metrics

You can record custom metrics in your code:

```csharp
public class MyController : ControllerBase
{
    private readonly MetricsService _metricsService;
    
    public MyController(MetricsService metricsService)
    {
        _metricsService = metricsService;
    }
    
    [HttpPost]
    public IActionResult ProcessData()
    {
        // Record custom metric
        _metricsService.RecordCustomMetric("data_processed", 1, "Count");
        
        return Ok();
    }
}
```

## Health Checks

The application provides three health check endpoints for monitoring:

### Endpoints

#### 1. `/health` - Complete Health Check
Returns the overall health status including all dependencies:
- API self-check
- Database connectivity
- Returns HTTP 200 if healthy, 503 if unhealthy

**Example Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "self": {
      "status": "Healthy",
      "description": "API is running"
    },
    "postgresql": {
      "status": "Healthy",
      "description": "Connected to PostgreSQL"
    }
  }
}
```

#### 2. `/health/ready` - Readiness Probe
Checks if the application is ready to accept traffic (database connected):
- Use for Kubernetes readiness probes
- Use for load balancer health checks

#### 3. `/health/live` - Liveness Probe
Checks if the application process is running:
- Use for Kubernetes liveness probes
- Always returns healthy if the app is running

### Using Health Checks

**Docker Compose:**
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
  interval: 30s
  timeout: 10s
  retries: 3
```

**Kubernetes:**
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
```

## AWS CloudWatch Integration

### Setup

1. **Enable CloudWatch in appsettings.Production.json:**
   ```json
   {
     "AWS": {
       "CloudWatch": {
         "Enabled": true,
         "Region": "us-east-1",
         "LogGroup": "/aws/elasticbeanstalk/gm-buddy/application",
         "MetricsNamespace": "GMBuddy/Application"
       }
     }
   }
   ```

2. **IAM Permissions Required:**
   The application requires the following IAM permissions:
   ```json
   {
     "Version": "2012-10-17",
     "Statement": [
       {
         "Effect": "Allow",
         "Action": [
           "logs:CreateLogGroup",
           "logs:CreateLogStream",
           "logs:PutLogEvents",
           "logs:DescribeLogStreams"
         ],
         "Resource": "arn:aws:logs:*:*:log-group:/aws/elasticbeanstalk/gm-buddy/*"
       },
       {
         "Effect": "Allow",
         "Action": [
           "cloudwatch:PutMetricData"
         ],
         "Resource": "*",
         "Condition": {
           "StringEquals": {
             "cloudwatch:namespace": "GMBuddy/Application"
           }
         }
       }
     ]
   }
   ```

### CloudWatch Logs

When enabled, all application logs are sent to CloudWatch Logs in addition to local files. This allows:
- Centralized log aggregation
- Long-term log retention
- Log analysis with CloudWatch Insights
- Alerting based on log patterns

### CloudWatch Metrics

Custom metrics are sent every 30-60 seconds (configurable) to CloudWatch, including:
- Request duration and count
- Error rates
- Memory usage
- CPU usage
- Thread count

### Creating CloudWatch Alarms

**Example: High Error Rate Alarm**
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-high-error-rate \
  --alarm-description "Alert when error rate is too high" \
  --metric-name Errors \
  --namespace GMBuddy/Application \
  --statistic Sum \
  --period 300 \
  --evaluation-periods 1 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold
```

**Example: High Memory Usage Alarm**
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-high-memory \
  --alarm-description "Alert when memory usage exceeds 500MB" \
  --metric-name MemoryUsage \
  --namespace GMBuddy/Application \
  --statistic Average \
  --period 300 \
  --evaluation-periods 2 \
  --threshold 500 \
  --comparison-operator GreaterThanThreshold
```

## Configuration

### appsettings.json Options

```json
{
  "Observability": {
    "EnableMetrics": true,              // Enable metrics collection
    "EnableDetailedErrors": false,       // Show detailed errors (dev only)
    "MemoryMetricsIntervalSeconds": 60, // Metrics collection interval
    "EnableHealthChecks": true           // Enable health check endpoints
  },
  "AWS": {
    "CloudWatch": {
      "Enabled": false,                  // Enable CloudWatch integration
      "Region": "us-east-1",             // AWS region
      "LogGroup": "/aws/elasticbeanstalk/gm-buddy/application",
      "MetricsNamespace": "GMBuddy/Application"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Monitoring Dashboards

### CloudWatch Dashboard (Recommended)

Create a CloudWatch dashboard to visualize metrics:

1. Go to CloudWatch Console → Dashboards
2. Create a new dashboard: "GMBuddy-Monitoring"
3. Add widgets for:
   - Request duration (line graph)
   - Error count (number)
   - Memory usage (line graph)
   - CPU usage (line graph)
   - Request count by path (stacked area)

### Grafana Integration

If you want to use Grafana:

1. Install the CloudWatch data source plugin
2. Configure it with your AWS credentials
3. Create dashboards querying the `GMBuddy/Application` namespace

## Best Practices

1. **Log Levels**
   - Use `LogInformation` for important business events
   - Use `LogWarning` for recoverable errors
   - Use `LogError` for unexpected errors that need attention
   - Use `LogDebug` for detailed troubleshooting (disabled in production)

2. **Sensitive Data**
   - Never log passwords, tokens, or API keys
   - The middleware automatically sanitizes common sensitive parameters
   - Review custom logging to ensure no PII is logged

3. **Performance**
   - Metrics collection has minimal overhead (~1-2ms per request)
   - CloudWatch submissions are async and don't block requests
   - Adjust `MemoryMetricsIntervalSeconds` based on your needs

4. **Cost Optimization**
   - CloudWatch Logs: ~$0.50/GB ingested + storage costs
   - CloudWatch Metrics: ~$0.30/metric/month
   - Set appropriate retention policies
   - Use metric filters to reduce data ingestion

5. **Alerting**
   - Set up alarms for critical metrics (error rate, memory usage)
   - Use SNS to send notifications to email/Slack
   - Create runbooks for common alerts

## Troubleshooting

### Logs Not Appearing

**Check log directory permissions:**
```bash
# Development
ls -la logs/

# Production
ls -la /var/log/gm-buddy/
```

**Check Serilog configuration:**
- Ensure `appsettings.json` has correct Serilog configuration
- Check for errors in console output during startup

### CloudWatch Not Receiving Data

**Verify IAM permissions:**
```bash
aws iam get-role-policy --role-name <your-role> --policy-name <policy-name>
```

**Check application logs for CloudWatch errors:**
```bash
grep "CloudWatch" logs/gm-buddy-*.log
```

**Test AWS credentials:**
```bash
aws sts get-caller-identity
```

### High Memory Usage

**Check for memory leaks:**
```bash
dotnet-counters monitor -p <process-id> System.Runtime
```

**Analyze GC metrics:**
- High Gen2 collections indicate potential memory pressure
- Review application code for large object allocations
- Consider increasing container memory limits

### Metrics Not Updating

**Verify MetricsService is registered:**
- Check `Program.cs` for `AddSingleton<MetricsService>()`
- Ensure middleware is injecting MetricsService

**Check metrics interval:**
- Default is 60 seconds in production, 30 in development
- Adjust `MemoryMetricsIntervalSeconds` if needed

## GitHub Actions Integration

The observability features integrate with GitHub Actions workflows:

1. **Build Logs**: All build and test output includes structured logs
2. **Health Checks**: Deployment workflows verify health endpoints
3. **Metrics**: Future enhancement to track deployment success rates

## Additional Resources

- [Serilog Documentation](https://serilog.net/)
- [.NET Metrics API](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics)
- [AWS CloudWatch Documentation](https://docs.aws.amazon.com/cloudwatch/)
- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

## Support

For issues or questions:
1. Check this documentation
2. Review application logs
3. Check CloudWatch Logs (if enabled)
4. Open a GitHub issue with relevant logs
