using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using TaskListProcessing.Extensions;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;
using TaskListProcessing.Options;
using TaskListProcessing.Scheduling;
using TaskListProcessing.Telemetry;
using TaskListProcessing.Utilities;

namespace TaskListProcessing.Core;

/// <summary>
/// Enhanced TaskListProcessor with comprehensive enterprise features.
/// </summary>
public class TaskListProcessorEnhanced : IDisposable, IAsyncDisposable
{
    private readonly ILogger? _logger;
    private readonly TaskListProcessorOptions _options;
    private readonly ConcurrentResultCollection<ITaskResult> _taskResults = new();
    private readonly ConcurrentResultCollection<TaskTelemetry> _telemetry = new();
    private readonly ConcurrentQueue<TaskDefinition> _taskQueue = new();
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly CircuitBreaker? _circuitBreaker;
    private readonly RetryHandler? _retryHandler;
    private readonly ObjectPool<PooledTaskResult<object>>? _resultPool;
    private readonly MemoryPressureMonitor? _memoryPressureMonitor;
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
            _memoryPressureMonitor = new MemoryPressureMonitor(_logger);
        }
    }

    /// <summary>
    /// Gets the name of the task list.
    /// </summary>
    public string TaskListName { get; }

    /// <summary>
    /// Gets the collection of task results (thread-safe).
    /// </summary>
    public IReadOnlyCollection<ITaskResult> TaskResults => _taskResults.GetSnapshot();

    /// <summary>
    /// Gets the collection of telemetry data (thread-safe).
    /// </summary>
    public IReadOnlyCollection<TaskTelemetry> Telemetry => _telemetry.GetSnapshot();

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

        try
        {
            // Use SafeAwait for internal operations
            await ProcessTaskDefinitionsAsync(taskDefinitions, progress, combinedCts.Token).SafeAwait();
        }
        finally
        {
            if (_options.TelemetryExporter != null && _options.EnableDetailedTelemetry)
            {
                await ExportTelemetryAsync(CancellationToken.None).SafeAwait();
            }
        }
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

        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        var effectiveToken = combinedCts.Token;

        // Initialize progress
        UpdateProgress(0, tasks.Count, null, TimeSpan.Zero, progress);

        // Handle empty task list
        if (tasks.Count == 0)
        {
            // Report completion immediately for empty task list
            UpdateProgress(0, 0, null, TimeSpan.Zero, progress, TimeSpan.Zero, 0.0);
            return;
        }

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
                // Check for cancellation before starting each task
                effectiveToken.ThrowIfCancellationRequested();

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
                    effectiveToken);

                processingTasks.Add(processingTask);

                // Respect max concurrency
                if (processingTasks.Count >= _options.MaxConcurrentTasks)
                {
                    var completedTask = await Task.WhenAny(processingTasks).SafeAwait();
                    processingTasks.Remove(completedTask);

                    // Check if the completed task was cancelled and propagate the exception
                    if (completedTask.IsCanceled)
                    {
                        effectiveToken.ThrowIfCancellationRequested();
                    }
                }
            }

            // Wait for all remaining tasks
            await Task.WhenAll(processingTasks).SafeAwait();
        }
        catch (OperationCanceledException) when (effectiveToken.IsCancellationRequested)
        {
            // Cancel all remaining tasks
            foreach (var task in processingTasks)
            {
                if (!task.IsCompleted)
                {
                    // Wait a bit for graceful cancellation
                    await Task.Delay(100, CancellationToken.None).SafeAwait();
                }
            }
            throw new TaskCanceledException("Task processing was cancelled");
        }
        finally
        {
            // Export telemetry if configured
            if (_options.TelemetryExporter != null && _options.EnableDetailedTelemetry)
            {
                await ExportTelemetryAsync(CancellationToken.None).SafeAwait();
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
                var result = await task.WaitAsync(ct).SafeAwait();
                return (object?)result;
            }
        };

        var result = await ProcessSingleTaskAsync(taskDef, cancellationToken).SafeAwait();

        // Ensure result is not null
        if (result == null)
        {
            throw new InvalidOperationException($"ProcessSingleTaskAsync returned null for task '{taskName}'");
        }

        // Convert to strongly typed result - handle null data carefully
        T? convertedData = default;
        if (result.Data != null)
        {
            try
            {
                convertedData = (T?)result.Data;
            }
            catch (InvalidCastException)
            {
                // If cast fails, keep default value
                convertedData = default;
            }
        }

        var convertedResult = new EnhancedTaskResult<T>(taskName, convertedData, result.IsSuccessful);

        // Set properties individually with null checks
        convertedResult.ErrorMessage = result.ErrorMessage;
        convertedResult.ErrorCategory = result.ErrorCategory;
        convertedResult.ExecutionTime = result.ExecutionTime;
        convertedResult.IsRetryable = result.IsRetryable;
        convertedResult.AttemptNumber = result.AttemptNumber;
        convertedResult.Exception = result.Exception;
        convertedResult.Metadata = result.Metadata ?? new Dictionary<string, object>();

        return convertedResult;
    }

    /// <summary>
    /// Streams task results as they complete using async enumerable.
    /// </summary>
    /// <param name="taskFactories">Dictionary of task name to task factory functions.</param>
    /// <param name="cancellationToken">Cancellation token for task cancellation.</param>
    /// <returns>An async enumerable of task results.</returns>
    /// <summary>
    /// High-performance async enumerable with minimal allocations.
    /// Uses Channel for efficient producer-consumer pattern.
    /// </summary>
    public async IAsyncEnumerable<EnhancedTaskResult<object>> ProcessTasksStreamAsync(
        IDictionary<string, Func<CancellationToken, Task<object?>>> taskFactories,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        // Use Channel for high-performance producer-consumer pattern
        var channel = Channel.CreateBounded<EnhancedTaskResult<object>>(
            new BoundedChannelOptions(Math.Min(taskFactories.Count, 100))
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });

        var writer = channel.Writer;
        var reader = channel.Reader;

        // Start producer task
        var producerTask = Task.Run(async () =>
        {
            try
            {
                var tasks = taskFactories.Select(kvp => ProduceResultAsync(
                    kvp.Key, kvp.Value, writer, cancellationToken));

                await Task.WhenAll(tasks).SafeAwait();
            }
            finally
            {
                writer.Complete();
            }
        }, cancellationToken);

        // Consume results as they're produced
        await foreach (var result in reader.ReadAllAsync(cancellationToken))
        {
            yield return result;
        }

        // Ensure producer completed
        await producerTask.SafeAwait();
    }

    /// <summary>
    /// Produces a single result and writes it to the channel.
    /// </summary>
    private async Task ProduceResultAsync(
        string taskName,
        Func<CancellationToken, Task<object?>> factory,
        ChannelWriter<EnhancedTaskResult<object>> writer,
        CancellationToken cancellationToken)
    {
        var taskDef = new TaskDefinition { Name = taskName, Factory = factory };
        var result = await ProcessSingleTaskAsync(taskDef, cancellationToken).SafeAwait();

        await writer.WriteAsync(result, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets comprehensive telemetry summary statistics.
    /// </summary>
    /// <returns>Summary of telemetry data.</returns>
    public TelemetrySummary GetTelemetrySummary()
    {
        var telemetryList = _telemetry.GetSnapshot();

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
        await _concurrencyLimiter.WaitAsync(cancellationToken).SafeAwait();

        try
        {
            // Check circuit breaker
            if (_circuitBreaker?.ShouldReject() == true)
            {
                using var result = GetPooledResult();
                result.Name = taskDefinition.Name;
                result.SetError(new InvalidOperationException("Circuit breaker is open"));
                return CreateResultCopy(result);
            }

            EnhancedTaskResult<object> taskResult;

            // Use retry handler if configured
            if (_retryHandler != null)
            {
                var retryResult = await _retryHandler.ExecuteWithRetryAsync(
                    taskDefinition.Name,
                    taskDefinition.Factory,
                    cancellationToken).SafeAwait();

                // Convert the result to the expected type
                using var pooledResult = GetPooledResult();
                pooledResult.Name = retryResult.Name;
                pooledResult.Data = retryResult.Data;
                pooledResult.IsSuccessful = retryResult.IsSuccessful;
                pooledResult.ErrorMessage = retryResult.ErrorMessage;
                pooledResult.Exception = retryResult.Exception;
                pooledResult.ExecutionTime = retryResult.ExecutionTime;
                pooledResult.AttemptNumber = retryResult.AttemptNumber;
                pooledResult.IsRetryable = retryResult.IsRetryable;
                pooledResult.StartTime = retryResult.StartTime;
                pooledResult.Timestamp = retryResult.Timestamp;
                taskResult = CreateResultCopy(pooledResult);
            }
            else
            {
                taskResult = await ExecuteSingleAttemptAsync(taskDefinition, cancellationToken).SafeAwait();
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
        using var result = GetPooledResult();
        result.Name = taskDefinition.Name;
        result.StartTime = DateTimeOffset.UtcNow;

        try
        {
            // Apply task-specific timeout if configured
            var taskTimeout = taskDefinition.Timeout ?? _options.DefaultTaskTimeout;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(taskTimeout);

            result.Data = await taskDefinition.Factory(timeoutCts.Token).SafeAwait();
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
            throw; // Re-throw the cancellation exception
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

        return CreateResultCopy(result);
    }

    private async Task ProcessSingleTaskWithManagementAsync(
        TaskDefinition taskDefinition,
        Action onCompleted,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await ProcessSingleTaskAsync(taskDefinition, cancellationToken).SafeAwait();
            TaskCompleted?.Invoke(this, result);
        }
        finally
        {
            // Ensure progress callback is called
            await Task.Yield();
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

        // Always report progress if a progress reporter is provided
        progress?.Report(_currentProgress);

        if (_options.EnableProgressReporting)
        {
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
        var telemetryList = _telemetry.GetSnapshot();
        if (!telemetryList.Any()) return 0.0;

        return (double)telemetryList.Count(t => t.IsSuccessful) / telemetryList.Count * 100;
    }

    private PooledTaskResult<object> GetPooledResult()
    {
        if (_resultPool != null)
        {
            var pooled = _resultPool.Get();
            // Object is already reset by pool policy
            return pooled;
        }

        // Return non-pooled result when pooling is disabled
        return new PooledTaskResult<object>(null);
    }

    /// <summary>
    /// Creates a copy of a pooled result for return since original will be disposed.
    /// </summary>
    /// <param name="source">The pooled result to copy.</param>
    /// <returns>A new EnhancedTaskResult with copied data.</returns>
    private EnhancedTaskResult<object> CreateResultCopy(PooledTaskResult<object> source)
    {
        return new EnhancedTaskResult<object>(source.Name, source.Data, source.IsSuccessful)
        {
            ErrorMessage = source.ErrorMessage,
            ErrorCategory = source.ErrorCategory,
            ExecutionTime = source.ExecutionTime,
            IsRetryable = source.IsRetryable,
            AttemptNumber = source.AttemptNumber,
            Exception = source.Exception,
            Metadata = new Dictionary<string, object>(source.Metadata),
            Timestamp = source.Timestamp,
            StartTime = source.StartTime
        };
    }

    private void ReturnPooledResult(PooledTaskResult<object> result)
    {
        _resultPool?.Return(result);
    }

    private ObjectPool<PooledTaskResult<object>> CreateResultPool()
    {
        var poolOptions = _options.MemoryPoolOptions ?? new MemoryPoolOptions();

        var provider = new DefaultObjectPoolProvider
        {
            MaximumRetained = poolOptions.MaxPoolSize
        };

        // Create a temporary pool for the policy
        var tempPool = provider.Create(new PooledTaskResultPolicy<object>(null!));

        // Create the actual policy with the pool reference
        var policy = new PooledTaskResultPolicy<object>(tempPool);

        return provider.Create(policy);
    }

    private async Task PrewarmObjectPoolAsync()
    {
        if (_resultPool == null || _options.MemoryPoolOptions == null) return;

        var prewarmedObjects = new List<PooledTaskResult<object>>();

        for (int i = 0; i < _options.MemoryPoolOptions.InitialPoolSize; i++)
        {
            prewarmedObjects.Add(_resultPool.Get());
        }

        await Task.Delay(1).SafeAwait(); // Yield to allow pool to stabilize

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
            await _options.TelemetryExporter.ExportAsync(_telemetry.GetSnapshot(), cancellationToken).SafeAwait();
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
            _taskResults?.Dispose();
            _telemetry?.Dispose();
            _memoryPressureMonitor?.Dispose();
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
                await ExportTelemetryAsync(CancellationToken.None).SafeAwait();
            }

            _concurrencyLimiter?.Dispose();
            _cancellationTokenSource?.Dispose();
            _taskResults?.Dispose();
            _telemetry?.Dispose();
            _memoryPressureMonitor?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Health check result for the task processor.
/// </summary>
// HealthCheckResult is now in TaskListProcessing.Models namespace
