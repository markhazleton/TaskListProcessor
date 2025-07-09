using Microsoft.Extensions.Options;
using System;
using System.Linq;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Scheduling;

namespace TaskListProcessing.Options;

/// <summary>
/// Enhanced configuration options for TaskListProcessor with validation support.
/// </summary>
public class TaskListProcessorOptions : IValidateOptions<TaskListProcessorOptions>
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
    /// Gets or sets whether to enable progress reporting.
    /// </summary>
    public bool EnableProgressReporting { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable memory pooling for better performance.
    /// </summary>
    public bool EnableMemoryPooling { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable async enumerable task streaming.
    /// </summary>
    public bool EnableTaskStreaming { get; set; } = false;

    /// <summary>
    /// Gets or sets the retry policy for failed tasks.
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the circuit breaker configuration.
    /// </summary>
    public CircuitBreakerOptions? CircuitBreakerOptions { get; set; }

    /// <summary>
    /// Gets or sets the telemetry exporter for external monitoring systems.
    /// </summary>
    public ITelemetryExporter? TelemetryExporter { get; set; }

    /// <summary>
    /// Gets or sets the health check configuration.
    /// </summary>
    public HealthCheckOptions? HealthCheckOptions { get; set; }

    /// <summary>
    /// Gets or sets the task dependency resolver.
    /// </summary>
    public ITaskDependencyResolver? DependencyResolver { get; set; }

    /// <summary>
    /// Gets or sets the memory pool configuration.
    /// </summary>
    public MemoryPoolOptions? MemoryPoolOptions { get; set; }

    /// <summary>
    /// Gets or sets custom metadata for the processor instance.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the task scheduling strategy.
    /// </summary>
    public TaskSchedulingStrategy SchedulingStrategy { get; set; } = TaskSchedulingStrategy.FirstInFirstOut;

    /// <summary>
    /// Gets or sets the maximum queue size for pending tasks.
    /// </summary>
    public int MaxQueueSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the task execution context options.
    /// </summary>
    public TaskExecutionContextOptions? ExecutionContextOptions { get; set; }

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>Validation result.</returns>
    public ValidateOptionsResult Validate(string? name, TaskListProcessorOptions options)
    {
        var failures = new List<string>();

        if (options.MaxConcurrentTasks <= 0)
            failures.Add("MaxConcurrentTasks must be greater than 0");

        if (options.DefaultTaskTimeout <= TimeSpan.Zero)
            failures.Add("DefaultTaskTimeout must be positive");

        if (options.MaxQueueSize <= 0)
            failures.Add("MaxQueueSize must be greater than 0");

        // Validate retry policy if present
        if (options.RetryPolicy != null)
        {
            var retryValidation = options.RetryPolicy.Validate();
            if (!retryValidation.IsValid)
                failures.AddRange(retryValidation.Errors.Select(e => $"RetryPolicy: {e}"));
        }

        // Validate circuit breaker options if present
        if (options.CircuitBreakerOptions != null)
        {
            var cbValidation = options.CircuitBreakerOptions.Validate();
            if (!cbValidation.IsValid)
                failures.AddRange(cbValidation.Errors.Select(e => $"CircuitBreaker: {e}"));
        }

        // Validate memory pool options if present
        if (options.MemoryPoolOptions != null)
        {
            var mpValidation = options.MemoryPoolOptions.Validate();
            if (!mpValidation.IsValid)
                failures.AddRange(mpValidation.Errors.Select(e => $"MemoryPool: {e}"));
        }

        return failures.Any()
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
