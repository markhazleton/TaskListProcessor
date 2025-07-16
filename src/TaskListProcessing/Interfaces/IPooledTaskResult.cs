using System;

namespace TaskListProcessing.Interfaces;

/// <summary>
/// Interface for pooled task results that manage their own lifecycle.
/// Provides automatic resource management and pool return capabilities.
/// </summary>
/// <typeparam name="T">The type of the task result data.</typeparam>
public interface IPooledTaskResult<T> : ITaskResult, IDisposable
{
    /// <summary>
    /// Gets whether this result has been returned to the pool.
    /// Once returned, the object should not be used.
    /// </summary>
    bool IsReturned { get; }
    
    /// <summary>
    /// Forces return to pool (for cleanup scenarios).
    /// This is equivalent to calling Dispose() but provides semantic clarity.
    /// </summary>
    void ForceReturn();
    
    /// <summary>
    /// Gets the result data of the task.
    /// </summary>
    T? Data { get; set; }
}
