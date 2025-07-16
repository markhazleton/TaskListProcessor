using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Decorators;

/// <summary>
/// Metrics decorator for ITaskStreamProcessor that adds metrics collection around stream task execution.
/// </summary>
public class MetricsTaskStreamProcessorDecorator : ITaskStreamProcessor
{
    private readonly ITaskStreamProcessor _inner;

    public MetricsTaskStreamProcessorDecorator(ITaskStreamProcessor inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public async IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTasksStreamAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // TODO: Start stream metrics collection
        await foreach (var result in _inner.ProcessTasksStreamAsync(taskFactories, cancellationToken))
        {
            // TODO: Record per-task metrics
            yield return result;
        }
        // TODO: Record overall stream metrics
    }

    public async IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTaskDefinitionsStreamAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // TODO: Start stream metrics collection
        await foreach (var result in _inner.ProcessTaskDefinitionsStreamAsync(taskDefinitions, cancellationToken))
        {
            // TODO: Record per-task metrics
            yield return result;
        }
        // TODO: Record overall stream metrics
    }
}
