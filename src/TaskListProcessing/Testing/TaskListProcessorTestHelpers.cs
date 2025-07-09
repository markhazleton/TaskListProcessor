using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Testing;

/// <summary>
/// Test helpers and utilities for TaskListProcessor testing.
/// </summary>
public static class TaskListProcessorTestHelpers
{
    /// <summary>
    /// Creates a test processor with default configuration.
    /// </summary>
    /// <param name="name">Optional processor name.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>A configured test processor.</returns>
    public static TaskListProcessorEnhanced CreateTestProcessor(string? name = null, ILogger? logger = null)
    {
        return TaskListProcessorBuilder
            .Create(name ?? "TestProcessor")
            .WithLogger(logger ?? NullLogger.Instance)
            .WithConcurrency(2)
            .WithTelemetry(true)
            .WithMemoryPooling(false) // Disable for easier testing
            .Build();
    }

    /// <summary>
    /// Creates a processor optimized for performance testing.
    /// </summary>
    /// <param name="name">Optional processor name.</param>
    /// <param name="maxConcurrency">Maximum concurrent tasks.</param>
    /// <returns>A configured performance test processor.</returns>
    public static TaskListProcessorEnhanced CreatePerformanceTestProcessor(string? name = null, int maxConcurrency = 10)
    {
        return TaskListProcessorBuilder
            .Create(name ?? "PerformanceTestProcessor")
            .WithConcurrency(maxConcurrency)
            .WithTelemetry(false) // Disable for pure performance
            .WithMemoryPooling(true)
            .WithProgressReporting(false)
            .Build();
    }

    /// <summary>
    /// Creates a resilience test processor with comprehensive error handling.
    /// </summary>
    /// <param name="name">Optional processor name.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>A configured resilience test processor.</returns>
    public static TaskListProcessorEnhanced CreateResilienceTestProcessor(string? name = null, ILogger? logger = null)
    {
        return TaskListProcessorBuilder
            .Create(name ?? "ResilienceTestProcessor")
            .WithLogger(logger ?? NullLogger.Instance)
            .WithRetry(3, TimeSpan.FromMilliseconds(100))
            .WithCircuitBreaker(3, TimeSpan.FromSeconds(5))
            .WithConcurrency(2)
            .WithTelemetry(true)
            .Build();
    }

