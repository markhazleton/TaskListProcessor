using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskListProcessing.Extensions;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Examples;

/// <summary>
/// Example demonstrating the use of segregated interfaces with dependency injection.
/// </summary>
public class TaskProcessorExample
{
    public static async Task Main(string[] args)
    {
        // Configure services with TaskListProcessor
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Add TaskListProcessor with decorators
        services.AddTaskListProcessor(options =>
        {
            options.MaxConcurrentTasks = 5;
            options.EnableDetailedTelemetry = true;
            options.EnableProgressReporting = true;
        })
        .WithLogging()
        .WithMetrics()
        .WithCircuitBreaker();

        // Register example service
        services.AddTransient<ExampleService>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Run example
        var exampleService = serviceProvider.GetRequiredService<ExampleService>();
        await exampleService.RunExampleAsync();
    }
}

/// <summary>
/// Example service demonstrating different processor interfaces.
/// </summary>
public class ExampleService
{
    private readonly ITaskProcessor _taskProcessor;
    private readonly ITaskBatchProcessor _batchProcessor;
    private readonly ITaskStreamProcessor _streamProcessor;
    private readonly ITaskTelemetryProvider _telemetryProvider;
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(
        ITaskProcessor taskProcessor,
        ITaskBatchProcessor batchProcessor,
        ITaskStreamProcessor streamProcessor,
        ITaskTelemetryProvider telemetryProvider,
        ILogger<ExampleService> logger)
    {
        _taskProcessor = taskProcessor;
        _batchProcessor = batchProcessor;
        _streamProcessor = streamProcessor;
        _telemetryProvider = telemetryProvider;
        _logger = logger;
    }

    public async Task RunExampleAsync()
    {
        _logger.LogInformation("Starting TaskProcessor examples...");

        // Example 1: Individual task execution
        await ExecuteIndividualTaskExample();

        // Example 2: Batch processing
        await ExecuteBatchProcessingExample();

        // Example 3: Streaming processing
        await ExecuteStreamingExample();

        // Example 4: Telemetry and health checks
        await ExecuteTelemetryExample();

        _logger.LogInformation("TaskProcessor examples completed.");
    }

    private async Task ExecuteIndividualTaskExample()
    {
        _logger.LogInformation("=== Individual Task Example ===");

        // Execute a single task
        var result = await _taskProcessor.ExecuteTaskAsync(
            "example-task",
            async ct =>
            {
                await Task.Delay(1000, ct);
                return $"Task completed at {DateTime.Now}";
            });

        _logger.LogInformation("Task result: {Result}, Success: {Success}",
            result.Data, result.IsSuccessful);
    }

    private async Task ExecuteBatchProcessingExample()
    {
        _logger.LogInformation("=== Batch Processing Example ===");

        // Create multiple tasks
        var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["Task-1"] = async ct =>
            {
                await Task.Delay(500, ct);
                return "Result from Task 1";
            },
            ["Task-2"] = async ct =>
            {
                await Task.Delay(800, ct);
                return "Result from Task 2";
            },
            ["Task-3"] = async ct =>
            {
                await Task.Delay(300, ct);
                return "Result from Task 3";
            },
            ["Task-4"] = async ct =>
            {
                await Task.Delay(1200, ct);
                return "Result from Task 4";
            }
        };

        // Progress reporting
        var progress = new Progress<TaskProgress>(p =>
        {
            var percentage = (double)p.CompletedTasks / p.TotalTasks * 100;
            _logger.LogInformation("Progress: {Completed}/{Total} ({Percentage:F1}%)",
                p.CompletedTasks, p.TotalTasks, percentage);
        });

        // Execute batch
        await _batchProcessor.ProcessTasksAsync(taskFactories, progress);

        // Show results
        var results = _batchProcessor.TaskResults;
        _logger.LogInformation("Batch completed with {SuccessCount} successful tasks",
            results.Count(r => r.IsSuccessful));
    }

    private async Task ExecuteStreamingExample()
    {
        _logger.LogInformation("=== Streaming Example ===");

        // Create streaming tasks
        var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["Stream-1"] = async ct =>
            {
                await Task.Delay(200, ct);
                return "Stream result 1";
            },
            ["Stream-2"] = async ct =>
            {
                await Task.Delay(400, ct);
                return "Stream result 2";
            },
            ["Stream-3"] = async ct =>
            {
                await Task.Delay(100, ct);
                return "Stream result 3";
            }
        };

        // Process tasks and stream results
        await foreach (var result in _streamProcessor.ProcessTasksStreamAsync(taskFactories))
        {
            _logger.LogInformation("Streamed result: {TaskName} = {Result}",
                result.Name, result.Data);
        }
    }

    private async Task ExecuteTelemetryExample()
    {
        _logger.LogInformation("=== Telemetry Example ===");

        // Get telemetry summary
        var summary = _telemetryProvider.GetTelemetrySummary();
        _logger.LogInformation("Telemetry Summary - Total: {Total}, Success: {Success}, Failed: {Failed}, Success Rate: {Rate:F1}%",
            summary.TotalTasks, summary.SuccessfulTasks, summary.FailedTasks, summary.SuccessRate);

        // Perform health check
        var health = _telemetryProvider.PerformHealthCheck();
        _logger.LogInformation("Health Check: {Status} - {Message}",
            health.IsHealthy ? "HEALTHY" : "UNHEALTHY", health.Message);

        // Export telemetry (if configured)
        try
        {
            await _telemetryProvider.ExportTelemetryAsync();
            _logger.LogInformation("Telemetry exported successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Telemetry export failed");
        }

        // Show individual telemetry records
        var telemetryRecords = _telemetryProvider.Telemetry;
        _logger.LogInformation("Individual telemetry records: {Count}", telemetryRecords.Count);

        foreach (var record in telemetryRecords.Take(5)) // Show first 5 records
        {
            _logger.LogInformation("  Task: {TaskName}, Duration: {Duration}ms, Success: {Success}",
                record.TaskName, record.ElapsedMilliseconds, record.IsSuccessful);
        }
    }
}
