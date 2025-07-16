using System.Net.Sockets;
using System.Security;
using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Models;

/// <summary>
/// Categorizes different types of task errors for better observability and retry logic.
/// </summary>
public enum TaskErrorCategory
{
    /// <summary>
    /// Task timed out or was cancelled.
    /// </summary>
    Timeout,

    /// <summary>
    /// Network-related error occurred.
    /// </summary>
    NetworkError,

    /// <summary>
    /// Authentication or authorization error.
    /// </summary>
    AuthenticationError,

    /// <summary>
    /// Input validation error.
    /// </summary>
    ValidationError,

    /// <summary>
    /// System-level error occurred.
    /// </summary>
    SystemError,

    /// <summary>
    /// Business logic error.
    /// </summary>
    BusinessError,

    /// <summary>
    /// Unknown or unclassified error.
    /// </summary>
    Unknown
}

/// <summary>
/// Enhanced task result with comprehensive error information and performance metrics.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class EnhancedTaskResult<T> : ITaskResult, IEquatable<EnhancedTaskResult<T>>
{
    /// <summary>
    /// Initializes a new instance of the EnhancedTaskResult class.
    /// </summary>
    public EnhancedTaskResult()
    {
        Name = "UNKNOWN";
        Data = default;
        IsSuccessful = false;
        Timestamp = DateTimeOffset.UtcNow;
        StartTime = DateTimeOffset.UtcNow;
        Metadata = new Dictionary<string, object>();
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
        Timestamp = DateTimeOffset.UtcNow;
        StartTime = DateTimeOffset.UtcNow;
        Metadata = new Dictionary<string, object>();
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

    /// <summary>
    /// Gets or sets the timestamp when the task started execution.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the execution time of the task.
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the category of error that occurred.
    /// </summary>
    public TaskErrorCategory? ErrorCategory { get; set; }

    /// <summary>
    /// Gets or sets whether the task failure is retryable.
    /// </summary>
    public bool IsRetryable { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts made for this task.
    /// </summary>
    public int AttemptNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the exception that caused the task to fail.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets additional metadata associated with the task.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Classifies an exception into an error category.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns>The error category.</returns>
    public static TaskErrorCategory ClassifyError(Exception exception)
    {
        return exception switch
        {
            TimeoutException or OperationCanceledException => TaskErrorCategory.Timeout,
            HttpRequestException or SocketException => TaskErrorCategory.NetworkError,
            UnauthorizedAccessException or SecurityException => TaskErrorCategory.AuthenticationError,
            ArgumentException or ArgumentNullException or FormatException => TaskErrorCategory.ValidationError,
            InvalidOperationException when exception.Message.Contains("business") => TaskErrorCategory.BusinessError,
            OutOfMemoryException or StackOverflowException => TaskErrorCategory.SystemError,
            _ => TaskErrorCategory.Unknown
        };
    }

    /// <summary>
    /// Determines if an error category is retryable.
    /// </summary>
    /// <param name="category">The error category to check.</param>
    /// <returns>True if the error is retryable; otherwise, false.</returns>
    public static bool IsRetryableError(TaskErrorCategory category)
    {
        return category switch
        {
            TaskErrorCategory.NetworkError => true,
            TaskErrorCategory.Timeout => true,
            TaskErrorCategory.SystemError => true,
            TaskErrorCategory.Unknown => true,
            _ => false
        };
    }

    /// <summary>
    /// Sets error information for a failed task.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public void SetError(Exception exception)
    {
        IsSuccessful = false;
        Exception = exception;
        ErrorMessage = exception.Message;
        ErrorCategory = ClassifyError(exception);
        IsRetryable = IsRetryableError(ErrorCategory.Value);
    }

    /// <summary>
    /// Returns a string representation of the task result.
    /// </summary>
    public override string ToString()
    {
        var status = IsSuccessful ? "Success" : "Failed";
        var error = !string.IsNullOrEmpty(ErrorMessage) ? $" - {ErrorMessage}" : string.Empty;
        var timing = ExecutionTime.TotalMilliseconds > 0 ? $" ({ExecutionTime.TotalMilliseconds:F0}ms)" : string.Empty;
        var attempt = AttemptNumber > 1 ? $" [Attempt {AttemptNumber}]" : string.Empty;
        return $"{Name}: {status}{error}{timing}{attempt}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public bool Equals(EnhancedTaskResult<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name &&
               Timestamp.Equals(other.Timestamp) &&
               IsSuccessful == other.IsSuccessful;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as EnhancedTaskResult<T>);
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Timestamp, IsSuccessful);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(EnhancedTaskResult<T>? left, EnhancedTaskResult<T>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(EnhancedTaskResult<T>? left, EnhancedTaskResult<T>? right)
    {
        return !Equals(left, right);
    }
}