    /// <summary>
    /// Creates a mock task that succeeds after a delay.
    /// </summary>
    /// <param name="result">The result to return.</param>
    /// <param name="delay">Delay before completion.</param>
    /// <returns>Task factory function.</returns>
    public static Func<CancellationToken, Task<object?>> CreateSuccessTask(
        object? result = null,
        TimeSpan delay = default)
    {
        return async ct =>
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
            return result ?? "Success";
        };
    }

    /// <summary>
    /// Creates a mock task that fails with a specified exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <param name="delay">Delay before failure.</param>
    /// <returns>Task factory function.</returns>
    public static Func<CancellationToken, Task<object?>> CreateFailureTask(
        Exception? exception = null,
        TimeSpan delay = default)
    {
        return async ct =>
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
            throw exception ?? new InvalidOperationException("Test failure");
        };
    }

    /// <summary>
    /// Creates a mock task that randomly succeeds or fails.
    /// </summary>
    /// <param name="successProbability">Probability of success (0.0 to 1.0).</param>
    /// <param name="delay">Delay before completion.</param>
    /// <returns>Task factory function.</returns>
    public static Func<CancellationToken, Task<object?>> CreateRandomTask(
        double successProbability = 0.8,
        TimeSpan delay = default)
    {
        return async ct =>
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);

            if (Random.Shared.NextDouble() < successProbability)
                return "Random Success";
            else
                throw new InvalidOperationException("Random Failure");
        };
    }

    /// <summary>
    /// Creates a collection of test tasks with varying characteristics.
    /// </summary>
    /// <param name="taskCount">Number of tasks to create.</param>
    /// <param name="successRate">Percentage of tasks that should succeed.</param>
    /// <param name="minDelay">Minimum task delay.</param>
    /// <param name="maxDelay">Maximum task delay.</param>
    /// <returns>Dictionary of task factories.</returns>
    public static Dictionary<string, Func<CancellationToken, Task<object?>>> CreateTestTaskBatch(
        int taskCount = 10,
        double successRate = 0.8,
        TimeSpan minDelay = default,
        TimeSpan maxDelay = default)
    {
        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        if (minDelay == default) minDelay = TimeSpan.FromMilliseconds(50);
        if (maxDelay == default) maxDelay = TimeSpan.FromMilliseconds(200);

        for (int i = 0; i < taskCount; i++)
        {
            var taskName = $"TestTask_{i:D3}";
            var delay = TimeSpan.FromMilliseconds(
                Random.Shared.Next((int)minDelay.TotalMilliseconds, (int)maxDelay.TotalMilliseconds));

            var shouldSucceed = Random.Shared.NextDouble() < successRate;

            if (shouldSucceed)
            {
                tasks[taskName] = CreateSuccessTask($"Result_{i}", delay);
            }
            else
            {
                tasks[taskName] = CreateFailureTask(new InvalidOperationException($"Test failure {i}"), delay);
            }
        }

        return tasks;
    }

    /// <summary>
    /// Creates test tasks with dependencies for dependency resolution testing.
    /// </summary>
    /// <returns>Collection of task definitions with dependencies.</returns>
    public static List<TaskDefinition> CreateDependentTestTasks()
    {
        return new List<TaskDefinition>
        {
            new() { Name = "Task_A", Factory = CreateSuccessTask("A") },
            new() { Name = "Task_B", Factory = CreateSuccessTask("B"), Dependencies = new[] { "Task_A" } },
            new() { Name = "Task_C", Factory = CreateSuccessTask("C"), Dependencies = new[] { "Task_A" } },
            new() { Name = "Task_D", Factory = CreateSuccessTask("D"), Dependencies = new[] { "Task_B", "Task_C" } },
            new() { Name = "Task_E", Factory = CreateSuccessTask("E") }
        };
    }

    /// <summary>
    /// Measures the execution time of a processor operation.
    /// </summary>
    /// <param name="operation">The operation to measure.</param>
    /// <returns>Execution time and operation result.</returns>
    public static async Task<(TimeSpan Duration, T Result)> MeasureExecutionTime<T>(Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (stopwatch.Elapsed, result);
    }

    /// <summary>
    /// Validates that telemetry data is consistent and accurate.
    /// </summary>
    /// <param name="processor">The processor to validate.</param>
    /// <param name="expectedTaskCount">Expected number of tasks.</param>
    /// <returns>Validation result.</returns>
    public static ValidationResult ValidateTelemetry(TaskListProcessorEnhanced processor, int expectedTaskCount)
    {
        var telemetry = processor.Telemetry.ToList();
        var results = processor.TaskResults.ToList();
        var errors = new List<string>();

        // Check counts match
        if (telemetry.Count != expectedTaskCount)
            errors.Add($"Telemetry count {telemetry.Count} != expected {expectedTaskCount}");

        if (results.Count != expectedTaskCount)
            errors.Add($"Results count {results.Count} != expected {expectedTaskCount}");

        if (telemetry.Count != results.Count)
            errors.Add($"Telemetry count {telemetry.Count} != results count {results.Count}");

        // Check all tasks have both telemetry and results
        var telemetryNames = telemetry.Select(t => t.TaskName).ToHashSet();
        var resultNames = results.Select(r => r.Name).ToHashSet();

        var missingTelemetry = resultNames.Except(telemetryNames).ToList();
        var missingResults = telemetryNames.Except(resultNames).ToList();

        if (missingTelemetry.Any())
            errors.Add($"Missing telemetry for tasks: {string.Join(", ", missingTelemetry)}");

        if (missingResults.Any())
            errors.Add($"Missing results for tasks: {string.Join(", ", missingResults)}");

        // Check success/failure consistency
        foreach (var result in results)
        {
            var telemetryRecord = telemetry.FirstOrDefault(t => t.TaskName == result.Name);
            if (telemetryRecord != null && telemetryRecord.IsSuccessful != result.IsSuccessful)
            {
                errors.Add($"Success state mismatch for task '{result.Name}': telemetry={telemetryRecord.IsSuccessful}, result={result.IsSuccessful}");
            }
        }

        return new ValidationResult(errors.Count == 0, errors);
    }

    /// <summary>
    /// Creates a stress test scenario with high concurrency.
    /// </summary>
    /// <param name="taskCount">Number of tasks to create.</param>
    /// <param name="maxConcurrency">Maximum concurrent tasks.</param>
    /// <param name="taskDuration">Average task duration.</param>
    /// <returns>Stress test configuration.</returns>
    public static StressTestConfig CreateStressTest(
        int taskCount = 1000,
        int maxConcurrency = 50,
        TimeSpan taskDuration = default)
    {
        if (taskDuration == default) taskDuration = TimeSpan.FromMilliseconds(100);

        var processor = TaskListProcessorBuilder
            .Create("StressTest")
            .WithConcurrency(maxConcurrency)
            .WithMemoryPooling(true)
            .WithTelemetry(true)
            .Build();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();
        for (int i = 0; i < taskCount; i++)
        {
            var taskName = $"StressTask_{i:D6}";
            var delay = TimeSpan.FromMilliseconds(
                taskDuration.TotalMilliseconds + Random.Shared.Next(-50, 50));
            tasks[taskName] = CreateSuccessTask($"Result_{i}", delay);
        }

        return new StressTestConfig(processor, tasks, maxConcurrency, taskCount);
    }
}

