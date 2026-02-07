namespace GM_Buddy.Server.Configuration;

/// <summary>
/// Configuration settings for AWS CloudWatch integration.
/// </summary>
public class CloudWatchSettings
{
    /// <summary>
    /// Enables AWS CloudWatch logging and metrics.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// AWS region for CloudWatch services.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// CloudWatch Log Group name for application logs.
    /// </summary>
    public string LogGroup { get; set; } = "/aws/elasticbeanstalk/gm-buddy/application";

    /// <summary>
    /// CloudWatch custom metrics namespace.
    /// </summary>
    public string MetricsNamespace { get; set; } = "GMBuddy/Application";
}
