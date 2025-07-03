namespace TaskListProcessing
{

    /// <summary>
    /// Statistics about the current state of a circuit breaker.
    /// </summary>
    public record CircuitBreakerStats
    {
        /// <summary>
        /// Current state of the circuit breaker.
        /// </summary>
        public CircuitBreakerState State { get; init; }

        /// <summary>
        /// Current failure count within the time window.
        /// </summary>
        public int FailureCount { get; init; }

        /// <summary>
        /// Failure threshold that triggers the circuit to open.
        /// </summary>
        public int FailureThreshold { get; init; }

        /// <summary>
        /// Time window for counting failures.
        /// </summary>
        public TimeSpan TimeWindow { get; init; }

        /// <summary>
        /// Duration the circuit stays open.
        /// </summary>
        public TimeSpan OpenDuration { get; init; }

        /// <summary>
        /// When the circuit was opened (if currently open).
        /// </summary>
        public DateTimeOffset? OpenedAt { get; init; }

        /// <summary>
        /// Time remaining until the circuit can attempt to close (if currently open).
        /// </summary>
        public TimeSpan? TimeUntilRetry { get; init; }
    }
}
