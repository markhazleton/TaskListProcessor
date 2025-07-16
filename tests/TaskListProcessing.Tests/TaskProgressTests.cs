using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Concurrent;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Tests;

/// <summary>
/// Tests for progress tracking functionality in TaskListProcessorEnhanced.
/// </summary>
[TestClass]
public class TaskProgressTests
{
    private Mock<ILogger<TaskListProcessorEnhanced>> _mockLogger = null!;
    private TaskListProcessorOptions _options = null!;
    private readonly List<TaskListProcessorEnhanced> _processors = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLogger = new Mock<ILogger<TaskListProcessorEnhanced>>();
        _options = new TaskListProcessorOptions
        {
            MaxConcurrentTasks = 2,
            EnableProgressReporting = true
        };
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Dispose all processors created during the test
        foreach (var processor in _processors)
        {
            processor.Dispose();
        }
        _processors.Clear();
    }

    private TaskListProcessorEnhanced CreateProcessor()
    {
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        _processors.Add(processor);
        return processor;
    }

    [TestMethod]
    public void TaskProgress_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var progress = new TaskProgress(5, 10, "test-task", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60), 0.8);

        // Assert
        Assert.AreEqual(5, progress.CompletedTasks);
        Assert.AreEqual(10, progress.TotalTasks);
        Assert.AreEqual("test-task", progress.CurrentTaskName);
        Assert.AreEqual(TimeSpan.FromSeconds(30), progress.ElapsedTime);
        Assert.AreEqual(TimeSpan.FromSeconds(60), progress.EstimatedTimeRemaining);
        Assert.AreEqual(0.8, progress.SuccessRate);
    }

    [TestMethod]
    public void TaskProgress_CompletionPercentage_CalculatesCorrectly()
    {
        // Arrange & Act
        var progress1 = new TaskProgress(5, 10);
        var progress2 = new TaskProgress(10, 10);
        var progress3 = new TaskProgress(0, 10);
        var progress4 = new TaskProgress(0, 0);

        // Assert
        Assert.AreEqual(0.5, progress1.CompletionPercentage);
        Assert.AreEqual(1.0, progress2.CompletionPercentage);
        Assert.AreEqual(0.0, progress3.CompletionPercentage);
        Assert.AreEqual(0.0, progress4.CompletionPercentage);
    }

    [TestMethod]
    public void TaskProgress_IsCompleted_ReturnsCorrectValue()
    {
        // Arrange & Act
        var progress1 = new TaskProgress(5, 10);
        var progress2 = new TaskProgress(10, 10);
        var progress3 = new TaskProgress(12, 10); // More than total

        // Assert
        Assert.IsFalse(progress1.IsCompleted);
        Assert.IsTrue(progress2.IsCompleted);
        Assert.IsTrue(progress3.IsCompleted);
    }

    [TestMethod]
    public void TaskProgress_RemainingTasks_CalculatesCorrectly()
    {
        // Arrange & Act
        var progress1 = new TaskProgress(3, 10);
        var progress2 = new TaskProgress(10, 10);
        var progress3 = new TaskProgress(12, 10); // More than total

        // Assert
        Assert.AreEqual(7, progress1.RemainingTasks);
        Assert.AreEqual(0, progress2.RemainingTasks);
        Assert.AreEqual(0, progress3.RemainingTasks);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithProgressCallback_ReportsProgress()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; },
            ["task3"] = async ct => { await Task.Delay(75, ct); return "result3"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        Assert.IsTrue(progressReports.Count > 0);

        // Check initial progress
        var initialProgress = progressReports.First();
        Assert.AreEqual(0, initialProgress.CompletedTasks);
        Assert.AreEqual(3, initialProgress.TotalTasks);
        Assert.AreEqual(0.0, initialProgress.CompletionPercentage);

        // Check final progress
        var finalProgress = progressReports.Last();
        Assert.AreEqual(3, finalProgress.CompletedTasks);
        Assert.AreEqual(3, finalProgress.TotalTasks);
        Assert.AreEqual(1.0, finalProgress.CompletionPercentage);
        Assert.IsTrue(finalProgress.IsCompleted);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_ProgressReporting_IncludesElapsedTime()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(100, ct); return "result1"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        var finalProgress = progressReports.Last();
        Assert.IsTrue(finalProgress.ElapsedTime > TimeSpan.Zero);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_ProgressReporting_IncludesCurrentTaskName()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["specific-task-name"] = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        var taskNameReports = progressReports.Where(p => p.CurrentTaskName == "specific-task-name").ToList();
        Assert.IsTrue(taskNameReports.Count > 0);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithMixedResults_ReportsSuccessRate()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["success1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["failure1"] = ct => Task.FromException<object?>(new Exception("Test failure")),
            ["success2"] = async ct => { await Task.Delay(75, ct); return "result2"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        var finalProgress = progressReports.Last();
        Assert.IsTrue(finalProgress.SuccessRate > 0.0);
        Assert.IsTrue(finalProgress.SuccessRate < 100.0); // Should be between 0 and 100 due to mixed results
    }

    [TestMethod]
    public void CurrentProgress_ThreadSafety_ReturnsConsistentValues()
    {
        // Arrange
        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        var progressValues = new ConcurrentBag<TaskProgress>();

        // Act - Access CurrentProgress from multiple threads
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                var progress = processor.CurrentProgress;
                if (progress != null)
                {
                    progressValues.Add(progress);
                }
            }
        })).ToArray();

        Task.WaitAll(tasks);

        // Assert - All values should be consistent (initial state)
        Assert.IsTrue(progressValues.Count > 0);
        Assert.IsTrue(progressValues.All(p => p != null));
        Assert.IsTrue(progressValues.All(p => p.CompletedTasks == 0));
        Assert.IsTrue(progressValues.All(p => p.TotalTasks == 0));
        Assert.IsTrue(progressValues.All(p => p.CompletionPercentage == 0.0));
    }

    [TestMethod]
    public async Task ProcessTasksAsync_WithZeroTasks_HandlesProgressCorrectly()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var emptyTasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        // Act
        await processor.ProcessTasksAsync(emptyTasks, progressReporter);

        // Give a small delay to ensure all progress reports are processed
        await Task.Delay(50);

        // Assert
        Assert.IsTrue(progressReports.Count > 0);
        var finalProgress = progressReports.Last();
        Assert.AreEqual(0, finalProgress.CompletedTasks);
        Assert.AreEqual(0, finalProgress.TotalTasks);
        Assert.AreEqual(0.0, finalProgress.CompletionPercentage);
        Assert.IsTrue(finalProgress.IsCompleted);
    }

    [TestMethod]
    public async Task ProcessTasksAsync_ProgressReporting_EstimatesTimeRemaining()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
            ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; },
            ["task3"] = async ct => { await Task.Delay(75, ct); return "result3"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        var progressWithEstimate = progressReports.Where(p => p.EstimatedTimeRemaining.HasValue).ToList();
        Assert.IsTrue(progressWithEstimate.Count > 0);

        // Estimated time should be reasonable (not negative and not extremely large)
        foreach (var progress in progressWithEstimate)
        {
            Assert.IsTrue(progress.EstimatedTimeRemaining!.Value >= TimeSpan.Zero);
            Assert.IsTrue(progress.EstimatedTimeRemaining!.Value <= TimeSpan.FromMinutes(5));
        }
    }

    [TestMethod]
    public async Task ProcessTasksAsync_ProgressReporting_HandlesLongRunningTasks()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
        await processor.InitializeAsync();

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["long-task"] = async ct => { await Task.Delay(200, ct); return "result"; }
        };

        // Act
        await processor.ProcessTasksAsync(tasks, progressReporter);

        // Assert
        var finalProgress = progressReports.Last();
        Assert.IsTrue(finalProgress.ElapsedTime >= TimeSpan.FromMilliseconds(200));
        Assert.AreEqual(1, finalProgress.CompletedTasks);
        Assert.AreEqual(1, finalProgress.TotalTasks);
        Assert.IsTrue(finalProgress.IsCompleted);
    }
}