/// <summary>
/// Validation result for test operations.
/// </summary>
public record ValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Gets the error summary.
    /// </summary>
    public string ErrorSummary => Errors.Any() ? string.Join("; ", Errors) : "No errors";
}

/// <summary>
/// Configuration for stress testing.
/// </summary>
public record StressTestConfig(
    TaskListProcessorEnhanced Processor,
    Dictionary<string, Func<CancellationToken, Task<object?>>> Tasks,
    int MaxConcurrency,
    int TaskCount);

/// <summary>
/// Performance benchmark utilities for TaskListProcessor.
/// </summary>
public static class TaskListProcessorBenchmarks
{
    /// <summary>
    /// Benchmarks throughput with varying task counts.
    /// </summary>
    /// <param name="taskCounts">Array of task counts to test.</param>
    /// <param name="iterations">Number of iterations per test.</param>
    /// <param name="taskDelay">Delay per task in milliseconds.</param>
    /// <returns>Benchmark results.</returns>
    public static async Task<BenchmarkResults> BenchmarkThroughputAsync(
        int[] taskCounts,
        int iterations = 3,
        int taskDelay = 100)
    {
        var results = new List<BenchmarkResult>();

        foreach (var taskCount in taskCounts)
        {
            var iterationResults = new List<TimeSpan>();
            var iterationThroughputs = new List<double>();

            for (int i = 0; i < iterations; i++)
            {
                using var processor = TaskListProcessorTestHelpers.CreatePerformanceTestProcessor(
                    $"Benchmark_{taskCount}_{i}", Environment.ProcessorCount * 2);

                var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();
                for (int j = 0; j < taskCount; j++)
                {
                    tasks[$"Task_{j}"] = TaskListProcessorTestHelpers.CreateSuccessTask(
                        $"Result_{j}", TimeSpan.FromMilliseconds(taskDelay));
                }

                var stopwatch = Stopwatch.StartNew();
                await processor.ProcessTasksAsync(tasks);
                stopwatch.Stop();

                var throughput = taskCount / stopwatch.Elapsed.TotalSeconds;
                iterationResults.Add(stopwatch.Elapsed);
                iterationThroughputs.Add(throughput);

                // Allow GC between iterations
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(100);
            }

            results.Add(new BenchmarkResult
            {
                TaskCount = taskCount,
                Iterations = iterations,
                AverageExecutionTime = TimeSpan.FromMilliseconds(iterationResults.Average(t => t.TotalMilliseconds)),
                MinExecutionTime = iterationResults.Min(),
                MaxExecutionTime = iterationResults.Max(),
                AverageThroughput = iterationThroughputs.Average(),
                MinThroughput = iterationThroughputs.Min(),
                MaxThroughput = iterationThroughputs.Max()
            });
        }

        return new BenchmarkResults(results);
    }

