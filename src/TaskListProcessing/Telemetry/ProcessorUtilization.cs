namespace TaskListProcessing.Telemetry
{

    /// <summary>
    /// Processor utilization statistics.
    /// </summary>
    public record ProcessorUtilization
    {
        /// <summary>
        /// Gets or sets the processor index.
        /// </summary>
        public int ProcessorIndex { get; init; }

        /// <summary>
        /// Gets or sets the processor name.
        /// </summary>
        public string ProcessorName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of tasks processed.
        /// </summary>
        public int TasksProcessed { get; init; }

        /// <summary>
        /// Gets or sets the number of successful tasks.
        /// </summary>
        public int SuccessfulTasks { get; init; }

        /// <summary>
        /// Gets or sets the number of failed tasks.
        /// </summary>
        public int FailedTasks { get; init; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds.
        /// </summary>
        public double AverageExecutionTime { get; init; }

        /// <summary>
        /// Gets or sets the total execution time in milliseconds.
        /// </summary>
        public long TotalExecutionTime { get; init; }

        /// <summary>
        /// Gets the success rate percentage.
        /// </summary>
        public double SuccessRate =>
            TasksProcessed > 0 ? (double)SuccessfulTasks / TasksProcessed * 100 : 0;

        /// <summary>
        /// Gets the utilization score (higher means more utilized).
        /// </summary>
        public double UtilizationScore => TasksProcessed * AverageExecutionTime;
    }
}
