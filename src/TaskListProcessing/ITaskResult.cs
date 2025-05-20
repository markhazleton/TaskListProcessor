namespace TaskListProcessing
{
    /// <summary>
    /// Defines a contract for task result objects, requiring a Name property.
    /// </summary>
    public interface ITaskResult
    {
        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        string Name { get; set; }
    }
}
