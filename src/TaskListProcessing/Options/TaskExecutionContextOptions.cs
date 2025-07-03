using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Options
{

    /// <summary>
    /// Task execution context options.
    /// </summary>
    public class TaskExecutionContextOptions
    {
        /// <summary>
        /// Gets or sets whether to capture execution context.
        /// </summary>
        public bool CaptureExecutionContext { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to capture synchronization context.
        /// </summary>
        public bool CaptureSynchronizationContext { get; set; } = false;

        /// <summary>
        /// Gets or sets custom context propagation handlers.
        /// </summary>
        public List<IContextPropagationHandler> PropagationHandlers { get; set; } = new();

        /// <summary>
        /// Gets or sets the task scheduler to use for task execution.
        /// </summary>
        public TaskScheduler? TaskScheduler { get; set; }
    }
}
