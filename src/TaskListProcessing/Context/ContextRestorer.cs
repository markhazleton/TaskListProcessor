namespace TaskListProcessing.Context
{

    /// <summary>
    /// Helper class for restoring context.
    /// </summary>
    internal class ContextRestorer : IDisposable
    {
        private readonly Action _restoreAction;
        private bool _disposed;

        public ContextRestorer(Action restoreAction)
        {
            _restoreAction = restoreAction ?? throw new ArgumentNullException(nameof(restoreAction));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _restoreAction();
                _disposed = true;
            }
        }
    }
}
