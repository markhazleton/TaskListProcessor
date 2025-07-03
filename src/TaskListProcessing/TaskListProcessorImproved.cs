using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TaskListProcessing;

/// <summary>
/// Provides functionality to process a list of asynchronous tasks, collect their results, and gather telemetry data.
/// Thread-safe implementation with improved error handling and telemetry.
/// </summary>
public class TaskListProcessorImproved : IDisposable
{
    private readonly ILogger? _logger;
    private readonly ConcurrentBag<ITaskResult> _taskResults = new();
    private readonly ConcurrentBag<TaskTelemetry> _telemetry = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the TaskListProcessorImproved class.
    /// </summary>
    /// <param name="taskListName">Optional name for the task list.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    public TaskListProcessorImproved(string? taskListName = null, ILogger? logger = null)
    {
        TaskListName = taskListName;
        _logger = logger;
    }

    /// <summary>
    /// Gets the name of the task list.
    /// </summary>
    public string? TaskListName { get; }

    /// <summary>
    /// Gets the collection of task results (thread-safe).
    /// </summary>
    public IReadOnlyCollection<ITaskResult> TaskResults => _taskResults.ToList().AsReadOnly();

    /// <summary>
    /// Gets the collection of telemetry data (thread-safe).
    /// </summary>
    public IReadOnlyCollection<TaskTelemetry> Telemetry => _telemetry.ToList().AsReadOnly();

    /// <summary>
    /// Executes multiple tasks concurrently and collects their results.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public async Task ProcessTasksAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        var tasks = taskFactories.Select(kvp =>
            ProcessSingleTaskAsync(kvp.Key, kvp.Value, cancellationToken));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Executes a single task with telemetry and error handling.
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

        var stopwatch = Stopwatch.StartNew();
        var taskResult = new EnhancedTaskResult<T> { Name = taskName };
        Exception? exception = null;

        try
        {
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            taskResult.Data = await task.WaitAsync(combinedCts.Token);
            taskResult.IsSuccessful = true;

            _logger?.LogDebug("Task '{TaskName}' completed successfully in {ElapsedMs}ms",
                taskName, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            exception = new OperationCanceledException($"Task '{taskName}' was cancelled.");
            taskResult.ErrorMessage = "Task was cancelled";
            _logger?.LogWarning("Task '{TaskName}' was cancelled after {ElapsedMs}ms",
                taskName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            exception = ex;
            taskResult.ErrorMessage = ex.Message;
            _logger?.LogError(ex, "Task '{TaskName}' failed after {ElapsedMs}ms",
                taskName, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            stopwatch.Stop();
            var telemetry = new TaskTelemetry(
                taskName,
                stopwatch.ElapsedMilliseconds,
                exception?.GetType().Name,
                exception?.Message,
                taskResult.IsSuccessful);

            _telemetry.Add(telemetry);
            _taskResults.Add(taskResult);
        }

        return taskResult;
    }

    /// <summary>
    /// Waits for all provided tasks to complete with enhanced error handling and logging.
    /// </summary>
    /// <param name="tasks">The tasks to wait for.</param>
    /// <param name="logger">Optional logger for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public static async Task WhenAllWithLoggingAsync(
        IEnumerable<Task> tasks,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var taskList = tasks.ToList();
        if (!taskList.Any()) return;

        try
        {
            await Task.WhenAll(taskList).WaitAsync(cancellationToken);
            logger?.LogInformation("All {TaskCount} tasks completed successfully", taskList.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning("Task execution was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            var completedTasks = taskList.Count(t => t.IsCompletedSuccessfully);
            var faultedTasks = taskList.Count(t => t.IsFaulted);
            var cancelledTasks = taskList.Count(t => t.IsCanceled);

            logger?.LogError(ex,
                "Task execution completed with errors. Completed: {Completed}, Faulted: {Faulted}, Cancelled: {Cancelled}",
                completedTasks, faultedTasks, cancelledTasks);
        }
    }

    /// <summary>
    /// Gets telemetry summary statistics.
    /// </summary>
    /// <returns>Summary of telemetry data.</returns>
    public TelemetrySummary GetTelemetrySummary()
    {
        var telemetryList = _telemetry.ToList();

        return new TelemetrySummary
        {
            TotalTasks = telemetryList.Count,
            SuccessfulTasks = telemetryList.Count(t => t.IsSuccessful),
            FailedTasks = telemetryList.Count(t => !t.IsSuccessful),
            AverageExecutionTime = telemetryList.Any() ? telemetryList.Average(t => t.ElapsedMilliseconds) : 0,
            TotalExecutionTime = telemetryList.Sum(t => t.ElapsedMilliseconds),
            MinExecutionTime = telemetryList.Any() ? telemetryList.Min(t => t.ElapsedMilliseconds) : 0,
            MaxExecutionTime = telemetryList.Any() ? telemetryList.Max(t => t.ElapsedMilliseconds) : 0
        };
    }

    private async Task ProcessSingleTaskAsync(
        string taskName,
        Func<CancellationToken, Task<object?>> taskFactory,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var taskResult = new EnhancedTaskResult<object> { Name = taskName };
        Exception? exception = null;

        try
        {
            taskResult.Data = await taskFactory(cancellationToken);
            taskResult.IsSuccessful = true;
        }
        catch (Exception ex)
        {
            exception = ex;
            taskResult.ErrorMessage = ex.Message;
        }
        finally
        {
            stopwatch.Stop();
            var telemetry = new TaskTelemetry(
                taskName,
                stopwatch.ElapsedMilliseconds,
                exception?.GetType().Name,
                exception?.Message,
                taskResult.IsSuccessful);

            _telemetry.Add(telemetry);
            _taskResults.Add(taskResult);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Clean up resources if needed
            _disposed = true;
        }
    }
}
