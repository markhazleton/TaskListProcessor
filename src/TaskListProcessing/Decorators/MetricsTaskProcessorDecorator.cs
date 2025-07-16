using System;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Metrics decorator for ITaskProcessor that adds metrics collection around task execution.
/// </summary>
public class MetricsTaskProcessorDecorator : ITaskProcessor
{
    private readonly ITaskProcessor _inner;
    // TODO: Add metrics collector dependency

    /// <summary>
    /// Initializes a new instance of the MetricsTaskProcessorDecorator class.
    /// </summary>
    /// <param name="inner">The inner task processor to decorate.</param>
    public MetricsTaskProcessorDecorator(ITaskProcessor inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <summary>
    /// Gets the name of the task processor.
    /// </summary>
    public string Name => _inner.Name;

    /// <summary>
    /// Event raised when a task completes (successfully or with error).
    /// </summary>
    public event EventHandler<ITaskResult>? TaskCompleted
    {
        add => _inner.TaskCompleted += value;
        remove => _inner.TaskCompleted -= value;
    }

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
        // TODO: Start metrics collection
        var result = await _inner.ExecuteTaskAsync(taskName, task, cancellationToken);
        // TODO: Record metrics (duration, success/failure, etc.)
        return result;
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
        // TODO: Start metrics collection
        var result = await _inner.ExecuteTaskAsync(taskName, taskFactory, cancellationToken);
        // TODO: Record metrics (duration, success/failure, etc.)
        return result;
    }
}
