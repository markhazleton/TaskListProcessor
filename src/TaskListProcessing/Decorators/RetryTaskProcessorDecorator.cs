using System;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Retry decorator for ITaskProcessor that adds retry logic around task execution.
/// </summary>
public class RetryTaskProcessorDecorator : ITaskProcessor
{
    private readonly ITaskProcessor _inner;
    // TODO: Add retry policy dependency

    public RetryTaskProcessorDecorator(ITaskProcessor inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public string Name => _inner.Name;

    public event EventHandler<ITaskResult>? TaskCompleted
    {
        add => _inner.TaskCompleted += value;
        remove => _inner.TaskCompleted -= value;
    }

    public async Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Task<T> task,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement retry logic
        return await _inner.ExecuteTaskAsync(taskName, task, cancellationToken);
    }

    public async Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Func<CancellationToken, Task<T>> taskFactory,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement retry logic
        return await _inner.ExecuteTaskAsync(taskName, taskFactory, cancellationToken);
    }
}
