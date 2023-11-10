using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TaskListProcessing;

/// <summary>
/// TaskListProcessing
/// </summary>
public class TaskListProcessorGeneric
{

    /// <summary>
    /// Generates telemetry data for a successful task.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="elapsedTimeMS">The time taken to execute the task in milliseconds.</param>
    /// <returns>A string representing the telemetry data.</returns>
    private static string GetTelemetry(string taskName, long elapsedTimeMS)
    {
        return taskName is null
            ? throw new ArgumentNullException(nameof(taskName))
            : $"{taskName}: Task completed in {elapsedTimeMS:N0} ms";
    }

    /// <summary>
    /// Generates telemetry data for a task that completed with an error.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="elapsedTimeMS">The time taken to execute the task in milliseconds.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorDescription">The error description.</param>
    /// <returns>A string representing the telemetry data with error details.</returns>
    private static string GetTelemetry(string taskName, long elapsedTimeMS, string errorCode, string errorDescription)
    {
        return taskName is null
            ? throw new ArgumentNullException(nameof(taskName))
            : $"{taskName}: Task completed in {elapsedTimeMS:N0} ms with ERROR {errorCode}: {errorDescription}";
    }

    /// <summary>
    /// Executes a task, logs the result and the time taken, and adds the result to the task list.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="task">The task to execute and log.</param>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    public async Task GetTaskResultAsync<T>(string taskName, Task<T> task) where T : class
    {
        var sw = new Stopwatch();
        sw.Start();
        var taskResult = new TaskResult<T> { Name = taskName };
        try
        {
            taskResult.Data = await task;
            sw.Stop();
            Telemetry.Add(GetTelemetry(taskName, sw.ElapsedMilliseconds));
        }
        catch (Exception ex)
        {
            sw.Stop();
            Telemetry.Add(GetTelemetry(taskName, sw.ElapsedMilliseconds, "Exception", ex.Message));
            taskResult.Data = null;
        }
        finally
        {
            TaskResults.Add(taskResult);
        }
    }
    /// <summary>
    /// Waits for all the provided tasks to complete and logs an error if any of the tasks fail.
    /// </summary>
    /// <param name="tasks">The tasks to wait on.</param>
    /// <param name="logger">The logger used to log any errors.</param>
    public async Task WhenAllWithLoggingAsync(IEnumerable<Task> tasks, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TLP: An error occurred while executing one or more tasks.");
        }
    }

    /// <summary>
    /// The name of the task list.
    /// </summary>
    public string? TaskListName { get; set; }

    /// <summary>
    /// A collection of task results.
    /// </summary>
    public ICollection<ITaskResult> TaskResults { get; set; } = new HashSet<ITaskResult>();

    /// <summary>
    /// A collection of telemetry data representing the performance of the tasks.
    /// </summary>
    public List<string> Telemetry { get; internal set; } = [];
}
public class TaskResult<T> : ITaskResult
{
    public TaskResult()
    {
        Name = "UNKNOWN";
        Data = default;
    }

    public TaskResult(string name, T data)
    {
        Name = name;
        Data = data;
    }

    public T? Data { get; set; }
    public string Name { get; set; }
}
public interface ITaskResult
{
    string Name { get; set; }
}
