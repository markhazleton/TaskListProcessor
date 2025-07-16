namespace TaskListProcessing.Models;

/// <summary>
/// Represents the result of a health check operation.
/// </summary>
/// <param name="IsHealthy">Whether the health check passed.</param>
/// <param name="Message">Descriptive message about the health check result.</param>
public record HealthCheckResult(bool IsHealthy, string Message);
