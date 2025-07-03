using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Options;
using TaskListProcessing.Scheduling;

namespace TaskListProcessing.Core;

/// <summary>
/// Fluent builder for configuring TaskListProcessor instances.
/// </summary>
public class TaskListProcessorBuilder
{
    private string _name = "TaskProcessor";
    private ILogger? _logger;
    private TaskListProcessorOptions _options = new();

    /// <summary>
    /// Creates a new TaskListProcessor builder with the specified name.
    /// </summary>
    /// <param name="name">The name of the task processor.</param>
    /// <returns>A new builder instance.</returns>
    public static TaskListProcessorBuilder Create(string name)
    {
        return new TaskListProcessorBuilder { _name = name };
    }

    /// <summary>
    /// Configures the logger for the task processor.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Configures retry policy for failed tasks.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts.</param>
    /// <param name="baseDelay">Base delay between retries.</param>
    /// <param name="strategy">Backoff strategy to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithRetry(
        int maxAttempts,
        TimeSpan baseDelay,
        BackoffStrategy strategy = BackoffStrategy.ExponentialWithJitter)
    {
        _options.RetryPolicy = new RetryPolicy
        {
            MaxRetryAttempts = maxAttempts,
            BaseDelay = baseDelay,
            BackoffStrategy = strategy
        };
        return this;
    }

    /// <summary>
    /// Configures retry policy using a predefined policy.
    /// </summary>
    /// <param name="retryPolicy">The retry policy to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithRetry(RetryPolicy retryPolicy)
    {
        _options.RetryPolicy = retryPolicy;
        return this;
    }

    /// <summary>
    /// Configures retry policy using an action.
    /// </summary>
    /// <param name="configureRetry">Action to configure the retry policy.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithRetry(Action<RetryPolicy> configureRetry)
    {
        _options.RetryPolicy ??= new RetryPolicy();
        configureRetry(_options.RetryPolicy);
        return this;
    }

    /// <summary>
    /// Configures circuit breaker for preventing cascading failures.
    /// </summary>
    /// <param name="failureThreshold">Number of failures before opening the circuit.</param>
    /// <param name="openDuration">Duration to keep the circuit open.</param>
    /// <param name="timeWindow">Time window for counting failures.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithCircuitBreaker(
        int failureThreshold,
        TimeSpan openDuration,
        TimeSpan? timeWindow = null)
    {
        _options.CircuitBreakerOptions = new CircuitBreakerOptions
        {
            FailureThreshold = failureThreshold,
            OpenDuration = openDuration,
            TimeWindow = timeWindow ?? TimeSpan.FromMinutes(1)
        };
        return this;
    }

    /// <summary>
    /// Configures circuit breaker using an action.
    /// </summary>
    /// <param name="configureCircuitBreaker">Action to configure the circuit breaker.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithCircuitBreaker(Action<CircuitBreakerOptions> configureCircuitBreaker)
    {
        _options.CircuitBreakerOptions ??= new CircuitBreakerOptions();
        configureCircuitBreaker(_options.CircuitBreakerOptions);
        return this;
    }

    /// <summary>
    /// Configures the maximum number of concurrent tasks.
    /// </summary>
    /// <param name="maxConcurrent">Maximum number of concurrent tasks.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithConcurrency(int maxConcurrent)
    {
        _options.MaxConcurrentTasks = maxConcurrent;
        return this;
    }

    /// <summary>
    /// Configures the default timeout for individual tasks.
    /// </summary>
    /// <param name="timeout">Default task timeout.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithTimeout(TimeSpan timeout)
    {
        _options.DefaultTaskTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Configures whether to continue processing when a task fails.
    /// </summary>
    /// <param name="continueOnFailure">True to continue on failure; otherwise, false.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithContinueOnFailure(bool continueOnFailure = true)
    {
        _options.ContinueOnTaskFailure = continueOnFailure;
        return this;
    }

    /// <summary>
    /// Configures whether to collect detailed telemetry.
    /// </summary>
    /// <param name="enableTelemetry">True to enable detailed telemetry; otherwise, false.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithTelemetry(bool enableTelemetry = true)
    {
        _options.EnableDetailedTelemetry = enableTelemetry;
        return this;
    }

    /// <summary>
    /// Configures telemetry export options.
    /// </summary>
    /// <param name="exporter">The telemetry exporter to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithTelemetryExporter(ITelemetryExporter exporter)
    {
        _options.TelemetryExporter = exporter;
        return this;
    }

    /// <summary>
    /// Configures progress reporting.
    /// </summary>
    /// <param name="enableProgressReporting">True to enable progress reporting; otherwise, false.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithProgressReporting(bool enableProgressReporting = true)
    {
        _options.EnableProgressReporting = enableProgressReporting;
        return this;
    }

    /// <summary>
    /// Configures memory pooling for better performance.
    /// </summary>
    /// <param name="enablePooling">True to enable memory pooling; otherwise, false.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithMemoryPooling(bool enablePooling = true)
    {
        _options.EnableMemoryPooling = enablePooling;
        return this;
    }

    /// <summary>
    /// Configures custom options using an action.
    /// </summary>
    /// <param name="configureOptions">Action to configure the options.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithOptions(Action<TaskListProcessorOptions> configureOptions)
    {
        configureOptions(_options);
        return this;
    }

    /// <summary>
    /// Sets predefined configuration for common scenarios.
    /// </summary>
    /// <param name="preset">The configuration preset to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TaskListProcessorBuilder WithPreset(TaskProcessorPreset preset)
    {
        switch (preset)
        {
            case TaskProcessorPreset.HighThroughput:
                WithConcurrency(Environment.ProcessorCount * 4)
                    .WithRetry(RetryPolicies.Simple)
                    .WithTelemetry(false)
                    .WithMemoryPooling(true);
                break;

            case TaskProcessorPreset.Resilient:
                WithRetry(RetryPolicies.Network)
                    .WithCircuitBreaker(5, TimeSpan.FromMinutes(2))
                    .WithContinueOnFailure(true)
                    .WithTelemetry(true);
                break;

            case TaskProcessorPreset.LowLatency:
                WithConcurrency(Environment.ProcessorCount * 2)
                    .WithTimeout(TimeSpan.FromSeconds(5))
                    .WithRetry(RetryPolicies.None)
                    .WithMemoryPooling(true);
                break;

            case TaskProcessorPreset.Balanced:
                WithConcurrency(Environment.ProcessorCount * 2)
                    .WithRetry(3, TimeSpan.FromSeconds(1))
                    .WithCircuitBreaker(3, TimeSpan.FromMinutes(1))
                    .WithTelemetry(true);
                break;

            case TaskProcessorPreset.Development:
                WithConcurrency(2)
                    .WithRetry(RetryPolicies.Simple)
                    .WithTelemetry(true)
                    .WithProgressReporting(true);
                break;
        }
        return this;
    }

    /// <summary>
    /// Builds the configured TaskListProcessor instance.
    /// </summary>
    /// <returns>A new TaskListProcessor instance with the configured options.</returns>
    public TaskListProcessorEnhanced Build()
    {
        // Validate configuration
        var validationResults = new List<string>();

        if (_options.RetryPolicy != null)
        {
            var retryValidation = _options.RetryPolicy.Validate();
            if (!retryValidation.IsValid)
                validationResults.AddRange(retryValidation.Errors);
        }

        if (_options.CircuitBreakerOptions != null)
        {
            var cbValidation = _options.CircuitBreakerOptions.Validate();
            if (!cbValidation.IsValid)
                validationResults.AddRange(cbValidation.Errors);
        }

        if (_options.MaxConcurrentTasks <= 0)
            validationResults.Add("MaxConcurrentTasks must be greater than 0");

        if (_options.DefaultTaskTimeout <= TimeSpan.Zero)
            validationResults.Add("DefaultTaskTimeout must be positive");

        if (validationResults.Any())
        {
            throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", validationResults)}");
        }

        return new TaskListProcessorEnhanced(_name, _logger, _options);
    }

    /// <summary>
    /// Builds the configured TaskListProcessor instance asynchronously with initialization.
    /// </summary>
    /// <returns>A new TaskListProcessor instance with the configured options.</returns>
    public async Task<TaskListProcessorEnhanced> BuildAsync()
    {
        var processor = Build();
        await processor.InitializeAsync();
        return processor;
    }
}

