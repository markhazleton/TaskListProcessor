namespace TaskListProcessing;

/// <summary>
/// Configuration options for TaskListProcessor.
/// </summary>
public class TaskListProcessorOptions
{
    /// <summary>
    /// Gets or sets the default timeout for individual tasks.
    /// </summary>
    public TimeSpan DefaultTaskTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum number of concurrent tasks.
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = Environment.ProcessorCount * 2;

    /// <summary>
    /// Gets or sets whether to continue processing when a task fails.
    /// </summary>
    public bool ContinueOnTaskFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect detailed telemetry.
    /// </summary>
    public bool EnableDetailedTelemetry { get; set; } = true;

    /// <summary>
    /// Gets or sets the retry policy for failed tasks.
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the circuit breaker configuration.
    /// </summary>
    public CircuitBreakerOptions? CircuitBreakerOptions { get; set; }
}

/// <summary>
/// Retry policy configuration for failed tasks.
/// </summary>
public class RetryPolicy
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retries.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the backoff strategy.
    /// </summary>
    public BackoffStrategy BackoffStrategy { get; set; } = BackoffStrategy.Exponential;

    /// <summary>
    /// Gets or sets the maximum delay between retries.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Circuit breaker configuration options.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the failure threshold before opening the circuit.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the time window for counting failures.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the duration to keep the circuit open.
    /// </summary>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromMinutes(1);
}

/// <summary>
/// Backoff strategies for retry policies.
/// </summary>
public enum BackoffStrategy
{
    /// <summary>
    /// Fixed delay between retries.
    /// </summary>
    Fixed,

    /// <summary>
    /// Exponential backoff with jitter.
    /// </summary>
    Exponential,

    /// <summary>
    /// Linear increase in delay.
    /// </summary>
    Linear
}
