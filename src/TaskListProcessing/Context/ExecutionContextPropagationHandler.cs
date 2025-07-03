using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Context
{

    /// <summary>
    /// Context propagation handler for maintaining thread context across task boundaries.
    /// </summary>
    public class ExecutionContextPropagationHandler : IContextPropagationHandler
    {
        /// <summary>
        /// Gets the name of the context handler.
        /// </summary>
        public string Name => "ExecutionContext";

        /// <summary>
        /// Captures the current execution context.
        /// </summary>
        /// <returns>Captured execution context.</returns>
        public object? CaptureContext()
        {
            return ExecutionContext.Capture();
        }

        /// <summary>
        /// Restores the execution context.
        /// </summary>
        /// <param name="context">The context to restore.</param>
        /// <returns>A disposable that restores the original context when disposed.</returns>
        public IDisposable? RestoreContext(object? context)
        {
            if (context is ExecutionContext executionContext)
            {
                var originalContext = ExecutionContext.Capture();
                ExecutionContext.Restore(executionContext);

                return new ContextRestorer(() =>
                {
                    if (originalContext != null)
                        ExecutionContext.Restore(originalContext);
                });
            }

            return null;
        }
    }
}
