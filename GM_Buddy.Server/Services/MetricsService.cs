using System.Diagnostics;
using System.Diagnostics.Metrics;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using GM_Buddy.Server.Configuration;

namespace GM_Buddy.Server.Services;

/// <summary>
/// Service for collecting and publishing application metrics including memory, CPU, and custom metrics.
/// Supports both .NET Metrics API and AWS CloudWatch.
/// </summary>
public class MetricsService : IDisposable
{
    private readonly Meter _meter;
    private readonly ILogger<MetricsService> _logger;
    private readonly CloudWatchSettings _cloudWatchSettings;
    private readonly IAmazonCloudWatch? _cloudWatchClient;
    private readonly Timer? _memoryMetricsTimer;
    
    // Instruments for tracking various metrics
    private readonly Counter<long> _requestCounter;
    private readonly Counter<long> _errorCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly ObservableGauge<long> _memoryUsage;
    private readonly ObservableGauge<double> _cpuUsage;
    private readonly ObservableGauge<int> _threadCount;
    private readonly ObservableGauge<int> _gen0Collections;
    private readonly ObservableGauge<int> _gen1Collections;
    private readonly ObservableGauge<int> _gen2Collections;
    
    private long _totalMemory;
    private double _lastCpuTime;
    private DateTime _lastCpuCheck = DateTime.UtcNow;

    public MetricsService(
        ILogger<MetricsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _cloudWatchSettings = configuration.GetSection("AWS:CloudWatch").Get<CloudWatchSettings>() ?? new CloudWatchSettings();
        
        // Initialize .NET Metrics
        _meter = new Meter("GMBuddy.Application", "1.0.0");
        
        // Request metrics
        _requestCounter = _meter.CreateCounter<long>(
            "gm_buddy.requests.total",
            description: "Total number of HTTP requests");
            
        _errorCounter = _meter.CreateCounter<long>(
            "gm_buddy.errors.total",
            description: "Total number of errors");
            
        _requestDuration = _meter.CreateHistogram<double>(
            "gm_buddy.request.duration",
            unit: "ms",
            description: "Request duration in milliseconds");
        
        // System metrics
        _memoryUsage = _meter.CreateObservableGauge(
            "gm_buddy.memory.used",
            () => _totalMemory,
            unit: "bytes",
            description: "Current memory usage in bytes");
            
        _cpuUsage = _meter.CreateObservableGauge(
            "gm_buddy.cpu.usage",
            () => _lastCpuTime,
            unit: "percent",
            description: "CPU usage percentage");
            
        _threadCount = _meter.CreateObservableGauge(
            "gm_buddy.threads.count",
            () => ThreadPool.ThreadCount,
            description: "Current thread count");
            
        _gen0Collections = _meter.CreateObservableGauge(
            "gm_buddy.gc.gen0",
            () => GC.CollectionCount(0),
            description: "Generation 0 garbage collections");
            
        _gen1Collections = _meter.CreateObservableGauge(
            "gm_buddy.gc.gen1",
            () => GC.CollectionCount(1),
            description: "Generation 1 garbage collections");
            
        _gen2Collections = _meter.CreateObservableGauge(
            "gm_buddy.gc.gen2",
            () => GC.CollectionCount(2),
            description: "Generation 2 garbage collections");
        
        // Initialize CloudWatch client if enabled
        if (_cloudWatchSettings.Enabled)
        {
            try
            {
                _cloudWatchClient = new AmazonCloudWatchClient(
                    Amazon.RegionEndpoint.GetBySystemName(_cloudWatchSettings.Region));
                _logger.LogInformation("CloudWatch metrics enabled for namespace: {Namespace}", 
                    _cloudWatchSettings.MetricsNamespace);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize CloudWatch client. Metrics will not be sent to CloudWatch.");
            }
        }
        
        // Start background task to update metrics
        var observabilitySettings = configuration.GetSection("Observability").Get<ObservabilitySettings>() ?? new ObservabilitySettings();
        var interval = TimeSpan.FromSeconds(observabilitySettings.MemoryMetricsIntervalSeconds);
        _memoryMetricsTimer = new Timer(UpdateSystemMetrics, null, TimeSpan.Zero, interval);
        
        _logger.LogInformation("MetricsService initialized with metrics collection interval: {Interval}s", 
            observabilitySettings.MemoryMetricsIntervalSeconds);
    }