/// <summary>
/// Predefined configuration presets for common scenarios.
/// </summary>
public enum TaskProcessorPreset
{
    /// <summary>
    /// Optimized for maximum throughput with minimal overhead.
    /// </summary>
    HighThroughput,

    /// <summary>
    /// Optimized for resilience with comprehensive error handling.
    /// </summary>
    Resilient,

    /// <summary>
    /// Optimized for low latency with minimal delays.
    /// </summary>
    LowLatency,

    /// <summary>
    /// Balanced configuration suitable for most scenarios.
    /// </summary>
    Balanced,

    /// <summary>
    /// Development-friendly configuration with verbose logging.
    /// </summary>
    Development
}

/// <summary>
/// Extension methods for common builder patterns.
/// </summary>
public static class TaskListProcessorBuilderExtensions
{
    /// <summary>
    /// Configures the processor for API integration scenarios.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="maxConcurrent">Maximum concurrent API calls.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static TaskListProcessorBuilder ForApiIntegration(this TaskListProcessorBuilder builder, int maxConcurrent = 10)
    {
        return builder
            .WithConcurrency(maxConcurrent)
            .WithRetry(RetryPolicies.Network)
            .WithCircuitBreaker(5, TimeSpan.FromMinutes(2))
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithTelemetry(true);
    }

