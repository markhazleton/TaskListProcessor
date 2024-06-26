using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskListProcessing.Tests;


[TestClass]
public class TaskListProcessorTests
{
    private TaskListProcessor? _taskListProcessor;

    [TestInitialize]
    public void Setup()
    {
        _taskListProcessor = new TaskListProcessor();
    }

    [TestMethod]
    public async Task GetTaskResultAsync_TaskCompletes_SuccessTelemetryRecorded()
    {
        if (_taskListProcessor == null) throw new NullReferenceException(nameof(_taskListProcessor));
        // Arrange
        var taskName = "SuccessfulTask";
        var task = Task.FromResult<object>(new object());

        // Act
        await _taskListProcessor.GetTaskResultAsync(taskName, task);

        // Assert
        Assert.IsTrue(_taskListProcessor.Telemetry.Count > 0, "Telemetry should be recorded for a successful task.");
    }

    [TestMethod]
    public async Task GetTaskResultAsync_TaskFails_FailureTelemetryRecorded()
    {
        if (_taskListProcessor == null) throw new NullReferenceException(nameof(_taskListProcessor));
        // Arrange
        var taskName = "FailedTask";
        var task = Task.FromException<object>(new Exception("Test exception"));

        // Act
        await _taskListProcessor.GetTaskResultAsync(taskName, task);

        // Assert
        Assert.IsTrue(_taskListProcessor.Telemetry.Count > 0, "Telemetry should be recorded for a failed task.");
        Assert.IsTrue(_taskListProcessor.TaskResults.Exists(tr => tr.Data == null && tr.Name == taskName), "A task result indicating failure should be recorded.");
    }
}
