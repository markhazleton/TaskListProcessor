using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskListProcessing.Tests;

[TestClass]
public class TaskListProcessorGenericTests
{
    private TaskListProcessorGeneric? _processor;
    private Mock<ILogger>? _mockLogger;

    [TestInitialize]
    public void Setup()
    {
        _processor = new TaskListProcessorGeneric();
        _mockLogger = new Mock<ILogger>();
    }

    #region Basic Functionality Tests

    [TestMethod]
    public async Task GetTaskResultAsync_StringTask_Success_RecordsTelemetryAndResult()
    {
        // Arrange
        var taskName = "StringTask";
        var expectedResult = "Test Result";
        var task = Task.FromResult(expectedResult);

        // Act
        await _processor!.GetTaskResultAsync(taskName, task);

        // Assert
        Assert.AreEqual(1, _processor.Telemetry.Count, "Should record one telemetry entry");
        Assert.AreEqual(1, _processor.TaskResults.Count, "Should record one task result");

        var telemetry = _processor.Telemetry.First();
        Assert.IsTrue(telemetry.Contains(taskName), "Telemetry should contain task name");
        Assert.IsTrue(telemetry.Contains("Task completed in"), "Telemetry should contain completion message");
        Assert.IsFalse(telemetry.Contains("ERROR"), "Successful task should not contain ERROR in telemetry");

        var result = _processor.TaskResults.First();
        Assert.AreEqual(taskName, result.Name, "Result name should match task name");

        if (result is TaskResult<string> stringResult)
        {
            Assert.AreEqual(expectedResult, stringResult.Data, "Result data should match expected value");
        }
        else
        {
            Assert.Fail("Result should be of type TaskResult<string>");
        }
    }

    [TestMethod]
    public async Task GetTaskResultAsync_ComplexObjectTask_Success_RecordsTelemetryAndResult()
    {
        // Arrange
        var taskName = "ComplexObjectTask";
        var expectedResult = new TestData { Id = 123, Name = "Test Name", Value = 45.67 };
        var task = Task.FromResult(expectedResult);

        // Act
        await _processor!.GetTaskResultAsync(taskName, task);

        // Assert
        Assert.AreEqual(1, _processor.Telemetry.Count, "Should record one telemetry entry");
        Assert.AreEqual(1, _processor.TaskResults.Count, "Should record one task result");

        var result = _processor.TaskResults.First();
        if (result is TaskResult<TestData> testDataResult)
        {
            Assert.AreEqual(expectedResult.Id, testDataResult.Data!.Id);
            Assert.AreEqual(expectedResult.Name, testDataResult.Data.Name);
            Assert.AreEqual(expectedResult.Value, testDataResult.Data.Value);
        }
        else
        {
            Assert.Fail("Result should be of type TaskResult<TestData>");
        }
    }

    [TestMethod]
    public async Task GetTaskResultAsync_TaskFails_RecordsErrorTelemetryAndNullResult()
    {
        // Arrange
        var taskName = "FailedTask";
        var errorMessage = "Test exception message";
        var task = Task.FromException<string>(new Exception(errorMessage));

        // Act
        await _processor!.GetTaskResultAsync(taskName, task);

        // Assert
        Assert.AreEqual(1, _processor.Telemetry.Count, "Should record one telemetry entry");
        Assert.AreEqual(1, _processor.TaskResults.Count, "Should record one task result");

        var telemetry = _processor.Telemetry.First();
        Assert.IsTrue(telemetry.Contains(taskName), "Telemetry should contain task name");
        Assert.IsTrue(telemetry.Contains("ERROR Exception"), "Failed task should contain ERROR in telemetry");
        Assert.IsTrue(telemetry.Contains(errorMessage), "Telemetry should contain error message");

        var result = _processor.TaskResults.First();
        Assert.AreEqual(taskName, result.Name, "Result name should match task name");

        if (result is TaskResult<string> stringResult)
        {
            Assert.IsNull(stringResult.Data, "Failed task should have null data");
        }
        else
        {
            Assert.Fail("Result should be of type TaskResult<string>");
        }
    }

    #endregion

    #region Multiple Task Types Tests

