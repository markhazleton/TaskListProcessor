using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Logging decorator for ITaskProcessor that adds comprehensive logging around task execution.
/// </summary>
public class LoggingTaskProcessorDecorator : ITaskProcessor
{
    private readonly ITaskProcessor _inner;
    private readonly ILogger<LoggingTaskProcessorDecorator> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingTaskProcessorDecorator class.
    /// </summary>
    /// <param name="inner">The inner task processor to decorate.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public LoggingTaskProcessorDecorator(ITaskProcessor inner, ILogger<LoggingTaskProcessorDecorator> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        _logger.LogInformation("Starting execution of task '{TaskName}' of type {TaskType}",
            taskName, typeof(T).Name);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteTaskAsync(taskName, task, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccessful)
            {
                _logger.LogInformation("Task '{TaskName}' completed successfully in {ElapsedMs}ms",
                    taskName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("Task '{TaskName}' failed in {ElapsedMs}ms: {ErrorMessage}",
                    taskName, stopwatch.ElapsedMilliseconds, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task '{TaskName}' threw exception after {ElapsedMs}ms",
                taskName, stopwatch.ElapsedMilliseconds);
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
        _logger.LogInformation("Starting execution of task '{TaskName}' using factory function", taskName);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteTaskAsync(taskName, taskFactory, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccessful)
            {
                _logger.LogInformation("Task '{TaskName}' completed successfully in {ElapsedMs}ms",
                    taskName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("Task '{TaskName}' failed in {ElapsedMs}ms: {ErrorMessage}",
                    taskName, stopwatch.ElapsedMilliseconds, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task '{TaskName}' threw exception after {ElapsedMs}ms",
                taskName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
