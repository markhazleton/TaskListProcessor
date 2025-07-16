using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Metrics decorator for ITaskTelemetryProvider that adds metrics collection around telemetry operations.
/// </summary>
public class MetricsTaskTelemetryProviderDecorator : ITaskTelemetryProvider
{
    private readonly ITaskTelemetryProvider _inner;

    public MetricsTaskTelemetryProviderDecorator(ITaskTelemetryProvider inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyCollection<TaskTelemetry> Telemetry => _inner.Telemetry;
    public CircuitBreakerStats? CircuitBreakerStats => _inner.CircuitBreakerStats;

    public TelemetrySummary GetTelemetrySummary()
    {
        // TODO: Record metrics about telemetry summary generation
        return _inner.GetTelemetrySummary();
    }

    public HealthCheckResult PerformHealthCheck()
    {
        // TODO: Record metrics about health check execution
        return _inner.PerformHealthCheck();
    }

    public async Task ExportTelemetryAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Record metrics about telemetry export
        await _inner.ExportTelemetryAsync(cancellationToken);
    }
}
