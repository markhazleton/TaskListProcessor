using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Metrics decorator for ITaskBatchProcessor that adds metrics collection around batch task execution.
/// </summary>
public class MetricsTaskBatchProcessorDecorator : ITaskBatchProcessor
{
    private readonly ITaskBatchProcessor _inner;

    public MetricsTaskBatchProcessorDecorator(ITaskBatchProcessor inner)
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
        // TODO: Start batch metrics collection
        await _inner.ProcessTasksAsync(taskFactories, progress, cancellationToken);
        // TODO: Record batch metrics
    }

    public async Task ProcessTaskDefinitionsAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Start batch metrics collection
        await _inner.ProcessTaskDefinitionsAsync(taskDefinitions, progress, cancellationToken);
        // TODO: Record batch metrics
    }
}
