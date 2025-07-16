using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskListProcessing.Decorators;
using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Extensions;

/// <summary>
/// Implementation of fluent builder for TaskProcessor configuration.
/// </summary>
internal class TaskProcessorBuilder : ITaskProcessorBuilder
{
    public IServiceCollection Services { get; }

    public TaskProcessorBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public ITaskProcessorBuilder WithLogging()
    {
        Services.Decorate<ITaskProcessor, LoggingTaskProcessorDecorator>();
        Services.Decorate<ITaskBatchProcessor, LoggingTaskBatchProcessorDecorator>();
        Services.Decorate<ITaskStreamProcessor, LoggingTaskStreamProcessorDecorator>();
        Services.Decorate<ITaskTelemetryProvider, LoggingTaskTelemetryProviderDecorator>();
        return this;
    }

    public ITaskProcessorBuilder WithMetrics()
    {
        Services.Decorate<ITaskProcessor, MetricsTaskProcessorDecorator>();
        Services.Decorate<ITaskBatchProcessor, MetricsTaskBatchProcessorDecorator>();
        Services.Decorate<ITaskStreamProcessor, MetricsTaskStreamProcessorDecorator>();
        Services.Decorate<ITaskTelemetryProvider, MetricsTaskTelemetryProviderDecorator>();
        return this;
    }

    public ITaskProcessorBuilder WithCircuitBreaker()
    {
        Services.Decorate<ITaskProcessor, CircuitBreakerTaskProcessorDecorator>();
        Services.Decorate<ITaskBatchProcessor, CircuitBreakerTaskBatchProcessorDecorator>();
        return this;
    }

    public ITaskProcessorBuilder WithRetryPolicy()
    {
        Services.Decorate<ITaskProcessor, RetryTaskProcessorDecorator>();
        Services.Decorate<ITaskBatchProcessor, RetryTaskBatchProcessorDecorator>();
        return this;
    }

    public ITaskProcessorBuilder WithTelemetryExport()
    {
        Services.Decorate<ITaskTelemetryProvider, TelemetryExportDecorator>();
        return this;
    }

    public ITaskProcessorBuilder WithAllDecorators()
    {
        return WithLogging()
            .WithMetrics()
            .WithCircuitBreaker()
            .WithRetryPolicy()
            .WithTelemetryExport();
    }
}
