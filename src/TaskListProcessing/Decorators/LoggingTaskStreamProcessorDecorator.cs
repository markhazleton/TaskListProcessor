using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Logging decorator for ITaskStreamProcessor that adds comprehensive logging around stream task execution.
/// </summary>
public class LoggingTaskStreamProcessorDecorator : ITaskStreamProcessor
{
    private readonly ITaskStreamProcessor _inner;
    private readonly ILogger<LoggingTaskStreamProcessorDecorator> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingTaskStreamProcessorDecorator class.
    /// </summary>
    /// <param name="inner">The inner task stream processor to decorate.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public LoggingTaskStreamProcessorDecorator(ITaskStreamProcessor inner, ILogger<LoggingTaskStreamProcessorDecorator> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
        _logger.LogInformation("Starting stream processing of {TaskCount} tasks", taskFactories.Count);

        var processedCount = 0;
        var successCount = 0;
        var failureCount = 0;

        try
        {
            await foreach (var result in _inner.ProcessTasksStreamAsync(taskFactories, cancellationToken))
            {
                processedCount++;

                if (result.IsSuccessful)
                {
                    successCount++;
                    _logger.LogDebug("Stream processed task '{TaskName}' successfully ({ProcessedCount}/{TotalCount})",
                        result.Name, processedCount, taskFactories.Count);
                }
                else
                {
                    failureCount++;
                    _logger.LogWarning("Stream processed task '{TaskName}' with failure ({ProcessedCount}/{TotalCount}): {ErrorMessage}",
                        result.Name, processedCount, taskFactories.Count, result.ErrorMessage);
                }

                yield return result;
            }

            _logger.LogInformation("Stream processing completed - {SuccessCount} successful, {FailureCount} failed",
                successCount, failureCount);
        }
        finally
        {
            _logger.LogDebug("Stream processing cleanup completed after processing {ProcessedCount} tasks", processedCount);
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
        var taskList = taskDefinitions.ToList();
        _logger.LogInformation("Starting stream processing of {TaskCount} task definitions", taskList.Count);

        var processedCount = 0;
        var successCount = 0;
        var failureCount = 0;

        try
        {
            await foreach (var result in _inner.ProcessTaskDefinitionsStreamAsync(taskList, cancellationToken))
            {
                processedCount++;

                if (result.IsSuccessful)
                {
                    successCount++;
                    _logger.LogDebug("Stream processed task definition '{TaskName}' successfully ({ProcessedCount}/{TotalCount})",
                        result.Name, processedCount, taskList.Count);
                }
                else
                {
                    failureCount++;
                    _logger.LogWarning("Stream processed task definition '{TaskName}' with failure ({ProcessedCount}/{TotalCount}): {ErrorMessage}",
                        result.Name, processedCount, taskList.Count, result.ErrorMessage);
                }

                yield return result;
            }

            _logger.LogInformation("Stream processing of task definitions completed - {SuccessCount} successful, {FailureCount} failed",
                successCount, failureCount);
        }
        finally
        {
            _logger.LogDebug("Stream processing cleanup completed after processing {ProcessedCount} task definitions", processedCount);
        }
    }
}
