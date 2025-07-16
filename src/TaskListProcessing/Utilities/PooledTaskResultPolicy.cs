using Microsoft.Extensions.ObjectPool;
using System;
using TaskListProcessing.Models;

namespace TaskListProcessing.Utilities;

/// <summary>
/// Custom pool policy for PooledTaskResult with proper lifecycle management.
/// Provides validation and creation logic for pooled task results.
/// </summary>
/// <typeparam name="T">The type of the task result data.</typeparam>
public class PooledTaskResultPolicy<T> : IPooledObjectPolicy<PooledTaskResult<T>>
{
    private readonly ObjectPool<PooledTaskResult<T>> _pool;

    /// <summary>
    /// Initializes a new instance of the PooledTaskResultPolicy.
    /// </summary>
    /// <param name="pool">The object pool this policy manages.</param>
    public PooledTaskResultPolicy(ObjectPool<PooledTaskResult<T>> pool)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>
    /// Creates a new pooled task result instance.
    /// </summary>
    /// <returns>A new PooledTaskResult instance.</returns>
    public PooledTaskResult<T> Create()
    {
        return new PooledTaskResult<T>(_pool);
    }

    /// <summary>
    /// Validates whether an object can be returned to the pool.
    /// </summary>
    /// <param name="obj">The object to validate for pool return.</param>
    /// <returns>True if the object can be returned to the pool; otherwise, false.</returns>
    public bool Return(PooledTaskResult<T> obj)
    {
        if (obj == null) return false;
        
        // Validate object state before returning to pool
        if (obj.IsReturned)
        {
            return false; // Already returned
        }

        // Don't pool objects with critical exceptions that might indicate corruption
        if (obj.Exception is OutOfMemoryException or StackOverflowException)
        {
            return false;
        }

        // Additional validation for data integrity
        if (obj.Name == null)
        {
            return false; // Corrupted state
        }

        return true;
    }
}
