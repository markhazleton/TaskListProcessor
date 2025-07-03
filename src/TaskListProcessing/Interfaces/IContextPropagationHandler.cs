namespace TaskListProcessing.Interfaces
{

    /// <summary>
    /// Interface for context propagation handlers.
    /// </summary>
    public interface IContextPropagationHandler
    {
        /// <summary>
        /// Captures context before task execution.
        /// </summary>
        /// <returns>Captured context data.</returns>
        object? CaptureContext();

        /// <summary>
        /// Restores context during task execution.
        /// </summary>
        /// <param name="context">The context to restore.</param>
        /// <returns>A disposable that restores the original context when disposed.</returns>
        IDisposable? RestoreContext(object? context);

        /// <summary>
        /// Gets the name of the context handler.
        /// </summary>
        string Name { get; }
    }
}
