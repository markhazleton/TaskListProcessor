using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Context
{

    /// <summary>
    /// Context propagation handler for maintaining synchronization context.
    /// </summary>
    public class SynchronizationContextPropagationHandler : IContextPropagationHandler
    {
        /// <summary>
        /// Gets the name of the context handler.
        /// </summary>
        public string Name => "SynchronizationContext";

        /// <summary>
        /// Captures the current synchronization context.
        /// </summary>
        /// <returns>Captured synchronization context.</returns>
        public object? CaptureContext()
        {
            return SynchronizationContext.Current;
        }

        /// <summary>
        /// Restores the synchronization context.
        /// </summary>
        /// <param name="context">The context to restore.</param>
        /// <returns>A disposable that restores the original context when disposed.</returns>
        public IDisposable? RestoreContext(object? context)
        {
            var originalContext = SynchronizationContext.Current;

            if (context is SynchronizationContext syncContext)
            {
                SynchronizationContext.SetSynchronizationContext(syncContext);
            }

            return new ContextRestorer(() =>
                SynchronizationContext.SetSynchronizationContext(originalContext));
        }
    }
}
