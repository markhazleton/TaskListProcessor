using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TaskListProcessing.Utilities;

/// <summary>
/// High-performance concurrent collection optimized for task results.
/// Provides better enumeration performance than ConcurrentBag through caching.
/// </summary>
/// <typeparam name="T">The type of items stored in the collection.</typeparam>
public class ConcurrentResultCollection<T> : IDisposable
{
    private readonly ConcurrentQueue<T> _items = new();
    private readonly ReaderWriterLockSlim _snapshotLock = new(LockRecursionPolicy.NoRecursion);
    private volatile T[]? _cachedSnapshot;
    private volatile int _snapshotInvalid = 1;
    private bool _disposed;

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConcurrentResultCollection<T>));

        _items.Enqueue(item);
        Interlocked.Exchange(ref _snapshotInvalid, 1);
    }

    /// <summary>
    /// Gets a read-only snapshot of the collection.
    /// This method is optimized for performance by caching snapshots.
    /// </summary>
    /// <returns>A read-only collection containing all items.</returns>
    public IReadOnlyCollection<T> GetSnapshot()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConcurrentResultCollection<T>));

        // Fast path: return cached snapshot if it's still valid
        if (Interlocked.CompareExchange(ref _snapshotInvalid, 0, 0) == 0 && _cachedSnapshot != null)
        {
            return _cachedSnapshot;
        }

        // Slow path: create new snapshot
        _snapshotLock.EnterWriteLock();
        try
        {
            // Double-check pattern
            if (Interlocked.CompareExchange(ref _snapshotInvalid, 0, 0) == 1 || _cachedSnapshot == null)
            {
                _cachedSnapshot = _items.ToArray();
                Interlocked.Exchange(ref _snapshotInvalid, 0);
            }
            return _cachedSnapshot;
        }
        finally
        {
            _snapshotLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets the approximate count of items in the collection.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Clears all items from the collection.
    /// </summary>
    public void Clear()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConcurrentResultCollection<T>));

        _snapshotLock.EnterWriteLock();
        try
        {
            while (_items.TryDequeue(out _)) { }
            _cachedSnapshot = Array.Empty<T>();
            Interlocked.Exchange(ref _snapshotInvalid, 0);
        }
        finally
        {
            _snapshotLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Tries to peek at the first item without removing it.
    /// </summary>
    /// <param name="result">The first item if available.</param>
    /// <returns>True if an item was found; otherwise, false.</returns>
    public bool TryPeek(out T? result)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConcurrentResultCollection<T>));

        return _items.TryPeek(out result);
    }

    /// <summary>
    /// Disposes the collection and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _snapshotLock?.Dispose();

            // Clear references to help GC
            _cachedSnapshot = null;

            // Clear the queue
            while (_items.TryDequeue(out _)) { }
        }
    }
}
