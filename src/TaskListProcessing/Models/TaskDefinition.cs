using TaskListProcessing.Scheduling;

namespace TaskListProcessing.Models
{

    /// <summary>
    /// Task definition with dependency and priority information.
    /// </summary>
    public class TaskDefinition
    {
        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the task factory function.
        /// </summary>
        public Func<CancellationToken, Task<object?>> Factory { get; set; } = _ => Task.FromResult<object?>(null);

        /// <summary>
        /// Gets or sets the task dependencies (must complete before this task).
        /// </summary>
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the task priority (higher values = higher priority).
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the estimated execution time for scheduling optimization.
        /// </summary>
        public TimeSpan? EstimatedExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets custom metadata for the task.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets task-specific timeout override.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Gets or sets task-specific retry policy override.
        /// </summary>
        public RetryPolicy? RetryPolicy { get; set; }
    }
}