    [TestMethod]
    public async Task GetTaskResultAsync_MultipleDifferentTypes_AllRecordedCorrectly()
    {
        // Arrange
        var stringTask = Task.FromResult("String Result");
        var intTask = Task.FromResult(new TestNumber { Value = 42 });
        var objectTask = Task.FromResult(new TestData { Id = 1, Name = "Test", Value = 3.14 });

        // Act
        await _processor!.GetTaskResultAsync("StringTask", stringTask);
        await _processor.GetTaskResultAsync("IntTask", intTask);
        await _processor.GetTaskResultAsync("ObjectTask", objectTask);

        // Assert
        Assert.AreEqual(3, _processor.Telemetry.Count, "Should record three telemetry entries");
        Assert.AreEqual(3, _processor.TaskResults.Count, "Should record three task results");

        // Verify string result
        var stringResult = _processor.TaskResults.OfType<TaskResult<string>>().FirstOrDefault();
        Assert.IsNotNull(stringResult, "Should have string result");
        Assert.AreEqual("String Result", stringResult.Data);

        // Verify int result
        var intResult = _processor.TaskResults.OfType<TaskResult<TestNumber>>().FirstOrDefault();
        Assert.IsNotNull(intResult, "Should have int result");
        Assert.AreEqual(42, intResult.Data!.Value);

        // Verify object result
        var objectResult = _processor.TaskResults.OfType<TaskResult<TestData>>().FirstOrDefault();
        Assert.IsNotNull(objectResult, "Should have object result");
        Assert.AreEqual("Test", objectResult.Data!.Name);
    }

    [TestMethod]
    public async Task GetTaskResultAsync_MixedSuccessAndFailure_RecordsAllCorrectly()
    {
        // Arrange
        var successTask = Task.FromResult("Success");
        var failureTask = Task.FromException<string>(new Exception("Failure"));
        var anotherSuccessTask = Task.FromResult(new TestData { Id = 1, Name = "Success2", Value = 1.0 });

        // Act
        await _processor!.GetTaskResultAsync("SuccessTask", successTask);
        await _processor.GetTaskResultAsync("FailureTask", failureTask);
        await _processor.GetTaskResultAsync("AnotherSuccessTask", anotherSuccessTask);

        // Assert
        Assert.AreEqual(3, _processor.Telemetry.Count, "Should record three telemetry entries");
        Assert.AreEqual(3, _processor.TaskResults.Count, "Should record three task results");

        var successTelemetry = _processor.Telemetry.Where(t => t.StartsWith("SuccessTask:") && !t.Contains("ERROR")).ToList();
        var failureTelemetry = _processor.Telemetry.Where(t => t.StartsWith("FailureTask:") && t.Contains("ERROR")).ToList();
        var anotherSuccessTelemetry = _processor.Telemetry.Where(t => t.StartsWith("AnotherSuccessTask:") && !t.Contains("ERROR")).ToList();

        Assert.AreEqual(1, successTelemetry.Count, "Should have one success telemetry for SuccessTask");
        Assert.AreEqual(1, failureTelemetry.Count, "Should have one failure telemetry for FailureTask");
        Assert.AreEqual(1, anotherSuccessTelemetry.Count, "Should have one success telemetry for AnotherSuccessTask");
    }

    #endregion

    #region WhenAllWithLoggingAsync Tests

    [TestMethod]
    public async Task WhenAllWithLoggingAsync_AllTasksSucceed_NoErrorsLogged()
    {
        // Arrange
        var tasks = new List<Task>
        {
            _processor!.GetTaskResultAsync("Task1", Task.FromResult("Result1")),
            _processor.GetTaskResultAsync("Task2", Task.FromResult("Result2")),
            _processor.GetTaskResultAsync("Task3", Task.FromResult("Result3"))
        };

        // Act
        await TaskListProcessorGeneric.WhenAllWithLoggingAsync(tasks, _mockLogger!.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "No errors should be logged when all tasks succeed");
    }

    [TestMethod]
    public async Task WhenAllWithLoggingAsync_SomeTasksFail_ErrorsLoggedButDoesNotThrow()
    {
        // Arrange - Create tasks that will actually throw when awaited by Task.WhenAll
        var tasks = new List<Task>
        {
            Task.FromResult("Result1"),
            Task.FromException(new Exception("Error")),
            Task.FromResult("Result3")
        };

        // Act & Assert (should not throw)
        await TaskListProcessorGeneric.WhenAllWithLoggingAsync(tasks, _mockLogger!.Object);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TLP: An error occurred while executing one or more tasks.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Error should be logged when tasks fail");
    }

