using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Tests;

/// <summary>
/// Comprehensive tests for TaskListProcessorEnhanced class.
/// </summary>
[TestClass]
public class TaskListProcessorEnhancedTests
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
            EnableDetailedTelemetry = true,
            EnableProgressReporting = true
        };
    }

    private TaskListProcessorEnhanced CreateProcessor()
    {
        return new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
    }

    [TestMethod]
    public void Constructor_WithDefaultParameters_CreatesValidInstance()
    {
        // Act
        var processor = new TaskListProcessorEnhanced();

        // Assert
        Assert.IsNotNull(processor);
        Assert.AreEqual("TaskProcessor", processor.TaskListName);
        Assert.IsNotNull(processor.TaskResults);
        Assert.IsNotNull(processor.Telemetry);
        Assert.AreEqual(0, processor.TaskResults.Count);
        Assert.AreEqual(0, processor.Telemetry.Count);
    }

    [TestMethod]
    public void Constructor_WithCustomName_SetsTaskListName()
    {
        // Arrange
        const string customName = "CustomTaskProcessor";

        // Act
        var processor = new TaskListProcessorEnhanced(customName);

        // Assert
        Assert.AreEqual(customName, processor.TaskListName);
    }

    [TestMethod]
    public void Constructor_WithLogger_DoesNotThrow()
    {
        // Act & Assert
        try
        {
            var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object);
            Assert.IsNotNull(processor);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Constructor should not throw exception: {ex.Message}");
        }
    }

    [TestMethod]
    public void Constructor_WithOptions_ConfiguresCorrectly()
    {
        // Act
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);

        // Assert
        Assert.IsNotNull(processor);
        Assert.AreEqual("TestProcessor", processor.TaskListName);
    }

    [TestMethod]
    public async Task InitializeAsync_CallsInitializationOnce()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);

        // Act
        await processor.InitializeAsync();
        await processor.InitializeAsync(); // Second call should be ignored

        // Assert
        // Verify logger was called for initialization
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initializing TaskListProcessor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithEmptyTasks_CompletesSuccessfully()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();
        var emptyTasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        // Act
        await processor.ProcessTasksAsync(emptyTasks);

        // Assert
        Assert.AreEqual(0, processor.TaskResults.Count);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithSingleTask_ExecutesSuccessfully()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct =>
            {
                await Task.Delay(100, ct);
                return "result1";
            }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("task1", result.Name);
        Assert.IsTrue(result.IsSuccessful);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithMultipleTasks_ExecutesAllTasks()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; },
            ["task3"] = async ct => { await Task.Delay(75, ct); return "result3"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(3, processor.TaskResults.Count);
        var taskNames = processor.TaskResults.Select(r => r.Name).ToList();
        CollectionAssert.Contains(taskNames, "task1");
        CollectionAssert.Contains(taskNames, "task2");
        CollectionAssert.Contains(taskNames, "task3");
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithFailingTask_ContinuesProcessing()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["failing_task"] = ct => Task.FromException<object?>(new InvalidOperationException("Test error")),
            ["task3"] = async ct => { await Task.Delay(75, ct); return "result3"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(3, processor.TaskResults.Count);
        var failedTask = processor.TaskResults.FirstOrDefault(r => r.Name == "failing_task");
        Assert.IsNotNull(failedTask);
        Assert.IsFalse(failedTask.IsSuccessful);
        Assert.IsNotNull(failedTask.ErrorMessage);
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_WithGenericTask_ReturnsTypedResult()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();
        var task = Task.FromResult(42);

        // Act
        var result = await processor.ExecuteTaskAsync("test_task", task);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test_task", result.Name);
        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(42, result.Data);
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_WithFailingTask_ReturnsFailedResult()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();
        var task = Task.FromException<string>(new InvalidOperationException("Test failure"));

        // Act
        var result = await processor.ExecuteTaskAsync("failing_task", task);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("failing_task", result.Name);
        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task ProcessTasksStreamAsync_YieldsResultsAsCompleted()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; }
        };

        // Act
        var results = new List<string>();
        await foreach (var result in processor.ProcessTasksStreamAsync(tasks))
        {
            results.Add(result.Name);
        }

        // Assert
        Assert.AreEqual(2, results.Count);
        CollectionAssert.Contains(results, "task1");
        CollectionAssert.Contains(results, "task2");
    }

    [TestMethod]
    public void CurrentProgress_InitialState_ReturnsZeroProgress()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);

        // Act
        var progress = processor.CurrentProgress;

        // Assert
        Assert.AreEqual(0, progress.CompletedTasks);
        Assert.AreEqual(0, progress.TotalTasks);
        Assert.AreEqual(0.0, progress.CompletionPercentage);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithProgressReporting_UpdatesProgress()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        Assert.IsTrue(progressReports.Count > 0);
        var finalProgress = progressReports.Last();
        Assert.AreEqual(2, finalProgress.CompletedTasks);
        Assert.AreEqual(2, finalProgress.TotalTasks);
        Assert.AreEqual(1.0, finalProgress.CompletionPercentage);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithCancellation_CancelsGracefully()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var cts = new CancellationTokenSource();
        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(1000, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(2000, ct); return "result2"; }
        };

        // Act
        var processingTask = processor.ProcessTasksAsync(tasks, cancellationToken: cts.Token);
        await Task.Delay(100); // Let processing start
        cts.Cancel();

        // Assert
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await processingTask);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessTasksAsync_WithNullTasks_ThrowsArgumentNullException()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        // Act
        await processor.ProcessTasksAsync(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task ExecuteTaskAsync_WithNullOrEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();
        var task = Task.FromResult("test");

        // Act
        await processor.ExecuteTaskAsync(string.Empty, task);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ExecuteTaskAsync_WithNullTask_ThrowsArgumentNullException()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        // Act
        await processor.ExecuteTaskAsync<string>("test", null!);
    }

    [TestMethod]
    public void Dispose_CallsDisposeOnce()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act & Assert
        try
        {
            processor.Dispose();
            processor.Dispose(); // Second call should be safe
            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Dispose should not throw exception: {ex.Message}");
        }
    }

    [TestMethod]
    public async Task DisposeAsync_CallsDisposeAsyncOnce()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act & Assert
        await processor.DisposeAsync();
        await processor.DisposeAsync(); // Second call should be safe
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TaskResults_IsThreadSafe_ReturnsReadOnlyCollection()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);

        // Act
        var results = processor.TaskResults;

        // Assert
        Assert.IsNotNull(results);
        Assert.IsInstanceOfType(results, typeof(IReadOnlyCollection<TaskListProcessing.Interfaces.ITaskResult>));
    }

    [TestMethod]
    public void Telemetry_IsThreadSafe_ReturnsReadOnlyCollection()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);

        // Act
        var telemetry = processor.Telemetry;

        // Assert
        Assert.IsNotNull(telemetry);
        Assert.IsInstanceOfType(telemetry, typeof(IReadOnlyCollection<TaskListProcessing.Telemetry.TaskTelemetry>));
    }
}
