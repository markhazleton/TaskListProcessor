using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Telemetry export decorator for ITaskTelemetryProvider that adds automatic telemetry export capabilities.
/// </summary>
public class TelemetryExportDecorator : ITaskTelemetryProvider
{
    private readonly ITaskTelemetryProvider _inner;

    public TelemetryExportDecorator(ITaskTelemetryProvider inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyCollection<TaskTelemetry> Telemetry => _inner.Telemetry;
    public CircuitBreakerStats? CircuitBreakerStats => _inner.CircuitBreakerStats;

    public TelemetrySummary GetTelemetrySummary()
    {
        var summary = _inner.GetTelemetrySummary();
        // TODO: Optionally trigger export based on summary
        return summary;
    }

    public HealthCheckResult PerformHealthCheck()
    {
        var result = _inner.PerformHealthCheck();
        // TODO: Optionally trigger export based on health check result
        return result;
    }

    public async Task ExportTelemetryAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Add additional export capabilities (multiple exporters, batching, etc.)
        await _inner.ExportTelemetryAsync(cancellationToken);
    }
}
