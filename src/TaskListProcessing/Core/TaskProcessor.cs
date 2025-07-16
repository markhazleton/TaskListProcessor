using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Core;

/// <summary>
/// Core implementation of task processor for individual task execution.
/// </summary>
public class TaskProcessor : ITaskProcessor, IDisposable
{
    private readonly ILogger? _logger;
    private readonly TaskListProcessorOptions _options;
    private readonly TaskListProcessorEnhanced _enhancedProcessor;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the TaskProcessor class.
    /// </summary>
    /// <param name="name">The name of the task processor.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    /// <param name="options">Configuration options.</param>
    public TaskProcessor(string? name = null, ILogger? logger = null, TaskListProcessorOptions? options = null)
    {
        Name = name ?? "TaskProcessor";
        _logger = logger;
        _options = options ?? new TaskListProcessorOptions();
        _enhancedProcessor = new TaskListProcessorEnhanced(Name, _logger, _options);
    }

    /// <summary>
    /// Gets the name of the task processor.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Event raised when a task completes (successfully or with error).
    /// </summary>
    public event EventHandler<ITaskResult>? TaskCompleted;

    /// <summary>
    /// Executes a single task with comprehensive error handling and telemetry.
    /// </summary>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    public async Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Task<T> task,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);
        ArgumentNullException.ThrowIfNull(task);

        _logger?.LogDebug("Executing task '{TaskName}' of type {TaskType}", taskName, typeof(T).Name);

        try
        {
            var result = await _enhancedProcessor.ExecuteTaskAsync(taskName, task, cancellationToken);

            // Raise completion event
            TaskCompleted?.Invoke(this, result);

            _logger?.LogDebug("Task '{TaskName}' completed with success={Success}", taskName, result.IsSuccessful);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing task '{TaskName}'", taskName);
            throw;
        }
    }

    /// <summary>
    /// Executes a single task using a factory function.
    /// </summary>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskFactory">Factory function to create the task.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    public async Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Func<CancellationToken, Task<T>> taskFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);
        ArgumentNullException.ThrowIfNull(taskFactory);

        _logger?.LogDebug("Executing task '{TaskName}' using factory function", taskName);

        try
        {
            var task = taskFactory(cancellationToken);
            return await ExecuteTaskAsync(taskName, task, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing task '{TaskName}' with factory", taskName);
            throw;
        }
    }

    /// <summary>
    /// Releases the resources used by the TaskProcessor.
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
