namespace Todo.Infrastructure.ExternalApi;

/// <summary>Binds the <c>ExternalApi</c> configuration section.</summary>
public class ExternalApiOptions
{
    public const string SectionName = "ExternalApi";

    public string BaseUrl { get; set; } = "http://localhost";

    /// <summary>Per-request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Maximum retry attempts for transient failures.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Base delay (seconds) for the exponential backoff between retries.</summary>
    public double RetryBaseDelaySeconds { get; set; } = 1;
}
