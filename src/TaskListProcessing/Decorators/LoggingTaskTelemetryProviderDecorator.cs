using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Logging decorator for ITaskTelemetryProvider that adds comprehensive logging around telemetry operations.
/// </summary>
public class LoggingTaskTelemetryProviderDecorator : ITaskTelemetryProvider
{
    private readonly ITaskTelemetryProvider _inner;
    private readonly ILogger<LoggingTaskTelemetryProviderDecorator> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingTaskTelemetryProviderDecorator class.
    /// </summary>
    /// <param name="inner">The inner task telemetry provider to decorate.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public LoggingTaskTelemetryProviderDecorator(ITaskTelemetryProvider inner, ILogger<LoggingTaskTelemetryProviderDecorator> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the collection of telemetry data (thread-safe).
    /// </summary>
    public IReadOnlyCollection<TaskTelemetry> Telemetry
    {
        get
        {
            var telemetry = _inner.Telemetry;
            _logger.LogDebug("Retrieved {TelemetryCount} telemetry records", telemetry.Count);
            return telemetry;
        }
    }

    /// <summary>
    /// Gets the current circuit breaker state.
    /// </summary>
    public CircuitBreakerStats? CircuitBreakerStats
    {
        get
        {
            var stats = _inner.CircuitBreakerStats;
            if (stats != null)
            {
                _logger.LogDebug("Circuit breaker state: {State}, Failures: {FailureCount}",
                    stats.State, stats.FailureCount);
            }
            return stats;
        }
    }

    /// <summary>
    /// Gets comprehensive telemetry summary statistics.
    /// </summary>
    /// <returns>Summary of telemetry data.</returns>
    public TelemetrySummary GetTelemetrySummary()
    {
        _logger.LogDebug("Generating telemetry summary");

        try
        {
            var summary = _inner.GetTelemetrySummary();

            _logger.LogInformation("Telemetry summary: {TotalTasks} total, {SuccessfulTasks} successful, {FailedTasks} failed, {SuccessRate}% success rate, {AvgExecutionTime}ms avg execution time",
                summary.TotalTasks, summary.SuccessfulTasks, summary.FailedTasks, summary.SuccessRate, summary.AverageExecutionTime);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating telemetry summary");
            throw;
        }
    }

    /// <summary>
    /// Performs a health check on the processor.
    /// </summary>
    /// <returns>Health check result.</returns>
    public HealthCheckResult PerformHealthCheck()
    {
        _logger.LogDebug("Performing health check");

        try
        {
            var result = _inner.PerformHealthCheck();

            if (result.IsHealthy)
            {
                _logger.LogInformation("Health check passed: {Message}", result.Message);
            }
            else
            {
                _logger.LogWarning("Health check failed: {Message}", result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing health check");
            throw;
        }
    }

    /// <summary>
    /// Exports telemetry data to configured exporters.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the export operation.</param>
    /// <returns>A task representing the export operation.</returns>
    public async Task ExportTelemetryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting telemetry export");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _inner.ExportTelemetryAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation("Telemetry export completed successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Telemetry export failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
