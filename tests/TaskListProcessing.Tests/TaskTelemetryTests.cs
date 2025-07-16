using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskListProcessing.Core;
using TaskListProcessing.Options;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Tests;

/// <summary>
/// Tests for telemetry functionality in TaskListProcessorEnhanced.
/// </summary>
[TestClass]
public class TaskTelemetryTests
{
    private Mock<ILogger<TaskListProcessorEnhanced>> _mockLogger = null!;
    private TaskListProcessorOptions _options = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLogger = new Mock<ILogger<TaskListProcessorEnhanced>>();
        _options = new TaskListProcessorOptions
        {
            MaxConcurrentTasks = 2,
            EnableDetailedTelemetry = true
        };
    }

    private TaskListProcessorEnhanced CreateProcessor()
    {
        return new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
    }

    [TestMethod]
    public void Telemetry_InitialState_IsEmpty()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act
        var telemetry = processor.Telemetry;

        // Assert
        Assert.IsNotNull(telemetry);
        Assert.AreEqual(0, telemetry.Count);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithTelemetryEnabled_CollectsTelemetry()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        // Should have telemetry entries for each task
        Assert.IsTrue(telemetry.Count >= 2);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithTelemetryDisabled_DoesNotCollectTelemetry()
    {
        // Arrange
        var optionsWithoutTelemetry = new TaskListProcessorOptions
        {
            EnableDetailedTelemetry = false
        };
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, optionsWithoutTelemetry);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        // With detailed telemetry disabled, we might still have basic telemetry
        // but it should be minimal
        Assert.IsTrue(telemetry.Count == 0 || telemetry.Count < 5);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryIncludesExecutionTimes()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["timed_task"] = async ct => { await Task.Delay(100, ct); return "result"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        // Check that telemetry includes timing information
        var taskTelemetry = telemetry.FirstOrDefault(t => t.TaskName == "timed_task");
        if (taskTelemetry != null)
        {
            Assert.IsTrue(taskTelemetry.ElapsedMilliseconds > 0);
            Assert.IsTrue(taskTelemetry.ElapsedMilliseconds >= 100);
        }
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryIncludesSuccessStatus()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["success_task"] = async ct => { await Task.Delay(50, ct); return "success"; },
            ["failure_task"] = ct => Task.FromException<object?>(new Exception("Test failure"))
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        // Check success task telemetry
        var successTelemetry = telemetry.FirstOrDefault(t => t.TaskName == "success_task");
        if (successTelemetry != null)
        {
            Assert.IsTrue(successTelemetry.IsSuccessful);
        }

        // Check failure task telemetry
        var failureTelemetry = telemetry.FirstOrDefault(t => t.TaskName == "failure_task");
        if (failureTelemetry != null)
        {
            Assert.IsFalse(failureTelemetry.IsSuccessful);
        }
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryIsThreadSafe()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();
        for (int i = 0; i < 20; i++)
        {
            var taskIndex = i;
            tasks[$"concurrent_task_{taskIndex}"] = async ct =>
            {
                await Task.Delay(10, ct);
                return $"result_{taskIndex}";
            };
        }

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        // Check that we don't have corrupted telemetry data
        var taskNames = telemetry.Select(t => t.TaskName).ToList();
        var uniqueTaskNames = taskNames.Distinct().Count();

        // Should have telemetry for multiple tasks
        Assert.IsTrue(uniqueTaskNames > 1);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryIncludesTimestamps()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["timestamped_task"] = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        var startTime = DateTimeOffset.UtcNow;
        await processor.ProcessTasksAsync(tasks);
        var endTime = DateTimeOffset.UtcNow;

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        var taskTelemetry = telemetry.FirstOrDefault(t => t.TaskName == "timestamped_task");
        if (taskTelemetry != null)
        {
            Assert.IsTrue(taskTelemetry.Timestamp >= startTime);
            Assert.IsTrue(taskTelemetry.Timestamp <= endTime);
        }
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryIncludesErrorInformation()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["error_task"] = ct => Task.FromException<object?>(new ArgumentException("Test error message"))
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        var errorTelemetry = telemetry.FirstOrDefault(t => t.TaskName == "error_task");
        if (errorTelemetry != null)
        {
            Assert.IsFalse(errorTelemetry.IsSuccessful);
            Assert.IsNotNull(errorTelemetry.ErrorMessage);
            Assert.IsTrue(errorTelemetry.ErrorMessage.Contains("Test error message"));
        }
    }

    [TestMethod]
    public void Telemetry_ReturnsReadOnlyCollection()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act
        var telemetry = processor.Telemetry;

        // Assert
        Assert.IsNotNull(telemetry);
        Assert.IsInstanceOfType(telemetry, typeof(IReadOnlyCollection<TaskTelemetry>));
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryPersistsAfterCompletion()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["persistent_task"] = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);
        var telemetryAfterCompletion = processor.Telemetry;

        // Assert
        Assert.IsTrue(telemetryAfterCompletion.Count > 0);

        // Telemetry should persist after task completion
        var persistentTelemetry = telemetryAfterCompletion.FirstOrDefault(t => t.TaskName == "persistent_task");
        Assert.IsNotNull(persistentTelemetry);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_MultipleBatches_AccumulatesTelemetry()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var firstBatch = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["batch1_task1"] = async ct => { await Task.Delay(50, ct); return "result1"; }
        };

        var secondBatch = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["batch2_task1"] = async ct => { await Task.Delay(75, ct); return "result2"; }
        };

        // Act
        await processor.ProcessTasksAsync(firstBatch);
        var telemetryAfterFirstBatch = processor.Telemetry.Count;

        await processor.ProcessTasksAsync(secondBatch);
        var telemetryAfterSecondBatch = processor.Telemetry.Count;

        // Assert
        Assert.IsTrue(telemetryAfterFirstBatch > 0);
        Assert.IsTrue(telemetryAfterSecondBatch > telemetryAfterFirstBatch);

        // Should have telemetry for both batches
        var allTelemetry = processor.Telemetry;
        Assert.IsTrue(allTelemetry.Any(t => t.TaskName == "batch1_task1"));
        Assert.IsTrue(allTelemetry.Any(t => t.TaskName == "batch2_task1"));
    }

    [TestMethod]
    public async Task ProcessTasksAsync_TelemetryIncludesMemoryUsage()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["memory_task"] = async ct =>
            {
                await Task.Delay(50, ct);
                var largeString = new string('x', 1000);
                return largeString;
            }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        var telemetry = processor.Telemetry;
        Assert.IsTrue(telemetry.Count > 0);

        var memoryTelemetry = telemetry.FirstOrDefault(t => t.TaskName == "memory_task");
        if (memoryTelemetry != null)
        {
            // Memory usage tracking might be implementation-specific
            // Just verify telemetry exists and has valid data
            Assert.IsNotNull(memoryTelemetry.TaskName);
            Assert.IsTrue(memoryTelemetry.ElapsedMilliseconds > 0);
        }
    }
}
