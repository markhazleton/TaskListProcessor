using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskListProcessing.Extensions;
using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Tests;

/// <summary>
/// Integration tests for the new segregated interfaces and dependency injection.
/// </summary>
[TestClass]
public class InterfaceSegregationTests
{
    [TestMethod]
    public void AddTaskListProcessor_RegistersAllInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        // Act
        services.AddTaskListProcessor();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(provider.GetService<ITaskProcessor>());
        Assert.IsNotNull(provider.GetService<ITaskBatchProcessor>());
        Assert.IsNotNull(provider.GetService<ITaskStreamProcessor>());
        Assert.IsNotNull(provider.GetService<ITaskTelemetryProvider>());
    }

    [TestMethod]
    public void AddTaskListProcessor_WithDecorators_RegistersDecoratedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        // Act
        services.AddTaskListProcessor()
            .WithLogging()
            .WithMetrics();

        var provider = services.BuildServiceProvider();

        // Assert
        var taskProcessor = provider.GetService<ITaskProcessor>();
        Assert.IsNotNull(taskProcessor);

        // The decorated service should still implement the interface
        Assert.IsInstanceOfType(taskProcessor, typeof(ITaskProcessor));
    }

    [TestMethod]
    public async Task TaskProcessor_ExecutesSingleTask_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTaskListProcessor();

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<ITaskProcessor>();

        // Act
        var result = await processor.ExecuteTaskAsync(
            "test-task",
            async ct =>
            {
                await Task.Delay(100, ct);
                return "test-result";
            });

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test-task", result.Name);
        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual("test-result", result.Data);
    }

    [TestMethod]
    public async Task TaskBatchProcessor_ExecutesBatch_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTaskListProcessor();

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<ITaskBatchProcessor>();

        var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; }
        };

        // Act
        await processor.ProcessTasksAsync(taskFactories);

        // Assert
        var results = processor.TaskResults;
        Assert.AreEqual(2, results.Count);
        foreach (var result in results)
        {
            Assert.IsTrue(result.IsSuccessful);
        }
    }

    [TestMethod]
    public async Task TaskStreamProcessor_StreamsResults_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTaskListProcessor();

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<ITaskStreamProcessor>();

        var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["stream1"] = async ct => { await Task.Delay(50, ct); return "stream-result1"; },
            ["stream2"] = async ct => { await Task.Delay(100, ct); return "stream-result2"; }
        };

        // Act
        var results = new List<string>();
        await foreach (var result in processor.ProcessTasksStreamAsync(taskFactories))
        {
            results.Add(result.Name);
        }

        // Assert
        Assert.AreEqual(2, results.Count);
        CollectionAssert.Contains(results, "stream1");
        CollectionAssert.Contains(results, "stream2");
    }

    [TestMethod]
    public void TaskTelemetryProvider_ProvidesHealthCheck_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTaskListProcessor();

        var provider = services.BuildServiceProvider();
        var telemetryProvider = provider.GetRequiredService<ITaskTelemetryProvider>();

        // Act
        var healthResult = telemetryProvider.PerformHealthCheck();
        var summary = telemetryProvider.GetTelemetrySummary();

        // Assert
        Assert.IsNotNull(healthResult);
        Assert.IsNotNull(summary);
        Assert.IsTrue(healthResult.IsHealthy); // Should be healthy with no tasks
    }

    [TestMethod]
    public void TaskProcessor_HasCorrectName()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTaskListProcessor();

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<ITaskProcessor>();

        // Act & Assert
        Assert.IsNotNull(processor.Name);
        Assert.IsFalse(string.IsNullOrEmpty(processor.Name));
    }
}
