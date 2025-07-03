namespace TaskListProcessing.Models
{

    /// <summary>
    /// Represents a scheduled task with execution metadata.
    /// </summary>
    public class ScheduledTask
    {
        /// <summary>
        /// Gets or sets the task definition.
        /// </summary>
        public TaskDefinition Definition { get; set; } = new();

        /// <summary>
        /// Gets or sets the task priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets when the task was scheduled.
        /// </summary>
        public DateTimeOffset ScheduledAt { get; set; }

        /// <summary>
        /// Gets or sets when the task started execution.
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Gets or sets when the task completed.
        /// </summary>
        public DateTimeOffset? CompletionTime { get; set; }

        /// <summary>
        /// Gets or sets the estimated task duration.
        /// </summary>
        public TimeSpan EstimatedDuration { get; set; }

        /// <summary>
        /// Gets or sets the current task status.
        /// </summary>
        public TaskStatus Status { get; set; } = TaskStatus.Created;

        /// <summary>
        /// Gets or sets the task result.
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// Gets or sets the exception if the task faulted.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets the actual execution duration.
        /// </summary>
        public TimeSpan? ActualDuration =>
            StartTime.HasValue && CompletionTime.HasValue
                ? CompletionTime.Value - StartTime.Value
                : null;

        /// <summary>
        /// Gets the time spent waiting in queue.
        /// </summary>
        public TimeSpan? QueueTime =>
            StartTime.HasValue
                ? StartTime.Value - ScheduledAt
                : null;
    }
}
