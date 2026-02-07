namespace GM_Buddy.Server.Configuration;

/// <summary>
/// Configuration settings for application observability, metrics, and monitoring.
/// </summary>
public class ObservabilitySettings
{
    /// <summary>
    /// Enables collection and reporting of application metrics.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Enables detailed error messages in responses. Should be false in production.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Interval in seconds for collecting memory and performance metrics.
    /// </summary>
    public int MemoryMetricsIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Enables health check endpoints.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;
}
