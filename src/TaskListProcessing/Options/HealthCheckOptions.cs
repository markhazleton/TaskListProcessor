using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Options
{

    /// <summary>
    /// Health check configuration options.
    /// </summary>
    public class HealthCheckOptions
    {
        /// <summary>
        /// Gets or sets the minimum success rate threshold (0.0 to 1.0).
        /// </summary>
        public double MinSuccessRate { get; set; } = 0.8;

        /// <summary>
        /// Gets or sets the maximum average execution time threshold.
        /// </summary>
        public TimeSpan MaxAverageExecutionTime { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the time window for health calculations.
        /// </summary>
        public TimeSpan HealthCheckWindow { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets whether to include circuit breaker state in health checks.
        /// </summary>
        public bool IncludeCircuitBreakerState { get; set; } = true;

        /// <summary>
        /// Gets or sets custom health check predicates.
        /// </summary>
        public List<Func<TelemetrySummary, (bool IsHealthy, string Message)>> CustomChecks { get; set; } = new();
    }
}
