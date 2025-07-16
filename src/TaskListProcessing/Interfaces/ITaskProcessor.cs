using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Models;

namespace TaskListProcessing.Interfaces;

/// <summary>
/// Core interface for executing individual tasks with comprehensive error handling and telemetry.
/// </summary>
public interface ITaskProcessor
{
    /// <summary>
    /// Gets the name of the task processor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes a single task with comprehensive error handling and telemetry.
    /// </summary>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Task<T> task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a single task using a factory function.
    /// </summary>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskFactory">Factory function to create the task.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Func<CancellationToken, Task<T>> taskFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when a task completes (successfully or with error).
    /// </summary>
    event EventHandler<ITaskResult>? TaskCompleted;
}
