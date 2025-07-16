using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Logging decorator for ITaskBatchProcessor that adds comprehensive logging around batch task execution.
/// </summary>
public class LoggingTaskBatchProcessorDecorator : ITaskBatchProcessor
{
    private readonly ITaskBatchProcessor _inner;
    private readonly ILogger<LoggingTaskBatchProcessorDecorator> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingTaskBatchProcessorDecorator class.
    /// </summary>
    /// <param name="inner">The inner task batch processor to decorate.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public LoggingTaskBatchProcessorDecorator(ITaskBatchProcessor inner, ILogger<LoggingTaskBatchProcessorDecorator> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the collection of task results (thread-safe).
    /// </summary>
    public IReadOnlyCollection<ITaskResult> TaskResults => _inner.TaskResults;

    /// <summary>
    /// Gets the current progress information.
    /// </summary>
    public TaskProgress CurrentProgress => _inner.CurrentProgress;

    /// <summary>
    /// Event raised when progress is updated.
    /// </summary>
    public event EventHandler<TaskProgress>? ProgressChanged
    {
        add => _inner.ProgressChanged += value;
        remove => _inner.ProgressChanged -= value;
    }

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
        _logger.LogInformation("Starting batch processing of {TaskCount} tasks", taskFactories.Count);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _inner.ProcessTasksAsync(taskFactories, progress, cancellationToken);

            stopwatch.Stop();

            var results = TaskResults;
            var successCount = results.Count(r => r.IsSuccessful);
            var failureCount = results.Count(r => !r.IsSuccessful);

            _logger.LogInformation("Batch processing completed in {ElapsedMs}ms - {SuccessCount} successful, {FailureCount} failed",
                stopwatch.ElapsedMilliseconds, successCount, failureCount);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Batch processing failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
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
        var taskList = taskDefinitions.ToList();
        _logger.LogInformation("Starting batch processing of {TaskCount} task definitions", taskList.Count);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _inner.ProcessTaskDefinitionsAsync(taskList, progress, cancellationToken);

            stopwatch.Stop();

            var results = TaskResults;
            var successCount = results.Count(r => r.IsSuccessful);
            var failureCount = results.Count(r => !r.IsSuccessful);

            _logger.LogInformation("Batch processing of task definitions completed in {ElapsedMs}ms - {SuccessCount} successful, {FailureCount} failed",
                stopwatch.ElapsedMilliseconds, successCount, failureCount);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Batch processing of task definitions failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
