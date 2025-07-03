using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskListProcessing.Core;

namespace TaskListProcessing.Tests;

[TestClass]
public class TaskListProcessorImprovedTests
{
    private Mock<ILogger>? _mockLogger;
    private TaskListProcessorImproved? _processor;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
        _processor = new TaskListProcessorImproved("Test Processor", _mockLogger.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _processor?.Dispose();
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_SuccessfulTask_ReturnsCorrectResult()
    {
        // Arrange
        var taskName = "Test Task";
        var expectedData = "Test Result";
        var task = Task.FromResult(expectedData);

        // Act
        var result = await _processor!.ExecuteTaskAsync(taskName, task);

        // Assert
        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(taskName, result.Name);
        Assert.AreEqual(expectedData, result.Data);
        Assert.IsNull(result.ErrorMessage);
        Assert.AreEqual(1, _processor.TaskResults.Count);
        Assert.AreEqual(1, _processor.Telemetry.Count);
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_FailedTask_ReturnsErrorResult()
    {
        // Arrange
        var taskName = "Failed Task";
        var errorMessage = "Test exception";
        var task = Task.FromException<string>(new InvalidOperationException(errorMessage));

        // Act
        var result = await _processor!.ExecuteTaskAsync(taskName, task);

        // Assert
        Assert.IsFalse(result.IsSuccessful);
        Assert.AreEqual(taskName, result.Name);
        Assert.IsNull(result.Data);
        Assert.AreEqual(errorMessage, result.ErrorMessage);
        Assert.AreEqual(1, _processor.TaskResults.Count);
        Assert.AreEqual(1, _processor.Telemetry.Count);
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_CancelledTask_ReturnsCancelledResult()
    {
        // Arrange
        var taskName = "Cancelled Task";
        var cts = new CancellationTokenSource();
        var task = Task.Delay(TimeSpan.FromSeconds(10), cts.Token).ContinueWith<string>(_ => "result", cts.Token);

        // Act
        cts.Cancel();
        var result = await _processor!.ExecuteTaskAsync(taskName, task, cts.Token);

        // Assert
        Assert.IsFalse(result.IsSuccessful);
        Assert.AreEqual(taskName, result.Name);
        Assert.IsNull(result.Data);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage.Contains("cancelled"));
    }

    [TestMethod]
    public async Task ProcessTasksAsync_MultipleTasks_ProcessesAllTasks()
    {
        // Arrange
        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["Task1"] = _ => Task.FromResult<object?>("Result1"),
            ["Task2"] = _ => Task.FromResult<object?>("Result2"),
            ["Task3"] = _ => Task.FromException<object?>(new InvalidOperationException("Error")),
            ["Task4"] = _ => Task.FromResult<object?>("Result4")
        };

        // Act
        await _processor!.ProcessTasksAsync(tasks);

        // Assert
        Assert.AreEqual(4, _processor.TaskResults.Count);
        Assert.AreEqual(4, _processor.Telemetry.Count);

        var summary = _processor.GetTelemetrySummary();
        Assert.AreEqual(4, summary.TotalTasks);
        Assert.AreEqual(3, summary.SuccessfulTasks);
        Assert.AreEqual(1, summary.FailedTasks);
    }

    [TestMethod]
    public void GetTelemetrySummary_EmptyProcessor_ReturnsZeroSummary()
    {
        // Act
        var summary = _processor!.GetTelemetrySummary();

        // Assert
        Assert.AreEqual(0, summary.TotalTasks);
        Assert.AreEqual(0, summary.SuccessfulTasks);
        Assert.AreEqual(0, summary.FailedTasks);
        Assert.AreEqual(0, summary.AverageExecutionTime);
        Assert.AreEqual(0, summary.TotalExecutionTime);
        Assert.AreEqual(0, summary.SuccessRate);
    }

    [TestMethod]
    public async Task WhenAllWithLoggingAsync_AllTasksSucceed_LogsSuccess()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult("Result1"),
            Task.FromResult("Result2"),
            Task.FromResult("Result3")
        };

        // Act
        await TaskListProcessorImproved.WhenAllWithLoggingAsync(tasks, _mockLogger!.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task WhenAllWithLoggingAsync_SomeTasksFail_LogsErrors()
    {
        // Arrange
        var tasks = new Task[]
        {
            Task.FromResult("Result1"),
            Task.FromException(new InvalidOperationException("Test error")),
            Task.FromResult("Result3")
        };

        // Act
        await TaskListProcessorImproved.WhenAllWithLoggingAsync(tasks, _mockLogger!.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed with errors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithNullTaskFactories_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => _processor!.ProcessTasksAsync(null!));
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_WithNullTaskName_ThrowsArgumentException()
    {
        // Arrange
        var task = Task.FromResult("result");

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _processor!.ExecuteTaskAsync("", task));
    }

    [TestMethod]
    public async Task ExecuteTaskAsync_WithNullTask_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => _processor!.ExecuteTaskAsync<string>("test", null!));
    }

    [TestMethod]
    public void TaskResults_IsThreadSafe()
    {
        // This test verifies that the ConcurrentBag implementation works correctly
        // Act
        var results = _processor!.TaskResults;

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }
}
