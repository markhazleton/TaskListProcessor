using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Interfaces;

/// <summary>
/// Interface for telemetry collection and health monitoring capabilities.
/// </summary>
public interface ITaskTelemetryProvider
{
    /// <summary>
    /// Gets the collection of telemetry data (thread-safe).
    /// </summary>
    IReadOnlyCollection<TaskTelemetry> Telemetry { get; }

    /// <summary>
    /// Gets comprehensive telemetry summary statistics.
    /// </summary>
    /// <returns>Summary of telemetry data.</returns>
    TelemetrySummary GetTelemetrySummary();

    /// <summary>
    /// Gets the current circuit breaker state.
    /// </summary>
    CircuitBreakerStats? CircuitBreakerStats { get; }

    /// <summary>
    /// Performs a health check on the processor.
    /// </summary>
    /// <returns>Health check result.</returns>
    HealthCheckResult PerformHealthCheck();

    /// <summary>
    /// Exports telemetry data to configured exporters.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the export operation.</param>
    /// <returns>A task representing the export operation.</returns>
    Task ExportTelemetryAsync(CancellationToken cancellationToken = default);
}
