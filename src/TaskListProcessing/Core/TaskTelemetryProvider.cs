using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Options;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Core;

/// <summary>
/// Core implementation of telemetry provider for task monitoring and health checks.
/// </summary>
public class TaskTelemetryProvider : ITaskTelemetryProvider, IDisposable
{
    private readonly ILogger? _logger;
    private readonly TaskListProcessorOptions _options;
    private readonly TaskListProcessorEnhanced _enhancedProcessor;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the TaskTelemetryProvider class.
    /// </summary>
    /// <param name="name">The name of the telemetry provider.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    /// <param name="options">Configuration options.</param>
    public TaskTelemetryProvider(string? name = null, ILogger? logger = null, TaskListProcessorOptions? options = null)
    {
        Name = name ?? "TaskTelemetryProvider";
        _logger = logger;
        _options = options ?? new TaskListProcessorOptions();
        _enhancedProcessor = new TaskListProcessorEnhanced(Name, _logger, _options);
    }

    /// <summary>
    /// Gets the name of the telemetry provider.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the collection of telemetry data (thread-safe).
    /// </summary>
    public IReadOnlyCollection<TaskTelemetry> Telemetry => _enhancedProcessor.Telemetry;

    /// <summary>
    /// Gets the current circuit breaker state.
    /// </summary>
    public CircuitBreakerStats? CircuitBreakerStats => _enhancedProcessor.CircuitBreakerStats;

    /// <summary>
    /// Gets comprehensive telemetry summary statistics.
    /// </summary>
    /// <returns>Summary of telemetry data.</returns>
    public TelemetrySummary GetTelemetrySummary()
    {
        _logger?.LogDebug("Retrieving telemetry summary");

        try
        {
            var summary = _enhancedProcessor.GetTelemetrySummary();
            _logger?.LogDebug("Retrieved telemetry summary: {TotalTasks} total, {SuccessfulTasks} successful, {FailedTasks} failed",
                summary.TotalTasks, summary.SuccessfulTasks, summary.FailedTasks);
            return summary;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving telemetry summary");
            throw;
        }
    }

    /// <summary>
    /// Performs a health check on the processor.
    /// </summary>
    /// <returns>Health check result.</returns>
    public HealthCheckResult PerformHealthCheck()
    {
        _logger?.LogDebug("Performing health check");

        try
        {
            var result = _enhancedProcessor.PerformHealthCheck();
            _logger?.LogDebug("Health check completed: {IsHealthy} - {Message}", result.IsHealthy, result.Message);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error performing health check");
            return new HealthCheckResult(false, $"Health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports telemetry data to configured exporters.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the export operation.</param>
    /// <returns>A task representing the export operation.</returns>
    public async Task ExportTelemetryAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting telemetry export");

        try
        {
            if (_options.TelemetryExporter != null && _options.EnableDetailedTelemetry)
            {
                await _options.TelemetryExporter.ExportAsync(Telemetry, cancellationToken);
                _logger?.LogInformation("Telemetry export completed successfully");
            }
            else
            {
                _logger?.LogDebug("Telemetry export skipped - no exporter configured or detailed telemetry disabled");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during telemetry export");
            throw;
        }
    }

    /// <summary>
    /// Releases the resources used by the TaskTelemetryProvider.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _enhancedProcessor?.Dispose();
            _disposed = true;
        }
    }
}
