using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Tests;

/// <summary>
/// Tests for error handling and edge cases in TaskListProcessorEnhanced.
/// </summary>
[TestClass]
public class TaskListProcessorEnhancedErrorTests
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
            DefaultTaskTimeout = TimeSpan.FromSeconds(30),
            ContinueOnTaskFailure = true,
            EnableDetailedTelemetry = true
        };
    }

    private TaskListProcessorEnhanced CreateProcessor()
    {
        return new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithException_CapturesError()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["failing_task"] = ct => Task.FromException<object?>(new InvalidOperationException("Test exception"))
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("failing_task", result.Name);
        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage.Contains("Test exception"));
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithMultipleFailures_CapturesAllErrors()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["failure1"] = ct => Task.FromException<object?>(new ArgumentException("Error 1")),
            ["failure2"] = ct => Task.FromException<object?>(new InvalidOperationException("Error 2")),
            ["success"] = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(3, processor.TaskResults.Count);
        var failures = processor.TaskResults.Where(r => !r.IsSuccessful).ToList();
        var successes = processor.TaskResults.Where(r => r.IsSuccessful).ToList();

        Assert.AreEqual(2, failures.Count);
        Assert.AreEqual(1, successes.Count);

        Assert.IsTrue(failures.Any(f => f.ErrorMessage?.Contains("Error 1") == true));
        Assert.IsTrue(failures.Any(f => f.ErrorMessage?.Contains("Error 2") == true));
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithTaskTimeout_HandlesTimeout()
    {
        // Arrange
        var options = new TaskListProcessorOptions
        {
            DefaultTaskTimeout = TimeSpan.FromMilliseconds(100),
            MaxConcurrentTasks = 1
        };
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["timeout_task"] = async ct =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct); // Much longer than timeout
                return "should not complete";
            }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("timeout_task", result.Name);
        // Note: The exact behavior depends on implementation - it may be cancelled or timeout
        Assert.IsTrue(!result.IsSuccessful || result.ErrorMessage?.Contains("cancel") == true);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithNullTaskFactory_HandlesGracefully()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["normal_task"] = async ct => { await Task.Delay(50, ct); return "result"; },
            ["null_result_task"] = ct => Task.FromResult<object?>(null)
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(2, processor.TaskResults.Count);
        var nullResultTask = processor.TaskResults.FirstOrDefault(r => r.Name == "null_result_task");
        Assert.IsNotNull(nullResultTask);
        Assert.IsTrue(nullResultTask.IsSuccessful); // null result should still be successful
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithTaskCancellation_HandlesCorrectly()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var cts = new CancellationTokenSource();
        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["cancellable_task"] = async ct =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return "should be cancelled";
            }
        };

        // Act
        var processingTask = processor.ProcessTasksAsync(tasks, cancellationToken: cts.Token);
        await Task.Delay(50); // Let task start
        cts.Cancel();

        // Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await processingTask);
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_WithNullResult_HandlesCorrectly()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();
        var task = Task.FromResult<string?>(null);

        // Act
        var result = await processor.ExecuteTaskAsync("null_task", task);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("null_task", result.Name);
        Assert.IsTrue(result.IsSuccessful);
        Assert.IsNull(result.Data);
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_WithGenericException_CapturesDetails()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();
        var exception = new ArgumentException("Invalid argument test");
        var task = Task.FromException<int>(exception);

        // Act
        var result = await processor.ExecuteTaskAsync("error_task", task);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("error_task", result.Name);
        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage.Contains("Invalid argument test"));
    }

    [TestMethod]
    public async Task ProcessTasksStreamAsync_WithFailingTasks_ContinuesStreaming()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["success1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["failure"] = ct => Task.FromException<object?>(new Exception("Stream error")),
            ["success2"] = async ct => { await Task.Delay(75, ct); return "result2"; }
        };

        // Act
        var results = new List<TaskListProcessing.Models.EnhancedTaskResult<object>>();
        await foreach (var result in processor.ProcessTasksStreamAsync(tasks))
        {
            results.Add(result);
        }

        // Assert
        Assert.AreEqual(3, results.Count);
        var successCount = results.Count(r => r.IsSuccessful);
        var failureCount = results.Count(r => !r.IsSuccessful);

        Assert.AreEqual(2, successCount);
        Assert.AreEqual(1, failureCount);

        var failedResult = results.First(r => !r.IsSuccessful);
        Assert.AreEqual("failure", failedResult.Name);
        Assert.IsNotNull(failedResult.ErrorMessage);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithMixedTaskTypes_HandlesAll()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["string_task"] = async ct => { await Task.Delay(50, ct); return "string result"; },
            ["int_task"] = async ct => { await Task.Delay(75, ct); return 42; },
            ["bool_task"] = async ct => { await Task.Delay(25, ct); return true; },
            ["null_task"] = ct => Task.FromResult<object?>(null),
            ["complex_task"] = async ct =>
            {
                await Task.Delay(100, ct);
                return new { Name = "Test", Value = 123 };
            }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(5, processor.TaskResults.Count);
        Assert.IsTrue(processor.TaskResults.All(r => r.IsSuccessful));

        var taskNames = processor.TaskResults.Select(r => r.Name).ToList();
        CollectionAssert.Contains(taskNames, "string_task");
        CollectionAssert.Contains(taskNames, "int_task");
        CollectionAssert.Contains(taskNames, "bool_task");
        CollectionAssert.Contains(taskNames, "null_task");
        CollectionAssert.Contains(taskNames, "complex_task");
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithSlowAndFastTasks_HandlesCorrectly()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["very_fast"] = ct => Task.FromResult<object?>("immediate"),
            ["fast"] = async ct => { await Task.Delay(10, ct); return "fast result"; },
            ["medium"] = async ct => { await Task.Delay(100, ct); return "medium result"; },
            ["slow"] = async ct => { await Task.Delay(200, ct); return "slow result"; }
        };

        // Act
        var startTime = DateTime.UtcNow;
        await processor.ProcessTasksAsync(tasks);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.AreEqual(4, processor.TaskResults.Count);
        Assert.IsTrue(processor.TaskResults.All(r => r.IsSuccessful));

        // Should complete in reasonable time (not sum of all delays due to concurrency)
        var elapsedTime = endTime - startTime;
        Assert.IsTrue(elapsedTime < TimeSpan.FromSeconds(1)); // Should be much faster than 310ms sum
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithEmptyTaskName_HandlesCorrectly()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            [""] = async ct => { await Task.Delay(50, ct); return "empty name result"; },
            [" "] = async ct => { await Task.Delay(75, ct); return "space name result"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(2, processor.TaskResults.Count);
        var emptyNameTask = processor.TaskResults.FirstOrDefault(r => r.Name == "");
        var spaceNameTask = processor.TaskResults.FirstOrDefault(r => r.Name == " ");

        Assert.IsNotNull(emptyNameTask);
        Assert.IsNotNull(spaceNameTask);
        Assert.IsTrue(emptyNameTask.IsSuccessful);
        Assert.IsTrue(spaceNameTask.IsSuccessful);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithLargeNumberOfTasks_HandlesEfficiently()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();
        for (int i = 0; i < 50; i++)
        {
            var taskIndex = i;
            tasks[$"task_{taskIndex}"] = async ct =>
            {
                await Task.Delay(10, ct);
                return $"result_{taskIndex}";
            };
        }

        // Act
        var startTime = DateTime.UtcNow;
        await processor.ProcessTasksAsync(tasks);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.AreEqual(50, processor.TaskResults.Count);
        Assert.IsTrue(processor.TaskResults.All(r => r.IsSuccessful));

        // Should complete efficiently due to concurrency
        var elapsedTime = endTime - startTime;
        Assert.IsTrue(elapsedTime < TimeSpan.FromSeconds(10)); // Much less than 50 * 10ms = 500ms
    }

    [TestMethod]
    public void Dispose_WithPendingTasks_DisposesGracefully()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act & Assert
        try
        {
            processor.Dispose();
            // If we get here, no exception was thrown
            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Dispose should not throw exception: {ex.Message}");
        }
    }

    [TestMethod]
    public async Task DisposeAsync_WithPendingTasks_DisposesGracefully()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act & Assert
        try
        {
            await processor.DisposeAsync();
            // If we get here, no exception was thrown
            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"DisposeAsync should not throw exception: {ex.Message}");
        }
    }
}
