using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Models;

namespace TaskListProcessing.Interfaces;

/// <summary>
/// Interface for streaming task results as they complete using async enumerable.
/// </summary>
public interface ITaskStreamProcessor
{
    /// <summary>
    /// Streams task results as they complete using async enumerable.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>An async enumerable of task results.</returns>
    IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTasksStreamAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams task results from task definitions.
    /// </summary>
    /// <param name="taskDefinitions">Task definitions to process.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>An async enumerable of task results.</returns>
    IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTaskDefinitionsStreamAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        CancellationToken cancellationToken = default);
}
