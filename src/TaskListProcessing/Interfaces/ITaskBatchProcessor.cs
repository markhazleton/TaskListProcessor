using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Models;

namespace TaskListProcessing.Interfaces;

/// <summary>
/// Interface for processing multiple tasks concurrently with comprehensive error handling and telemetry.
/// </summary>
public interface ITaskBatchProcessor
{
    /// <summary>
    /// Executes multiple tasks concurrently with comprehensive error handling and telemetry.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="progress">Optional progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    Task ProcessTasksAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes tasks with dependency resolution and scheduling.
    /// </summary>
    /// <param name="taskDefinitions">Task definitions with dependencies and priorities.</param>
    /// <param name="progress">Optional progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    Task ProcessTaskDefinitionsAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the collection of task results (thread-safe).
    /// </summary>
    IReadOnlyCollection<ITaskResult> TaskResults { get; }

    /// <summary>
    /// Gets the current progress information.
    /// </summary>
    TaskProgress CurrentProgress { get; }

    /// <summary>
    /// Event raised when progress is updated.
    /// </summary>
    event EventHandler<TaskProgress>? ProgressChanged;
}
