using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Core;

/// <summary>
/// Core implementation of batch task processor for multiple task execution.
/// </summary>
public class TaskBatchProcessor : ITaskBatchProcessor, IDisposable
{
    private readonly ILogger? _logger;
    private readonly TaskListProcessorOptions _options;
    private readonly TaskListProcessorEnhanced _enhancedProcessor;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the TaskBatchProcessor class.
    /// </summary>
    /// <param name="name">The name of the task processor.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    /// <param name="options">Configuration options.</param>
    public TaskBatchProcessor(string? name = null, ILogger? logger = null, TaskListProcessorOptions? options = null)
    {
        Name = name ?? "TaskBatchProcessor";
        _logger = logger;
        _options = options ?? new TaskListProcessorOptions();
        _enhancedProcessor = new TaskListProcessorEnhanced(Name, _logger, _options);

        // Forward events
        _enhancedProcessor.TaskCompleted += (sender, result) => TaskCompleted?.Invoke(this, result);
        _enhancedProcessor.ProgressChanged += (sender, progress) => ProgressChanged?.Invoke(this, progress);
    }

    /// <summary>
    /// Gets the name of the task processor.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the collection of task results (thread-safe).
    /// </summary>
    public IReadOnlyCollection<ITaskResult> TaskResults => _enhancedProcessor.TaskResults;

    /// <summary>
    /// Gets the current progress information.
    /// </summary>
    public TaskProgress CurrentProgress => _enhancedProcessor.CurrentProgress;

    /// <summary>
    /// Event raised when progress is updated.
    /// </summary>
    public event EventHandler<TaskProgress>? ProgressChanged;

    /// <summary>
    /// Event raised when a task completes (successfully or with error).
    /// </summary>
    public event EventHandler<ITaskResult>? TaskCompleted;

    /// <summary>
    /// Executes multiple tasks concurrently with comprehensive error handling and telemetry.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="progress">Optional progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public async Task ProcessTasksAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        _logger?.LogInformation("Starting batch processing of {TaskCount} tasks", taskFactories.Count);

        try
        {
            await _enhancedProcessor.ProcessTasksAsync(taskFactories, progress, cancellationToken);
            _logger?.LogInformation("Completed batch processing of {TaskCount} tasks", taskFactories.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during batch processing of {TaskCount} tasks", taskFactories.Count);
            throw;
        }
    }

    /// <summary>
    /// Executes tasks with dependency resolution and scheduling.
    /// </summary>
    /// <param name="taskDefinitions">Task definitions with dependencies and priorities.</param>
    /// <param name="progress">Optional progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public async Task ProcessTaskDefinitionsAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskDefinitions);

        var taskList = taskDefinitions.ToList();
        _logger?.LogInformation("Starting batch processing of {TaskCount} task definitions", taskList.Count);

        try
        {
            await _enhancedProcessor.ProcessTaskDefinitionsAsync(taskList, progress, cancellationToken);
            _logger?.LogInformation("Completed batch processing of {TaskCount} task definitions", taskList.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during batch processing of {TaskCount} task definitions", taskList.Count);
            throw;
        }
    }

    /// <summary>
    /// Releases the resources used by the TaskBatchProcessor.
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
