using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Core;

/// <summary>
/// Core implementation of stream task processor for async enumerable task execution.
/// </summary>
public class TaskStreamProcessor : ITaskStreamProcessor, IDisposable
{
    private readonly ILogger? _logger;
    private readonly TaskListProcessorOptions _options;
    private readonly TaskListProcessorEnhanced _enhancedProcessor;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the TaskStreamProcessor class.
    /// </summary>
    /// <param name="name">The name of the task processor.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    /// <param name="options">Configuration options.</param>
    public TaskStreamProcessor(string? name = null, ILogger? logger = null, TaskListProcessorOptions? options = null)
    {
        Name = name ?? "TaskStreamProcessor";
        _logger = logger;
        _options = options ?? new TaskListProcessorOptions();
        _enhancedProcessor = new TaskListProcessorEnhanced(Name, _logger, _options);
    }

    /// <summary>
    /// Gets the name of the task processor.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Streams task results as they complete using async enumerable.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>An async enumerable of task results.</returns>
    public async IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTasksStreamAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        _logger?.LogInformation("Starting stream processing of {TaskCount} tasks", taskFactories.Count);

        try
        {
            await foreach (var result in _enhancedProcessor.ProcessTasksStreamAsync(taskFactories, cancellationToken))
            {
                _logger?.LogDebug("Streamed result for task '{TaskName}' with success={Success}",
                    result.Name, result.IsSuccessful);
                yield return result;
            }

            _logger?.LogInformation("Completed stream processing of {TaskCount} tasks", taskFactories.Count);
        }
        finally
        {
            _logger?.LogDebug("Stream processing cleanup completed");
        }
    }

    /// <summary>
    /// Streams task results from task definitions.
    /// </summary>
    /// <param name="taskDefinitions">Task definitions to process.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>An async enumerable of task results.</returns>
    public async IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTaskDefinitionsStreamAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskDefinitions);

        var taskList = taskDefinitions.ToList();
        _logger?.LogInformation("Starting stream processing of {TaskCount} task definitions", taskList.Count);

        try
        {
            // Convert task definitions to task factories
            var taskFactories = taskList.ToDictionary(
                td => td.Name,
                td => td.Factory);

            await foreach (var result in ProcessTasksStreamAsync(taskFactories, cancellationToken))
            {
                yield return result;
            }

            _logger?.LogInformation("Completed stream processing of {TaskCount} task definitions", taskList.Count);
        }
        finally
        {
            _logger?.LogDebug("Stream processing cleanup completed");
        }
    }

    /// <summary>
    /// Releases the resources used by the TaskStreamProcessor.
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