    /// <summary>
    /// Benchmarks memory usage and GC pressure.
    /// </summary>
    /// <param name="taskCount">Number of tasks to execute.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <returns>Memory benchmark results.</returns>
    public static async Task<MemoryBenchmarkResult> BenchmarkMemoryUsageAsync(
        int taskCount = 1000,
        int iterations = 5)
    {
        var results = new List<MemoryMeasurement>();

        for (int i = 0; i < iterations; i++)
        {
            // Force GC before measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryBefore = GC.GetTotalMemory(false);
            var gen0Before = GC.CollectionCount(0);
            var gen1Before = GC.CollectionCount(1);
            var gen2Before = GC.CollectionCount(2);

            using var processor = TaskListProcessorTestHelpers.CreatePerformanceTestProcessor(
                $"MemoryBenchmark_{i}", Environment.ProcessorCount);

            var tasks = TaskListProcessorTestHelpers.CreateTestTaskBatch(
                taskCount, 1.0, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(50));

            await processor.ProcessTasksAsync(tasks);

            var memoryAfter = GC.GetTotalMemory(false);
            var gen0After = GC.CollectionCount(0);
            var gen1After = GC.CollectionCount(1);
            var gen2After = GC.CollectionCount(2);

            results.Add(new MemoryMeasurement
            {
                MemoryBefore = memoryBefore,
                MemoryAfter = memoryAfter,
                MemoryUsed = memoryAfter - memoryBefore,
                Gen0Collections = gen0After - gen0Before,
                Gen1Collections = gen1After - gen1Before,
                Gen2Collections = gen2After - gen2Before
            });
        }

        return new MemoryBenchmarkResult(results);
    }

    /// <summary>
    /// Benchmarks concurrent processing vs sequential processing.
    /// </summary>
    /// <param name="taskCount">Number of tasks to test.</param>
    /// <param name="taskDelay">Delay per task.</param>
    /// <returns>Concurrency comparison results.</returns>
    public static async Task<ConcurrencyBenchmarkResult> BenchmarkConcurrencyAsync(
        int taskCount = 100,
        int taskDelay = 100)
    {
        // Sequential processing (concurrency = 1)
        using var sequentialProcessor = TaskListProcessorBuilder
            .Create("Sequential")
            .WithConcurrency(1)
            .WithTelemetry(false)
            .Build();

        var tasks = TaskListProcessorTestHelpers.CreateTestTaskBatch(
            taskCount, 1.0, TimeSpan.FromMilliseconds(taskDelay), TimeSpan.FromMilliseconds(taskDelay));

        var sequentialStopwatch = Stopwatch.StartNew();
        await sequentialProcessor.ProcessTasksAsync(tasks);
        sequentialStopwatch.Stop();

        // Concurrent processing (optimal concurrency)
        using var concurrentProcessor = TaskListProcessorBuilder
            .Create("Concurrent")
            .WithConcurrency(Environment.ProcessorCount * 2)
            .WithTelemetry(false)
            .Build();

        var concurrentStopwatch = Stopwatch.StartNew();
        await concurrentProcessor.ProcessTasksAsync(tasks);
        concurrentStopwatch.Stop();

        var speedup = sequentialStopwatch.Elapsed.TotalMilliseconds / concurrentStopwatch.Elapsed.TotalMilliseconds;
        var efficiency = speedup / (Environment.ProcessorCount * 2);

        return new ConcurrencyBenchmarkResult
        {
            TaskCount = taskCount,
            TaskDelay = TimeSpan.FromMilliseconds(taskDelay),
            SequentialTime = sequentialStopwatch.Elapsed,
            ConcurrentTime = concurrentStopwatch.Elapsed,
            Speedup = speedup,
            Efficiency = efficiency,
            MaxConcurrency = Environment.ProcessorCount * 2
        };
    }
}

/// <summary>
/// Results from performance benchmarks.
/// </summary>
public record BenchmarkResults(IReadOnlyList<BenchmarkResult> Results)
{
    /// <summary>
    /// Gets the best performing result.
    /// </summary>
    public BenchmarkResult? BestThroughput => Results.OrderByDescending(r => r.AverageThroughput).FirstOrDefault();

    /// <summary>
    /// Gets the worst performing result.
    /// </summary>
    public BenchmarkResult? WorstThroughput => Results.OrderBy(r => r.AverageThroughput).FirstOrDefault();

    /// <summary>
    /// Formats results as a summary table.
    /// </summary>
    public string FormatSummary()
    {
        var lines = new List<string>
        {
            "=== THROUGHPUT BENCHMARK RESULTS ===",
            $"{"Tasks",-8} {"Avg Time",-12} {"Min Time",-12} {"Max Time",-12} {"Avg Tput",-12} {"Min Tput",-12} {"Max Tput",-12}",
            new string('-', 85)
        };

        foreach (var result in Results.OrderBy(r => r.TaskCount))
        {
            lines.Add($"{result.TaskCount,-8} " +
                     $"{result.AverageExecutionTime.TotalMilliseconds,-12:F0}ms " +
                     $"{result.MinExecutionTime.TotalMilliseconds,-12:F0}ms " +
                     $"{result.MaxExecutionTime.TotalMilliseconds,-12:F0}ms " +
                     $"{result.AverageThroughput,-12:F1}/s " +
                     $"{result.MinThroughput,-12:F1}/s " +
                     $"{result.MaxThroughput,-12:F1}/s");
        }

        return string.Join(Environment.NewLine, lines);
    }
}