    /// <summary>
    /// Records a completed HTTP request.
    /// </summary>
    public void RecordRequest(string method, string path, int statusCode, double durationMs)
    {
        _requestCounter.Add(1, 
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("path", path),
            new KeyValuePair<string, object?>("status", statusCode));
            
        _requestDuration.Record(durationMs,
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("path", path));
        
        // Send to CloudWatch if enabled
        if (_cloudWatchClient != null && _cloudWatchSettings.Enabled)
        {
            _ = SendMetricToCloudWatchAsync("RequestDuration", durationMs, StandardUnit.Milliseconds, 
                new Dictionary<string, string> { { "Method", method }, { "Path", path } });
        }
    }

    /// <summary>
    /// Records an error occurrence.
    /// </summary>
    public void RecordError(string errorType, string? path = null)
    {
        _errorCounter.Add(1,
            new KeyValuePair<string, object?>("error_type", errorType),
            new KeyValuePair<string, object?>("path", path ?? "unknown"));
        
        // Send to CloudWatch if enabled
        if (_cloudWatchClient != null && _cloudWatchSettings.Enabled)
        {
            _ = SendMetricToCloudWatchAsync("Errors", 1, StandardUnit.Count,
                new Dictionary<string, string> { { "ErrorType", errorType } });
        }
    }

    /// <summary>
    /// Records a custom metric value.
    /// </summary>
    public void RecordCustomMetric(string name, double value, string unit = "Count")
    {
        _logger.LogDebug("Custom metric recorded: {Name} = {Value} {Unit}", name, value, unit);
        
        // Send to CloudWatch if enabled
        if (_cloudWatchClient != null && _cloudWatchSettings.Enabled)
        {
            var cloudWatchUnit = unit switch
            {
                "Count" => StandardUnit.Count,
                "Milliseconds" => StandardUnit.Milliseconds,
                "Seconds" => StandardUnit.Seconds,
                "Bytes" => StandardUnit.Bytes,
                "Percent" => StandardUnit.Percent,
                _ => StandardUnit.None
            };
            
            _ = SendMetricToCloudWatchAsync(name, value, cloudWatchUnit);
        }
    }

    private void UpdateSystemMetrics(object? state)
    {
        try
        {
            // Update memory metrics
            var process = Process.GetCurrentProcess();
            _totalMemory = process.WorkingSet64;
            
            // Update CPU metrics
            var currentTime = DateTime.UtcNow;
            var timeDiff = (currentTime - _lastCpuCheck).TotalMilliseconds;
            
            if (timeDiff > 0)
            {
                var currentCpuTime = process.TotalProcessorTime.TotalMilliseconds;
                var cpuUsedMs = currentCpuTime - _lastCpuTime;
                _lastCpuTime = (cpuUsedMs / (Environment.ProcessorCount * timeDiff)) * 100;
            }
            
            _lastCpuCheck = currentTime;
            
            // Send to CloudWatch if enabled
            if (_cloudWatchClient != null && _cloudWatchSettings.Enabled)
            {
                _ = SendMetricToCloudWatchAsync("MemoryUsage", _totalMemory / (1024.0 * 1024.0), StandardUnit.Megabytes);
                _ = SendMetricToCloudWatchAsync("CPUUsage", _lastCpuTime, StandardUnit.Percent);
                _ = SendMetricToCloudWatchAsync("ThreadCount", ThreadPool.ThreadCount, StandardUnit.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system metrics");
        }
    }

    private async Task SendMetricToCloudWatchAsync(
        string metricName, 
        double value, 
        StandardUnit unit,
        Dictionary<string, string>? dimensions = null)
    {
        if (_cloudWatchClient == null) return;
        
        try
        {
            var metricDimensions = new List<Dimension>
            {
                new Dimension { Name = "Application", Value = "GMBuddy" }
            };
            
            if (dimensions != null)
            {
                foreach (var dim in dimensions)
                {
                    metricDimensions.Add(new Dimension { Name = dim.Key, Value = dim.Value });
                }
            }
            
            var request = new PutMetricDataRequest
            {
                Namespace = _cloudWatchSettings.MetricsNamespace,
                MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        MetricName = metricName,
                        Value = value,
                        Unit = unit,
                        Timestamp = DateTime.UtcNow,
                        Dimensions = metricDimensions
                    }
                }
            };
            
            await _cloudWatchClient.PutMetricDataAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send metric {MetricName} to CloudWatch", metricName);
        }
    }

    public void Dispose()
    {
        _memoryMetricsTimer?.Dispose();
        _meter?.Dispose();
        _cloudWatchClient?.Dispose();
    }
}
