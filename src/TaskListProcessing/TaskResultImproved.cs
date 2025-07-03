namespace TaskListProcessing;

/// <summary>
/// Enhanced task result with success tracking and error information.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class EnhancedTaskResult<T> : ITaskResult
{
    /// <summary>
    /// Initializes a new instance of the EnhancedTaskResult class.
    /// </summary>
    public EnhancedTaskResult()
    {
        Name = "UNKNOWN";
        Data = default;
        IsSuccessful = false;
    }

    /// <summary>
    /// Initializes a new instance of the EnhancedTaskResult class with specified values.
    /// </summary>
    /// <param name="name">The name of the task.</param>
    /// <param name="data">The result data.</param>
    /// <param name="isSuccessful">Whether the task was successful.</param>
    public EnhancedTaskResult(string name, T? data, bool isSuccessful = true)
    {
        Name = name;
        Data = data;
        IsSuccessful = isSuccessful;
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
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Returns a string representation of the task result.
    /// </summary>
    public override string ToString()
    {
        var status = IsSuccessful ? "Success" : "Failed";
        var error = !string.IsNullOrEmpty(ErrorMessage) ? $" - {ErrorMessage}" : "";
        return $"{Name}: {status}{error}";
    }
}
