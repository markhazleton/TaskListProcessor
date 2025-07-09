namespace TaskListProcessing.Telemetry;

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
        var errorInfo = !string.IsNullOrEmpty(ErrorMessage) ? $": {ErrorMessage}" : string.Empty;
        return $"{TaskName}: Task {status} in {ElapsedMilliseconds:N0} ms{errorInfo}";
    }
}
