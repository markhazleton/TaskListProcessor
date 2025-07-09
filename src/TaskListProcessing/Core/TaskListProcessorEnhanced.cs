using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Options;
using TaskListProcessing.Scheduling;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Core;

/// <summary>
/// Enhanced TaskListProcessor with comprehensive enterprise features.
/// </summary>
public class TaskListProcessorEnhanced : IDisposable, IAsyncDisposable
{
    private readonly ILogger? _logger;
    private readonly TaskListProcessorOptions _options;
    private readonly ConcurrentBag<ITaskResult> _taskResults = new();
    private readonly ConcurrentBag<TaskTelemetry> _telemetry = new();
    private readonly ConcurrentQueue<TaskDefinition> _taskQueue = new();
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly CircuitBreaker? _circuitBreaker;
    private readonly RetryHandler? _retryHandler;
    private readonly ObjectPool<EnhancedTaskResult<object>>? _resultPool;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _disposed;
    private bool _initialized;
    private TaskProgress _currentProgress = new(0, 0);
    private readonly object _progressLock = new();

    /// <summary>
    /// Initializes a new instance of the TaskListProcessorEnhanced class.
    /// </summary>
    /// <param name="taskListName">Optional name for the task list.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    /// <param name="options">Configuration options.</param>
    public TaskListProcessorEnhanced(string? taskListName = null, ILogger? logger = null, TaskListProcessorOptions? options = null)
    {
        TaskListName = taskListName ?? "TaskProcessor";
        _logger = logger;
        _options = options ?? new TaskListProcessorOptions();

        _concurrencyLimiter = new SemaphoreSlim(_options.MaxConcurrentTasks, _options.MaxConcurrentTasks);

        if (_options.CircuitBreakerOptions != null)
        {
            _circuitBreaker = new CircuitBreaker(_options.CircuitBreakerOptions);
        }

        if (_options.RetryPolicy != null)
        {
            _retryHandler = new RetryHandler(_options.RetryPolicy);
        }

        if (_options.EnableMemoryPooling)
        {
            _resultPool = CreateResultPool();
        }
    }

    /// <summary>
    /// Gets the name of the task list.
    /// </summary>
    public string TaskListName { get; }

    /// <summary>
    /// Gets the collection of task results (thread-safe).
    /// </summary>
    public IReadOnlyCollection<ITaskResult> TaskResults => _taskResults.ToList().AsReadOnly();

    /// <summary>
    /// Gets the collection of telemetry data (thread-safe).
    /// </summary>
    public IReadOnlyCollection<TaskTelemetry> Telemetry => _telemetry.ToList().AsReadOnly();

    /// <summary>
    /// Gets the current circuit breaker state.
    /// </summary>
    public CircuitBreakerStats? CircuitBreakerStats => _circuitBreaker?.GetStats();

    /// <summary>
    /// Gets the current progress information.
    /// </summary>
    public TaskProgress CurrentProgress
    {
        get
        {
            lock (_progressLock)
            {
                return _currentProgress;
            }
        }
    }

    /// <summary>
    /// Event raised when progress is updated.
    /// </summary>
    public event EventHandler<TaskProgress>? ProgressChanged;

    /// <summary>
    /// Event raised when a task completes (successfully or with error).
    /// </summary>
    public event EventHandler<ITaskResult>? TaskCompleted;

