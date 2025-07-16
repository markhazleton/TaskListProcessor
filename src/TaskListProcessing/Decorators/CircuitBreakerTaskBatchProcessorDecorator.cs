using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Circuit breaker decorator for ITaskBatchProcessor that adds circuit breaker pattern around batch task execution.
/// </summary>
public class CircuitBreakerTaskBatchProcessorDecorator : ITaskBatchProcessor
{
    private readonly ITaskBatchProcessor _inner;

    public CircuitBreakerTaskBatchProcessorDecorator(ITaskBatchProcessor inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyCollection<ITaskResult> TaskResults => _inner.TaskResults;
    public TaskProgress CurrentProgress => _inner.CurrentProgress;

    public event EventHandler<TaskProgress>? ProgressChanged
    {
        add => _inner.ProgressChanged += value;
        remove => _inner.ProgressChanged -= value;
    }

    public async Task ProcessTasksAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Apply circuit breaker pattern to batch execution
        await _inner.ProcessTasksAsync(taskFactories, progress, cancellationToken);
    }

    public async Task ProcessTaskDefinitionsAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Apply circuit breaker pattern to batch execution
        await _inner.ProcessTaskDefinitionsAsync(taskDefinitions, progress, cancellationToken);
    }
}
