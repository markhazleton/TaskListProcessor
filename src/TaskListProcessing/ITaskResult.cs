namespace TaskListProcessing
{
    /// <summary>
    /// Defines a contract for task result objects, requiring a Name property and success tracking.
    /// </summary>
    public interface ITaskResult
    {
        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the task completed successfully.
        /// </summary>
        bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message if the task failed.
        /// </summary>
        string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the task was created.
        /// </summary>
        DateTimeOffset Timestamp { get; set; }
    }
}