/// <summary>
/// Individual benchmark result.
/// </summary>
public record BenchmarkResult
{
    public int TaskCount { get; init; }
    public int Iterations { get; init; }
    public TimeSpan AverageExecutionTime { get; init; }
    public TimeSpan MinExecutionTime { get; init; }
    public TimeSpan MaxExecutionTime { get; init; }
    public double AverageThroughput { get; init; }
    public double MinThroughput { get; init; }
    public double MaxThroughput { get; init; }
}

/// <summary>
/// Memory benchmark measurement.
/// </summary>
public record MemoryMeasurement
{
    public long MemoryBefore { get; init; }
    public long MemoryAfter { get; init; }
    public long MemoryUsed { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
}

/// <summary>
/// Memory benchmark results.
/// </summary>
public record MemoryBenchmarkResult(IReadOnlyList<MemoryMeasurement> Measurements)
{
    /// <summary>
    /// Gets the average memory used.
    /// </summary>
    public long AverageMemoryUsed => (long)Measurements.Average(m => m.MemoryUsed);

    /// <summary>
    /// Gets the total GC collections across all generations.
    /// </summary>
    public int TotalCollections => Measurements.Sum(m => m.Gen0Collections + m.Gen1Collections + m.Gen2Collections);

    /// <summary>
    /// Formats memory results as a summary.
    /// </summary>
    public string FormatSummary()
    {
        return $"Memory Usage: {AverageMemoryUsed / 1024:N0} KB average, " +
               $"GC Collections: {TotalCollections} total " +
               $"(Gen0: {Measurements.Sum(m => m.Gen0Collections)}, " +
               $"Gen1: {Measurements.Sum(m => m.Gen1Collections)}, " +
               $"Gen2: {Measurements.Sum(m => m.Gen2Collections)})";
    }
}

/// <summary>
/// Concurrency benchmark results.
/// </summary>
public record ConcurrencyBenchmarkResult
{
    public int TaskCount { get; init; }
    public TimeSpan TaskDelay { get; init; }
    public TimeSpan SequentialTime { get; init; }
    public TimeSpan ConcurrentTime { get; init; }
    public double Speedup { get; init; }
    public double Efficiency { get; init; }
    public int MaxConcurrency { get; init; }

    /// <summary>
    /// Formats concurrency results as a summary.
    /// </summary>
    public string FormatSummary()
    {
        return $"Concurrency Benchmark: {TaskCount} tasks with {TaskDelay.TotalMilliseconds}ms delay\n" +
               $"Sequential: {SequentialTime.TotalMilliseconds:F0}ms\n" +
               $"Concurrent: {ConcurrentTime.TotalMilliseconds:F0}ms\n" +
               $"Speedup: {Speedup:F2}x\n" +
               $"Efficiency: {Efficiency:F2} ({Efficiency * 100:F1}%)";
    }
}

/// <summary>
/// Integration test helpers for real-world scenarios.
/// </summary>
public static class IntegrationTestHelpers
{
    /// <summary>
    /// Creates a realistic API integration test scenario.
    /// </summary>
    /// <param name="apiEndpoints">Number of different API endpoints to simulate.</param>
    /// <param name="requestsPerEndpoint">Number of requests per endpoint.</param>
    /// <returns>Integration test configuration.</returns>
    public static async Task<IntegrationTestResult> SimulateApiIntegrationAsync(
        int apiEndpoints = 5,
        int requestsPerEndpoint = 10)
    {
        using var processor = TaskListProcessorBuilder
            .Create("ApiIntegration")
            .ForApiIntegration(maxConcurrent: 15)
            .WithTelemetryExporter(TelemetryExporterFactory.CreateMemory())
            .Build();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        // Simulate different API endpoints with varying characteristics
        var endpoints = new[]
        {
            ("Users", TimeSpan.FromMilliseconds(200), 0.95),     // Fast, reliable
            ("Orders", TimeSpan.FromMilliseconds(500), 0.90),    // Medium, mostly reliable
            ("Inventory", TimeSpan.FromMilliseconds(300), 0.85), // Medium, occasional issues
            ("Reports", TimeSpan.FromSeconds(2), 0.80),          // Slow, less reliable
            ("Analytics", TimeSpan.FromSeconds(1), 0.75)         // Slow, variable reliability
        };

        for (int i = 0; i < Math.Min(apiEndpoints, endpoints.Length); i++)
        {
            var (name, delay, successRate) = endpoints[i];

            for (int j = 0; j < requestsPerEndpoint; j++)
            {
                var taskName = $"{name}_Request_{j:D3}";
                tasks[taskName] = TaskListProcessorTestHelpers.CreateRandomTask(successRate, delay);
            }
        }

        var stopwatch = Stopwatch.StartNew();
        await processor.ProcessTasksAsync(tasks);
        stopwatch.Stop();

        var summary = processor.GetTelemetrySummary();
        return new IntegrationTestResult
        {
            TotalTasks = tasks.Count,
            ExecutionTime = stopwatch.Elapsed,
            SuccessRate = summary.SuccessRate,
            AverageTaskTime = TimeSpan.FromMilliseconds(summary.AverageExecutionTime),
            ThroughputPerSecond = tasks.Count / stopwatch.Elapsed.TotalSeconds
        };
    }

