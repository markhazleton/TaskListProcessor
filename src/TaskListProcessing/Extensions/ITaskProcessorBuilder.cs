using Microsoft.Extensions.DependencyInjection;

namespace TaskListProcessing.Extensions;

/// <summary>
/// Fluent interface for configuring TaskProcessor services with decorators.
/// </summary>
public interface ITaskProcessorBuilder
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds logging decorator to task processors.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    ITaskProcessorBuilder WithLogging();

    /// <summary>
    /// Adds metrics collection decorator to task processors.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    ITaskProcessorBuilder WithMetrics();

    /// <summary>
    /// Adds circuit breaker decorator to task processors.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    ITaskProcessorBuilder WithCircuitBreaker();

    /// <summary>
    /// Adds retry policy decorator to task processors.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    ITaskProcessorBuilder WithRetryPolicy();

    /// <summary>
    /// Adds telemetry export capabilities.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    ITaskProcessorBuilder WithTelemetryExport();

    /// <summary>
    /// Adds all standard decorators (logging, metrics, circuit breaker, retry).
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    ITaskProcessorBuilder WithAllDecorators();
}
