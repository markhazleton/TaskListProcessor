namespace TaskListProcessing
{

    /// <summary>
    /// Enhanced circuit breaker configuration options.
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// Gets or sets the failure threshold before opening the circuit.
        /// </summary>
        public int FailureThreshold { get; set; } = 5;

        /// <summary>
        /// Gets or sets the time window for counting failures.
        /// </summary>
        public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the duration to keep the circuit open.
        /// </summary>
        public TimeSpan OpenDuration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the number of attempts allowed in half-open state.
        /// </summary>
        public int HalfOpenAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the number of successful attempts needed to close the circuit from half-open state.
        /// </summary>
        public int SuccessThreshold { get; set; } = 2;

        /// <summary>
        /// Validates the circuit breaker options.
        /// </summary>
        /// <returns>Validation result.</returns>
        public (bool IsValid, string[] Errors) Validate()
        {
            var errors = new List<string>();

            if (FailureThreshold <= 0)
                errors.Add("FailureThreshold must be greater than 0");

            if (TimeWindow <= TimeSpan.Zero)
                errors.Add("TimeWindow must be positive");

            if (OpenDuration <= TimeSpan.Zero)
                errors.Add("OpenDuration must be positive");

            if (HalfOpenAttempts <= 0)
                errors.Add("HalfOpenAttempts must be greater than 0");

            if (SuccessThreshold <= 0)
                errors.Add("SuccessThreshold must be greater than 0");

            if (SuccessThreshold > HalfOpenAttempts)
                errors.Add("SuccessThreshold cannot be greater than HalfOpenAttempts");

            return (errors.Count == 0, errors.ToArray());
        }
    }
}