    /// <summary>
    /// Configures the processor for data processing scenarios.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enableParallelism">Whether to enable high parallelism.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static TaskListProcessorBuilder ForDataProcessing(this TaskListProcessorBuilder builder, bool enableParallelism = true)
    {
        var concurrency = enableParallelism ? Environment.ProcessorCount * 4 : Environment.ProcessorCount;

        return builder
            .WithConcurrency(concurrency)
            .WithRetry(RetryPolicies.Simple)
            .WithMemoryPooling(true)
            .WithProgressReporting(true)
            .WithTelemetry(true);
    }

    /// <summary>
    /// Configures the processor for background job scenarios.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static TaskListProcessorBuilder ForBackgroundJobs(this TaskListProcessorBuilder builder)
    {
        return builder
            .WithConcurrency(Environment.ProcessorCount)
            .WithRetry(RetryPolicies.Critical)
            .WithCircuitBreaker(3, TimeSpan.FromMinutes(5))
            .WithContinueOnFailure(true)
            .WithTelemetry(true)
            .WithProgressReporting(true);
    }

    /// <summary>
    /// Configures the processor for microservices communication.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static TaskListProcessorBuilder ForMicroservices(this TaskListProcessorBuilder builder)
    {
        return builder
            .WithConcurrency(Environment.ProcessorCount * 2)
            .WithRetry(retry =>
            {
                retry.MaxRetryAttempts = 3;
                retry.BaseDelay = TimeSpan.FromMilliseconds(100);
                retry.BackoffStrategy = BackoffStrategy.ExponentialWithJitter;
                retry.JitterFactor = 0.2;
            })
            .WithCircuitBreaker(circuitBreaker =>
            {
                circuitBreaker.FailureThreshold = 5;
                circuitBreaker.TimeWindow = TimeSpan.FromMinutes(1);
                circuitBreaker.OpenDuration = TimeSpan.FromSeconds(30);
            })
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithTelemetry(true);
    }
}

/// <summary>
/// Provides factory methods for creating commonly configured processors.
/// </summary>
public static class TaskListProcessorFactory
{
    /// <summary>
    /// Creates a simple processor with basic configuration.
    /// </summary>
    /// <param name="name">The processor name.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>A configured processor instance.</returns>
    public static TaskListProcessorEnhanced CreateSimple(string name, ILogger? logger = null)
    {
        return TaskListProcessorBuilder
            .Create(name)
            .WithLogger(logger)
            .WithPreset(TaskProcessorPreset.Balanced)
            .Build();
    }

    /// <summary>
    /// Creates a high-performance processor optimized for throughput.
    /// </summary>
    /// <param name="name">The processor name.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>A configured processor instance.</returns>
    public static TaskListProcessorEnhanced CreateHighThroughput(string name, ILogger? logger = null)
    {
        return TaskListProcessorBuilder
            .Create(name)
            .WithLogger(logger)
            .WithPreset(TaskProcessorPreset.HighThroughput)
            .Build();
    }

    /// <summary>
    /// Creates a resilient processor with comprehensive error handling.
    /// </summary>
    /// <param name="name">The processor name.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>A configured processor instance.</returns>
    public static TaskListProcessorEnhanced CreateResilient(string name, ILogger? logger = null)
    {
        return TaskListProcessorBuilder
            .Create(name)
            .WithLogger(logger)
            .WithPreset(TaskProcessorPreset.Resilient)
            .Build();
    }
}