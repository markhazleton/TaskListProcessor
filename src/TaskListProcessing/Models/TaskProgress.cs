namespace TaskListProcessing.Models
{

    /// <summary>
    /// Progress reporting information for task processing.
    /// </summary>
    public record TaskProgress(
        int CompletedTasks,
        int TotalTasks,
        string? CurrentTaskName = null,
        TimeSpan ElapsedTime = default,
        TimeSpan? EstimatedTimeRemaining = null,
        double SuccessRate = 0.0)
    {
        /// <summary>
        /// Gets the completion percentage (0.0 to 1.0).
        /// </summary>
        public double CompletionPercentage => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks : 0.0;

        /// <summary>
        /// Gets whether all tasks are completed.
        /// </summary>
        public bool IsCompleted => CompletedTasks >= TotalTasks;

        /// <summary>
        /// Gets the number of remaining tasks.
        /// </summary>
        public int RemainingTasks => Math.Max(0, TotalTasks - CompletedTasks);
    }
}