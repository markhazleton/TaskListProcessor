using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Models;

/// <summary>
/// Self-managing pooled task result that automatically returns to pool on disposal.
/// THREAD SAFETY: This class is thread-safe for disposal operations.
/// </summary>
/// <typeparam name="T">The type of the task result data.</typeparam>
public sealed class PooledTaskResult<T> : EnhancedTaskResult<T>, IPooledTaskResult<T>
{
    private readonly ObjectPool<PooledTaskResult<T>>? _pool;
    private volatile bool _isReturned;
    private readonly object _disposeLock = new();

    /// <summary>
    /// Initializes a new instance of the PooledTaskResult class.
    /// </summary>
    /// <param name="pool">The object pool to return to on disposal.</param>
    internal PooledTaskResult(ObjectPool<PooledTaskResult<T>>? pool) : base()
    {
        _pool = pool;
        _isReturned = false;
    }

    /// <summary>
    /// Gets whether this result has been returned to the pool.
    /// </summary>
    public bool IsReturned => _isReturned;

    /// <summary>
    /// Forces return to pool (for cleanup scenarios).
    /// </summary>
    public void ForceReturn()
    {
        Dispose();
    }

    /// <summary>
    /// Disposes the pooled task result and returns it to the pool.
    /// </summary>
    public void Dispose()
    {
        lock (_disposeLock)
        {
            if (!_isReturned && _pool != null)
            {
                // Reset all properties to default state
                ResetToDefault();
                _pool.Return(this);
                _isReturned = true;
            }
        }
    }

    /// <summary>
    /// Resets the task result to its default state for reuse.
    /// </summary>
    private void ResetToDefault()
    {
        Name = "UNKNOWN";
        Data = default;
        IsSuccessful = false;
        ErrorMessage = null;
        ErrorCategory = null;
        IsRetryable = false;
        AttemptNumber = 1;
        Exception = null;
        Metadata?.Clear();
        Metadata ??= new Dictionary<string, object>();
        Timestamp = DateTimeOffset.UtcNow;
        StartTime = DateTimeOffset.UtcNow;
        ExecutionTime = TimeSpan.Zero;
    }

    /// <summary>
    /// Finalizer to detect improper disposal.
    /// </summary>
    ~PooledTaskResult()
    {
        if (!_isReturned)
        {
            // Log warning about improper disposal
            System.Diagnostics.Debug.WriteLine($"PooledTaskResult for '{Name}' was not properly disposed");
        }
    }
}
