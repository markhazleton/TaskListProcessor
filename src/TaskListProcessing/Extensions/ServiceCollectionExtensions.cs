using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskListProcessing.Core;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Options;

namespace TaskListProcessing.Extensions;

/// <summary>
/// Extension methods for configuring TaskListProcessor services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TaskListProcessor services to the DI container with default configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>A fluent configuration builder.</returns>
    public static ITaskProcessorBuilder AddTaskListProcessor(this IServiceCollection services)
    {
        return services.AddTaskListProcessor(_ => { });
    }

    /// <summary>
    /// Adds TaskListProcessor services to the DI container with custom configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Configuration delegate for TaskListProcessor options.</param>
    /// <returns>A fluent configuration builder.</returns>
    public static ITaskProcessorBuilder AddTaskListProcessor(
        this IServiceCollection services,
        Action<TaskListProcessorOptions> configure)
    {
        // Configure options
        services.Configure(configure);
        services.AddSingleton<IValidateOptions<TaskListProcessorOptions>, TaskListProcessorOptions>();

        // Register core services
        services.TryAddSingleton<ITaskProcessor, TaskProcessor>();
        services.TryAddSingleton<ITaskBatchProcessor, TaskBatchProcessor>();
        services.TryAddSingleton<ITaskStreamProcessor, TaskStreamProcessor>();
        services.TryAddSingleton<ITaskTelemetryProvider, TaskTelemetryProvider>();

        // Register the enhanced processor for backward compatibility
        services.TryAddSingleton<TaskListProcessorEnhanced>(provider =>
        {
            var options = provider.GetService<IOptions<TaskListProcessorOptions>>()?.Value;
            var logger = provider.GetService<ILogger<TaskListProcessorEnhanced>>();
            return new TaskListProcessorEnhanced("DI-Processor", logger, options);
        });

        return new TaskProcessorBuilder(services);
    }

    /// <summary>
    /// Adds TaskListProcessor services with a named configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="name">The name of the processor instance.</param>
    /// <param name="configure">Configuration delegate for TaskListProcessor options.</param>
    /// <returns>A fluent configuration builder.</returns>
    public static ITaskProcessorBuilder AddTaskListProcessor(
        this IServiceCollection services,
        string name,
        Action<TaskListProcessorOptions> configure)
    {
        // Configure named options
        services.Configure(name, configure);

        // Register named services
        services.TryAddSingleton<ITaskProcessor>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<TaskListProcessorOptions>>()?.Get(name);
            var logger = provider.GetService<ILogger<TaskProcessor>>();
            return new TaskProcessor(name, logger, options);
        });

        services.TryAddSingleton<ITaskBatchProcessor>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<TaskListProcessorOptions>>()?.Get(name);
            var logger = provider.GetService<ILogger<TaskBatchProcessor>>();
            return new TaskBatchProcessor(name, logger, options);
        });

        services.TryAddSingleton<ITaskStreamProcessor>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<TaskListProcessorOptions>>()?.Get(name);
            var logger = provider.GetService<ILogger<TaskStreamProcessor>>();
            return new TaskStreamProcessor(name, logger, options);
        });

        services.TryAddSingleton<ITaskTelemetryProvider>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<TaskListProcessorOptions>>()?.Get(name);
            var logger = provider.GetService<ILogger<TaskTelemetryProvider>>();
            return new TaskTelemetryProvider(name, logger, options);
        });

        return new TaskProcessorBuilder(services);
    }
}

/// <summary>
/// Extension methods for decorator pattern support in DI.
/// </summary>
public static class ServiceCollectionDecoratorExtensions
{
    /// <summary>
    /// Decorates a service with the specified decorator type.
    /// </summary>
    /// <typeparam name="TInterface">The service interface type.</typeparam>
    /// <typeparam name="TDecorator">The decorator type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection Decorate<TInterface, TDecorator>(this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        // Find the existing service descriptor
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TInterface));
        if (descriptor == null)
        {
            throw new InvalidOperationException($"Service {typeof(TInterface).Name} is not registered.");
        }

        // Remove the existing descriptor
        services.Remove(descriptor);

        // Register the decorator
        services.Add(ServiceDescriptor.Describe(
            typeof(TInterface),
            provider =>
            {
                // Create the original service
                var originalService = descriptor.ImplementationType != null
                    ? ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType)
                    : descriptor.ImplementationFactory?.Invoke(provider)
                    ?? descriptor.ImplementationInstance;

                // Create the decorator with the original service
                return ActivatorUtilities.CreateInstance(provider, typeof(TDecorator), originalService ?? throw new InvalidOperationException("Failed to create original service"));
            },
            descriptor.Lifetime));

        return services;
    }
}