    /// <summary>
    /// Creates a data processing pipeline test scenario.
    /// </summary>
    /// <param name="recordCount">Number of records to process.</param>
    /// <returns>Data processing test result.</returns>
    public static async Task<IntegrationTestResult> SimulateDataProcessingAsync(int recordCount = 1000)
    {
        using var processor = TaskListProcessorBuilder
            .Create("DataProcessing")
            .ForDataProcessing(enableParallelism: true)
            .WithProgressReporting(true)
            .Build();

        var progressUpdates = new List<TaskProgress>();
        var progress = new Progress<TaskProgress>(p => progressUpdates.Add(p));

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        // Simulate different data processing stages
        var stages = new[]
        {
            ("Validation", TimeSpan.FromMilliseconds(50)),
            ("Transformation", TimeSpan.FromMilliseconds(100)),
            ("Enrichment", TimeSpan.FromMilliseconds(150)),
            ("Persistence", TimeSpan.FromMilliseconds(75))
        };

        for (int i = 0; i < recordCount; i++)
        {
            var stage = stages[i % stages.Length];
            var taskName = $"{stage.Item1}_Record_{i:D6}";
            tasks[taskName] = TaskListProcessorTestHelpers.CreateSuccessTask($"Processed_{i}", stage.Item2);
        }

        var stopwatch = Stopwatch.StartNew();
        await processor.ProcessTasksAsync(tasks, progress);
        stopwatch.Stop();

        var summary = processor.GetTelemetrySummary();
        return new IntegrationTestResult
        {
            TotalTasks = tasks.Count,
            ExecutionTime = stopwatch.Elapsed,
            SuccessRate = summary.SuccessRate,
            AverageTaskTime = TimeSpan.FromMilliseconds(summary.AverageExecutionTime),
            ThroughputPerSecond = tasks.Count / stopwatch.Elapsed.TotalSeconds,
            ProgressUpdates = progressUpdates.Count
        };
    }
}

/// <summary>
/// Integration test result.
/// </summary>
public record IntegrationTestResult
{
    public int TotalTasks { get; init; }
    public TimeSpan ExecutionTime { get; init; }
    public double SuccessRate { get; init; }
    public TimeSpan AverageTaskTime { get; init; }
    public double ThroughputPerSecond { get; init; }
    public int ProgressUpdates { get; init; }

    /// <summary>
    /// Formats the integration test result as a summary.
    /// </summary>
    public string FormatSummary()
    {
        return $"Integration Test Results:\n" +
               $"  Tasks: {TotalTasks:N0}\n" +
               $"  Execution Time: {ExecutionTime.TotalSeconds:F2}s\n" +
               $"  Success Rate: {SuccessRate:F1}%\n" +
               $"  Average Task Time: {AverageTaskTime.TotalMilliseconds:F0}ms\n" +
               $"  Throughput: {ThroughputPerSecond:F1} tasks/second\n" +
               $"  Progress Updates: {ProgressUpdates}";
    }
}