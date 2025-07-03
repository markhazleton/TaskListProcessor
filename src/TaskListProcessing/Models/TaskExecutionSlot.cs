namespace TaskListProcessing.Models
{

    /// <summary>
    /// Represents an active task execution slot.
    /// </summary>
    public class TaskExecutionSlot
    {
        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the task started execution.
        /// </summary>
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Gets or sets the estimated completion time.
        /// </summary>
        public DateTimeOffset EstimatedCompletion { get; set; }

        /// <summary>
        /// Gets the elapsed execution time.
        /// </summary>
        public TimeSpan ElapsedTime => DateTimeOffset.UtcNow - StartTime;

        /// <summary>
        /// Gets the estimated remaining time.
        /// </summary>
        public TimeSpan EstimatedRemainingTime => EstimatedCompletion - DateTimeOffset.UtcNow;
    }
}