    [TestMethod]
    public async Task WhenAllWithLoggingAsync_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var tasks = new List<Task> { Task.CompletedTask };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            async () => await TaskListProcessorGeneric.WhenAllWithLoggingAsync(tasks, null!));
    }

    #endregion

    #region Performance and Timing Tests

    [TestMethod]
    public async Task GetTaskResultAsync_DelayedTask_RecordsElapsedTime()
    {
        // Arrange
        var taskName = "DelayedTask";
        var delayMs = 100;
        var task = CreateDelayedTask("Result", delayMs);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _processor!.GetTaskResultAsync(taskName, task);
        stopwatch.Stop();

        // Assert
        var telemetry = _processor.Telemetry.First();
        Assert.IsTrue(telemetry.Contains("ms"), "Telemetry should contain milliseconds");

        // Extract the milliseconds from telemetry (rough validation)
        var telemetryMs = ExtractMillisecondsFromTelemetry(telemetry);
        Assert.IsTrue(telemetryMs >= delayMs - 50, $"Recorded time should be at least close to delay time. Expected: ~{delayMs}ms, Got: {telemetryMs}ms");
    }

    [TestMethod]
    public async Task GetTaskResultAsync_ConcurrentTasks_AllCompletedIndependently()
    {
        // Arrange
        var tasks = new List<Task>
        {
            _processor!.GetTaskResultAsync("Concurrent1", CreateDelayedTask("Result1", 50)),
            _processor.GetTaskResultAsync("Concurrent2", CreateDelayedTask("Result2", 100)),
            _processor.GetTaskResultAsync("Concurrent3", CreateDelayedTask("Result3", 75))
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(3, _processor.TaskResults.Count, "All three tasks should complete");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200, "Concurrent execution should be faster than sequential");

        // Verify all results are present
        var results = _processor.TaskResults.OfType<TaskResult<string>>().ToList();
        Assert.AreEqual(3, results.Count, "Should have 3 string results");
        Assert.IsTrue(results.Any(r => r.Data == "Result1"), "Should contain Result1");
        Assert.IsTrue(results.Any(r => r.Data == "Result2"), "Should contain Result2");
        Assert.IsTrue(results.Any(r => r.Data == "Result3"), "Should contain Result3");
    }

    #endregion

    #region Property Tests

    [TestMethod]
    public void TaskListName_CanSetAndGet()
    {
        // Arrange
        var expectedName = "Test Task List";

        // Act
        _processor!.TaskListName = expectedName;

        // Assert
        Assert.AreEqual(expectedName, _processor.TaskListName);
    }

    [TestMethod]
    public void TaskResults_InitializedAsEmptyCollection()
    {
        // Arrange & Act
        var newProcessor = new TaskListProcessorGeneric();

        // Assert
        Assert.IsNotNull(newProcessor.TaskResults);
        Assert.AreEqual(0, newProcessor.TaskResults.Count);
    }

    [TestMethod]
    public void Telemetry_InitializedAsEmptyList()
    {
        // Arrange & Act
        var newProcessor = new TaskListProcessorGeneric();

        // Assert
        Assert.IsNotNull(newProcessor.Telemetry);
        Assert.AreEqual(0, newProcessor.Telemetry.Count);
    }

    #endregion

    #region Type Casting and Interface Tests

    [TestMethod]
    public async Task TaskResults_ImplementsITaskResult_CanAccessName()
    {
        // Arrange
        var task1 = Task.FromResult("String result");
        var task2 = Task.FromResult(new TestData { Id = 1, Name = "Test", Value = 1.0 });

        // Act
        await _processor!.GetTaskResultAsync("StringTask", task1);
        await _processor.GetTaskResultAsync("ObjectTask", task2);

        // Assert
        foreach (ITaskResult result in _processor.TaskResults)
        {
            Assert.IsNotNull(result.Name, "Name should be accessible via interface");
            Assert.IsTrue(result.Name.Contains("Task"), "Name should contain 'Task'");
        }
    }

    [TestMethod]
    public async Task TaskResults_CanFilterByType()
    {
        // Arrange
        await _processor!.GetTaskResultAsync("String1", Task.FromResult("Result1"));
        await _processor.GetTaskResultAsync("String2", Task.FromResult("Result2"));
        await _processor.GetTaskResultAsync("Object1", Task.FromResult(new TestData { Id = 1, Name = "Test", Value = 1.0 }));

        // Act
        var stringResults = _processor.TaskResults.OfType<TaskResult<string>>().ToList();
        var objectResults = _processor.TaskResults.OfType<TaskResult<TestData>>().ToList();

        // Assert
        Assert.AreEqual(2, stringResults.Count, "Should have 2 string results");
        Assert.AreEqual(1, objectResults.Count, "Should have 1 object result");
    }

    #endregion

    #region Helper Methods and Test Classes

    private static async Task<string> CreateDelayedTask(string result, int delayMs)
    {
        await Task.Delay(delayMs);
        return result;
    }

    private static long ExtractMillisecondsFromTelemetry(string telemetry)
    {
        // Extract number before " ms" in the telemetry string
        var msIndex = telemetry.IndexOf(" ms");
        if (msIndex == -1) return 0;

        var startIndex = telemetry.LastIndexOf(' ', msIndex - 1) + 1;
        var msString = telemetry.Substring(startIndex, msIndex - startIndex).Replace(",", "");

        return long.TryParse(msString, out var ms) ? ms : 0;
    }

    // Test data classes
    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class TestNumber
    {
        public int Value { get; set; }
    }

    #endregion
}