using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Models;

/// <summary>
/// Represents the result of a task, including its name and result data.
/// </summary>
public class TaskResult<T> : ITaskResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskResult{T}"/> class with default values.
    /// </summary>
    public TaskResult()
    {
        Name = "UNKNOWN";
        Data = default;
        IsSuccessful = false;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskResult{T}"/> class with the specified name and data.
    /// </summary>
    /// <param name="name">The name of the task.</param>
    /// <param name="data">The result data of the task.</param>
    public TaskResult(string name, T data)
    {
        Name = name;
        Data = data;
        IsSuccessful = true;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets or sets the result data of the task.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the name of the task.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the task completed successfully.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Gets or sets the error message if the task failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the task was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
