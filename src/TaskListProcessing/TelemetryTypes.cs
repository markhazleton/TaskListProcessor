namespace TaskListProcessing;

/// <summary>
/// Represents telemetry data for a task execution.
/// </summary>
public record TaskTelemetry(
    string TaskName,
    long ElapsedMilliseconds,
    string? ErrorType = null,
    string? ErrorMessage = null,
    bool IsSuccessful = true)
{
    /// <summary>
    /// Gets the timestamp when the telemetry was recorded.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Returns a formatted string representation of the telemetry.
    /// </summary>
    public override string ToString()
    {
        var status = IsSuccessful ? "completed" : $"failed with {ErrorType}";
        var errorInfo = !string.IsNullOrEmpty(ErrorMessage) ? $": {ErrorMessage}" : "";
        return $"{TaskName}: Task {status} in {ElapsedMilliseconds:N0} ms{errorInfo}";
    }
}

/// <summary>
/// Provides summary statistics for telemetry data.
/// </summary>
public class TelemetrySummary
{
    /// <summary>
    /// Gets or sets the total number of tasks.
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of successful tasks.
    /// </summary>
    public int SuccessfulTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of failed tasks.
    /// </summary>
    public int FailedTasks { get; set; }

    /// <summary>
    /// Gets or sets the average execution time in milliseconds.
    /// </summary>
    public double AverageExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the total execution time in milliseconds.
    /// </summary>
    public long TotalExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the minimum execution time in milliseconds.
    /// </summary>
    public long MinExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the maximum execution time in milliseconds.
    /// </summary>
    public long MaxExecutionTime { get; set; }

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => TotalTasks > 0 ? (double)SuccessfulTasks / TotalTasks * 100 : 0;

    /// <summary>
    /// Returns a formatted string representation of the summary.
    /// </summary>
    public override string ToString()
    {
        return $"Tasks: {TotalTasks}, Success: {SuccessfulTasks} ({SuccessRate:F1}%), " +
               $"Failed: {FailedTasks}, Avg Time: {AverageExecutionTime:F0}ms, " +
               $"Total Time: {TotalExecutionTime:N0}ms";
    }
}