    /// <summary>
    /// Initializes the processor asynchronously.
    /// </summary>
    /// <returns>A task representing the initialization operation.</returns>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _logger?.LogInformation("Initializing TaskListProcessor '{ProcessorName}' with {MaxConcurrency} max concurrency",
            TaskListName, _options.MaxConcurrentTasks);

        // Pre-warm object pool if enabled
        if (_resultPool != null && _options.MemoryPoolOptions?.PrewarmPool == true)
        {
            await PrewarmObjectPoolAsync();
        }

        // Initialize telemetry exporter if configured
        if (_options.TelemetryExporter != null && _options.TelemetryExporter.IsEnabled)
        {
            _logger?.LogInformation("Telemetry exporter '{ExporterName}' enabled", _options.TelemetryExporter.Name);
        }

        _initialized = true;
        _logger?.LogInformation("TaskListProcessor '{ProcessorName}' initialized successfully", TaskListName);
    }

    /// <summary>
    /// Executes multiple tasks concurrently with comprehensive error handling and telemetry.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="progress">Optional progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public async Task ProcessTasksAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        var startTime = DateTimeOffset.UtcNow;

        // Convert to task definitions
        var taskDefinitions = taskFactories.Select(kvp => new TaskDefinition
        {
            Name = kvp.Key,
            Factory = kvp.Value
        }).ToList();

        await ProcessTaskDefinitionsAsync(taskDefinitions, progress, combinedCts.Token);
    }

    /// <summary>
    /// Executes tasks with dependency resolution and scheduling.
    /// </summary>
    /// <param name="taskDefinitions">Task definitions with dependencies and priorities.</param>
    /// <param name="progress">Optional progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public async Task ProcessTaskDefinitionsAsync(
        IEnumerable<TaskDefinition> taskDefinitions,
        IProgress<TaskProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = taskDefinitions.ToList();
        var startTime = DateTimeOffset.UtcNow;

        // Initialize progress
        UpdateProgress(0, tasks.Count, null, TimeSpan.Zero, progress);

        // Resolve dependencies if resolver is configured
        if (_options.DependencyResolver != null)
        {
            if (!_options.DependencyResolver.ValidateDependencies(tasks))
            {
                throw new InvalidOperationException("Circular dependency detected in task definitions");
            }

            tasks = _options.DependencyResolver.ResolveDependencies(tasks).ToList();
        }
        else
        {
            // Apply scheduling strategy
            tasks = ApplySchedulingStrategy(tasks);
        }

        var processingTasks = new List<Task>();
        var completedTasks = 0;

        try
        {
            foreach (var taskDef in tasks)
            {
                var processingTask = ProcessSingleTaskWithManagementAsync(
                    taskDef,
                    () =>
                    {
                        var completed = Interlocked.Increment(ref completedTasks);
                        var elapsed = DateTimeOffset.UtcNow - startTime;
                        var estimated = EstimateRemainingTime(completed, tasks.Count, elapsed);
                        var successRate = CalculateSuccessRate();

                        UpdateProgress(completed, tasks.Count, taskDef.Name, elapsed, progress, estimated, successRate);
                    },
                    cancellationToken);

                processingTasks.Add(processingTask);

                // Respect max concurrency
                if (processingTasks.Count >= _options.MaxConcurrentTasks)
                {
                    var completedTask = await Task.WhenAny(processingTasks);
                    processingTasks.Remove(completedTask);
                }
            }

            // Wait for all remaining tasks
            await Task.WhenAll(processingTasks);
        }
        finally
        {
            // Export telemetry if configured
            if (_options.TelemetryExporter != null && _options.EnableDetailedTelemetry)
            {
                await ExportTelemetryAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Executes a single task with comprehensive error handling and telemetry.
    /// </summary>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    public async Task<EnhancedTaskResult<T>> ExecuteTaskAsync<T>(
        string taskName,
        Task<T> task,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);
        ArgumentNullException.ThrowIfNull(task);

        var taskDef = new TaskDefinition
        {
            Name = taskName,
            Factory = async ct =>
            {
                var result = await task.WaitAsync(ct);
                return (object?)result;
            }
        };

        var result = await ProcessSingleTaskAsync(taskDef, cancellationToken);

        // Convert to strongly typed result
        return new EnhancedTaskResult<T>(taskName, (T?)result.Data, result.IsSuccessful)
        {
            ErrorMessage = result.ErrorMessage,
            ErrorCategory = result.ErrorCategory,
            ExecutionTime = result.ExecutionTime,
            IsRetryable = result.IsRetryable,
            AttemptNumber = result.AttemptNumber,
            Exception = result.Exception,
            Metadata = result.Metadata
        };
    }

    /// <summary>
    /// Streams task results as they complete using async enumerable.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>An async enumerable of task results.</returns>
    public async IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTasksStreamAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        var taskDefinitions = taskFactories.Select(kvp => new TaskDefinition
        {
            Name = kvp.Key,
            Factory = kvp.Value
        });

        var tasks = taskDefinitions.Select(taskDef =>
            ProcessSingleTaskAsync(taskDef, cancellationToken)).ToList();

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            var result = await completedTask;
            yield return result;
        }
    }

    /// <summary>
    /// Gets comprehensive telemetry summary statistics.
    /// </summary>
    /// <returns>Summary of telemetry data.</returns>
    public TelemetrySummary GetTelemetrySummary()
    {
        var telemetryList = _telemetry.ToList();

        if (!telemetryList.Any())
        {
            return new TelemetrySummary();
        }

        return new TelemetrySummary
        {
            TotalTasks = telemetryList.Count,
            SuccessfulTasks = telemetryList.Count(t => t.IsSuccessful),
            FailedTasks = telemetryList.Count(t => !t.IsSuccessful),
            AverageExecutionTime = telemetryList.Average(t => t.ElapsedMilliseconds),
            TotalExecutionTime = telemetryList.Sum(t => t.ElapsedMilliseconds),
            MinExecutionTime = telemetryList.Min(t => t.ElapsedMilliseconds),
            MaxExecutionTime = telemetryList.Max(t => t.ElapsedMilliseconds)
        };
    }

    /// <summary>
    /// Performs a health check on the processor.
    /// </summary>
    /// <returns>Health check result.</returns>
    public HealthCheckResult PerformHealthCheck()
    {
        if (_options.HealthCheckOptions == null)
        {
            return new HealthCheckResult(true, "Health checking disabled");
        }

        var summary = GetTelemetrySummary();
        var healthOptions = _options.HealthCheckOptions;

        // Check success rate
        if (summary.SuccessRate < healthOptions.MinSuccessRate * 100)
        {
            return new HealthCheckResult(false, $"Success rate {summary.SuccessRate:F1}% below threshold {healthOptions.MinSuccessRate * 100:F1}%");
        }

        // Check average execution time
        if (TimeSpan.FromMilliseconds(summary.AverageExecutionTime) > healthOptions.MaxAverageExecutionTime)
        {
            return new HealthCheckResult(false, $"Average execution time {summary.AverageExecutionTime:F0}ms exceeds threshold {healthOptions.MaxAverageExecutionTime.TotalMilliseconds:F0}ms");
        }

        // Check circuit breaker state if enabled
        if (healthOptions.IncludeCircuitBreakerState && _circuitBreaker != null)
        {
            var cbStats = _circuitBreaker.GetStats();
            if (cbStats.State == CircuitBreakerState.Open)
            {
                return new HealthCheckResult(false, $"Circuit breaker is open (opened at {cbStats.OpenedAt})");
            }
        }

        // Run custom health checks
        foreach (var customCheck in healthOptions.CustomChecks)
        {
            var (isHealthy, message) = customCheck(summary);
            if (!isHealthy)
            {
                return new HealthCheckResult(false, message);
            }
        }

        return new HealthCheckResult(true, "All health checks passed");
    }

    /// <summary>
    /// Waits for all provided tasks to complete with enhanced error handling and logging.
    /// </summary>
    /// <param name="tasks">The tasks to wait for.</param>
    /// <param name="logger">Optional logger for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the completion of all tasks.</returns>
    public static async Task WhenAllWithLoggingAsync(
        IEnumerable<Task> tasks,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var taskList = tasks.ToList();
        if (!taskList.Any()) return;

        try
        {
            await Task.WhenAll(taskList).WaitAsync(cancellationToken);
            logger?.LogInformation("All {TaskCount} tasks completed successfully", taskList.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning("Task execution was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            var completedTasks = taskList.Count(t => t.IsCompletedSuccessfully);
            var faultedTasks = taskList.Count(t => t.IsFaulted);
            var cancelledTasks = taskList.Count(t => t.IsCanceled);

            logger?.LogError(ex,
                "Task execution completed with errors. Completed: {Completed}, Faulted: {Faulted}, Cancelled: {Cancelled}",
                completedTasks, faultedTasks, cancelledTasks);
        }
    }

    private async Task<EnhancedTaskResult<object>> ProcessSingleTaskAsync(
        TaskDefinition taskDefinition,
        CancellationToken cancellationToken)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);

        try
        {
            // Check circuit breaker
            if (_circuitBreaker?.ShouldReject() == true)
            {
                var result = GetPooledResult();
                result.Name = taskDefinition.Name;
                result.SetError(new InvalidOperationException("Circuit breaker is open"));
                return result;
            }

            EnhancedTaskResult<object> taskResult;

            // Use retry handler if configured
            if (_retryHandler != null)
            {
                taskResult = await _retryHandler.ExecuteWithRetryAsync(
                    taskDefinition.Name,
                    taskDefinition.Factory,
                    cancellationToken);
            }
            else
            {
                taskResult = await ExecuteSingleAttemptAsync(taskDefinition, cancellationToken);
            }

            // Update circuit breaker
            if (_circuitBreaker != null)
            {
                if (taskResult.IsSuccessful)
                    _circuitBreaker.RecordSuccess();
                else
                    _circuitBreaker.RecordFailure();
            }

            // Record telemetry
            var telemetry = new TaskTelemetry(
                taskResult.Name,
                (long)taskResult.ExecutionTime.TotalMilliseconds,
                taskResult.Exception?.GetType().Name,
                taskResult.ErrorMessage,
                taskResult.IsSuccessful);

            _telemetry.Add(telemetry);
            _taskResults.Add(taskResult);

            return taskResult;
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    private async Task<EnhancedTaskResult<object>> ExecuteSingleAttemptAsync(
        TaskDefinition taskDefinition,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = GetPooledResult();
        result.Name = taskDefinition.Name;
        result.StartTime = DateTimeOffset.UtcNow;

        try
        {
            // Apply task-specific timeout if configured
            var taskTimeout = taskDefinition.Timeout ?? _options.DefaultTaskTimeout;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(taskTimeout);

            result.Data = await taskDefinition.Factory(timeoutCts.Token);
            result.IsSuccessful = true;

            _logger?.LogDebug("Task '{TaskName}' completed successfully in {ElapsedMs}ms",
                taskDefinition.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var ex = new OperationCanceledException($"Task '{taskDefinition.Name}' was cancelled.");
            result.SetError(ex);
            _logger?.LogWarning("Task '{TaskName}' was cancelled after {ElapsedMs}ms",
                taskDefinition.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            result.SetError(ex);
            _logger?.LogError(ex, "Task '{TaskName}' failed after {ElapsedMs}ms",
                taskDefinition.Name, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    private async Task ProcessSingleTaskWithManagementAsync(
        TaskDefinition taskDefinition,
        Action onCompleted,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await ProcessSingleTaskAsync(taskDefinition, cancellationToken);
            TaskCompleted?.Invoke(this, result);
        }
        finally
        {
            onCompleted();
        }
    }

    private List<TaskDefinition> ApplySchedulingStrategy(IEnumerable<TaskDefinition> tasks)
    {
        return _options.SchedulingStrategy switch
        {
            TaskSchedulingStrategy.FirstInFirstOut => tasks.ToList(),
            TaskSchedulingStrategy.LastInFirstOut => tasks.Reverse().ToList(),
            TaskSchedulingStrategy.Priority => tasks.OrderByDescending(t => t.Priority).ToList(),
            TaskSchedulingStrategy.ShortestJobFirst => tasks.OrderBy(t => t.EstimatedExecutionTime ?? TimeSpan.Zero).ToList(),
            TaskSchedulingStrategy.Random => tasks.OrderBy(_ => Random.Shared.Next()).ToList(),
            _ => tasks.ToList()
        };
    }

    private void UpdateProgress(
        int completed,
        int total,
        string? currentTask,
        TimeSpan elapsed,
        IProgress<TaskProgress>? progress = null,
        TimeSpan? estimated = null,
        double successRate = 0.0)
    {
        lock (_progressLock)
        {
            _currentProgress = new TaskProgress(completed, total, currentTask, elapsed, estimated, successRate);
        }

        if (_options.EnableProgressReporting)
        {
            progress?.Report(_currentProgress);
            ProgressChanged?.Invoke(this, _currentProgress);
        }
    }

    private TimeSpan? EstimateRemainingTime(int completed, int total, TimeSpan elapsed)
    {
        if (completed == 0 || total == 0) return null;

        var avgTimePerTask = elapsed.TotalMilliseconds / completed;
        var remaining = total - completed;
        return TimeSpan.FromMilliseconds(avgTimePerTask * remaining);
    }

    private double CalculateSuccessRate()
    {
        var telemetryList = _telemetry.ToList();
        if (!telemetryList.Any()) return 0.0;

        return (double)telemetryList.Count(t => t.IsSuccessful) / telemetryList.Count * 100;
    }

    private EnhancedTaskResult<object> GetPooledResult()
    {
        if (_resultPool != null)
        {
            var pooled = _resultPool.Get();
            // Reset the pooled object
            pooled.Name = "UNKNOWN";
            pooled.Data = null;
            pooled.IsSuccessful = false;
            pooled.ErrorMessage = null;
            pooled.ErrorCategory = null;
            pooled.IsRetryable = false;
            pooled.AttemptNumber = 1;
            pooled.Exception = null;
            pooled.Metadata.Clear();
            pooled.Timestamp = DateTimeOffset.UtcNow;
            pooled.StartTime = DateTimeOffset.UtcNow;
            pooled.ExecutionTime = TimeSpan.Zero;
            return pooled;
        }

        return new EnhancedTaskResult<object>();
    }

    private void ReturnPooledResult(EnhancedTaskResult<object> result)
    {
        _resultPool?.Return(result);
    }

    private ObjectPool<EnhancedTaskResult<object>> CreateResultPool()
    {
        var poolOptions = _options.MemoryPoolOptions ?? new MemoryPoolOptions();

        var policy = new DefaultPooledObjectPolicy<EnhancedTaskResult<object>>();
        var provider = new DefaultObjectPoolProvider
        {
            MaximumRetained = poolOptions.MaxPoolSize
        };

        return provider.Create(policy);
    }

    private async Task PrewarmObjectPoolAsync()
    {
        if (_resultPool == null || _options.MemoryPoolOptions == null) return;

        var prewarmedObjects = new List<EnhancedTaskResult<object>>();

        for (int i = 0; i < _options.MemoryPoolOptions.InitialPoolSize; i++)
        {
            prewarmedObjects.Add(_resultPool.Get());
        }

        await Task.Delay(1); // Yield to allow pool to stabilize

        foreach (var obj in prewarmedObjects)
        {
            _resultPool.Return(obj);
        }
    }

    private async Task ExportTelemetryAsync(CancellationToken cancellationToken)
    {
        if (_options.TelemetryExporter == null) return;

        try
        {
            await _options.TelemetryExporter.ExportAsync(_telemetry, cancellationToken);
            _logger?.LogDebug("Telemetry exported successfully to '{ExporterName}'", _options.TelemetryExporter.Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to export telemetry to '{ExporterName}'", _options.TelemetryExporter.Name);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _cancellationTokenSource?.Cancel();
            _concurrencyLimiter?.Dispose();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            _cancellationTokenSource?.Cancel();

            // Export final telemetry
            if (_options.TelemetryExporter != null && _options.EnableDetailedTelemetry)
            {
                await ExportTelemetryAsync(CancellationToken.None);
            }

            _concurrencyLimiter?.Dispose();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Health check result for the task processor.
/// </summary>
public record HealthCheckResult(bool IsHealthy, string Message);
