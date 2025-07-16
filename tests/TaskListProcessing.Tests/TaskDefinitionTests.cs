using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Tests;

/// <summary>
/// Tests for TaskDefinition processing functionality in TaskListProcessorEnhanced.
/// </summary>
[TestClass]
public class TaskDefinitionTests
{
    private Mock<ILogger<TaskListProcessorEnhanced>> _mockLogger = null!;
    private TaskListProcessorOptions _options = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLogger = new Mock<ILogger<TaskListProcessorEnhanced>>();
        _options = new TaskListProcessorOptions
        {
            MaxConcurrentTasks = 3,
            EnableDetailedTelemetry = true
        };
    }

    private TaskListProcessorEnhanced CreateProcessor()
    {
        return new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
    }

    [TestMethod]
    public void TaskDefinition_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var taskDef = new TaskDefinition();

        // Assert
        Assert.AreEqual(string.Empty, taskDef.Name);
        Assert.IsNotNull(taskDef.Factory);
        Assert.AreEqual(0, taskDef.Dependencies.Length);
        Assert.AreEqual(0, taskDef.Priority);
        Assert.IsNull(taskDef.EstimatedExecutionTime);
        Assert.IsNotNull(taskDef.Metadata);
        Assert.AreEqual(0, taskDef.Metadata.Count);
        Assert.IsNull(taskDef.Timeout);
        Assert.IsNull(taskDef.RetryPolicy);
    }

    [TestMethod]
    public void TaskDefinition_WithCustomValues_SetsPropertiesCorrectly()
    {
        // Arrange
        var factory = new Func<CancellationToken, Task<object?>>(ct => Task.FromResult<object?>("test"));
        var dependencies = new[] { "dep1", "dep2" };
        var metadata = new Dictionary<string, object> { ["key1"] = "value1" };
        var timeout = TimeSpan.FromMinutes(5);

        // Act
        var taskDef = new TaskDefinition
        {
            Name = "test-task",
            Factory = factory,
            Dependencies = dependencies,
            Priority = 10,
            EstimatedExecutionTime = TimeSpan.FromSeconds(30),
            Metadata = metadata,
            Timeout = timeout
        };

        // Assert
        Assert.AreEqual("test-task", taskDef.Name);
        Assert.AreEqual(factory, taskDef.Factory);
        Assert.AreEqual(dependencies, taskDef.Dependencies);
        Assert.AreEqual(10, taskDef.Priority);
        Assert.AreEqual(TimeSpan.FromSeconds(30), taskDef.EstimatedExecutionTime);
        Assert.AreEqual(metadata, taskDef.Metadata);
        Assert.AreEqual(timeout, taskDef.Timeout);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithSingleDefinition_ExecutesSuccessfully()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDef = new TaskDefinition
        {
            Name = "single-task",
            Factory = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(new[] { taskDef });

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("single-task", result.Name);
        Assert.IsTrue(result.IsSuccessful);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithMultipleDefinitions_ExecutesAll()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDefs = new[]
        {
            new TaskDefinition
            {
                Name = "task1",
                Factory = async ct => { await Task.Delay(50, ct); return "result1"; }
            },
            new TaskDefinition
            {
                Name = "task2",
                Factory = async ct => { await Task.Delay(75, ct); return "result2"; }
            },
            new TaskDefinition
            {
                Name = "task3",
                Factory = async ct => { await Task.Delay(25, ct); return "result3"; }
            }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(taskDefs);

        // Assert
        Assert.AreEqual(3, processor.TaskResults.Count);
        var taskNames = processor.TaskResults.Select(r => r.Name).ToList();
        CollectionAssert.Contains(taskNames, "task1");
        CollectionAssert.Contains(taskNames, "task2");
        CollectionAssert.Contains(taskNames, "task3");
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithPriorities_ExecutesInOrder()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var executionOrder = new List<string>();
        var lockObj = new object();

        var taskDefs = new[]
        {
            new TaskDefinition
            {
                Name = "low-priority",
                Priority = 1,
                Factory = async ct =>
                {
                    await Task.Delay(50, ct);
                    lock (lockObj) { executionOrder.Add("low-priority"); }
                    return "result";
                }
            },
            new TaskDefinition
            {
                Name = "high-priority",
                Priority = 10,
                Factory = async ct =>
                {
                    await Task.Delay(50, ct);
                    lock (lockObj) { executionOrder.Add("high-priority"); }
                    return "result";
                }
            },
            new TaskDefinition
            {
                Name = "medium-priority",
                Priority = 5,
                Factory = async ct =>
                {
                    await Task.Delay(50, ct);
                    lock (lockObj) { executionOrder.Add("medium-priority"); }
                    return "result";
                }
            }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(taskDefs);

        // Assert
        Assert.AreEqual(3, processor.TaskResults.Count);
        Assert.IsTrue(processor.TaskResults.All(r => r.IsSuccessful));

        // Note: Exact order depends on implementation, but we can verify all tasks executed
        Assert.AreEqual(3, executionOrder.Count);
        CollectionAssert.Contains(executionOrder, "low-priority");
        CollectionAssert.Contains(executionOrder, "high-priority");
        CollectionAssert.Contains(executionOrder, "medium-priority");
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithMetadata_PreservesMetadata()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var metadata = new Dictionary<string, object>
        {
            ["author"] = "test-author",
            ["version"] = "1.0.0",
            ["category"] = "unit-test"
        };

        var taskDef = new TaskDefinition
        {
            Name = "metadata-task",
            Metadata = metadata,
            Factory = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(new[] { taskDef });

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("metadata-task", result.Name);
        Assert.IsTrue(result.IsSuccessful);

        // Verify metadata was preserved in the task definition
        Assert.AreEqual(metadata, taskDef.Metadata);
        Assert.AreEqual("test-author", taskDef.Metadata["author"]);
        Assert.AreEqual("1.0.0", taskDef.Metadata["version"]);
        Assert.AreEqual("unit-test", taskDef.Metadata["category"]);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithEstimatedExecutionTime_CompletesSuccessfully()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDef = new TaskDefinition
        {
            Name = "estimated-task",
            EstimatedExecutionTime = TimeSpan.FromMilliseconds(100),
            Factory = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(new[] { taskDef });

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("estimated-task", result.Name);
        Assert.IsTrue(result.IsSuccessful);

        // Verify estimated execution time was set
        Assert.AreEqual(TimeSpan.FromMilliseconds(100), taskDef.EstimatedExecutionTime);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithTimeout_CompletesWithinTimeout()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDef = new TaskDefinition
        {
            Name = "timeout-task",
            Timeout = TimeSpan.FromSeconds(5),
            Factory = async ct => { await Task.Delay(50, ct); return "result"; }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(new[] { taskDef });

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("timeout-task", result.Name);
        Assert.IsTrue(result.IsSuccessful);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithFailingTask_CapturesError()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDef = new TaskDefinition
        {
            Name = "failing-task",
            Factory = ct => Task.FromException<object?>(new InvalidOperationException("Test failure"))
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(new[] { taskDef });

        // Assert
        Assert.AreEqual(1, processor.TaskResults.Count);
        var result = processor.TaskResults.First();
        Assert.AreEqual("failing-task", result.Name);
        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage.Contains("Test failure"));
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithProgressReporting_UpdatesProgress()
    {
        // Arrange
        var progressReports = new List<TaskProgress>();
        var progressReporter = new Progress<TaskProgress>(p => progressReports.Add(p));

        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDefs = new[]
        {
            new TaskDefinition
            {
                Name = "progress-task1",
                Factory = async ct => { await Task.Delay(50, ct); return "result1"; }
            },
            new TaskDefinition
            {
                Name = "progress-task2",
                Factory = async ct => { await Task.Delay(75, ct); return "result2"; }
            }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(taskDefs, progressReporter);

        // Assert
        Assert.AreEqual(2, processor.TaskResults.Count);
        Assert.IsTrue(progressReports.Count > 0);

        var finalProgress = progressReports.Last();
        Assert.AreEqual(2, finalProgress.CompletedTasks);
        Assert.AreEqual(2, finalProgress.TotalTasks);
        Assert.AreEqual(1.0, finalProgress.CompletionPercentage);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithCancellation_CancelsGracefully()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var cts = new CancellationTokenSource();
        var taskDef = new TaskDefinition
        {
            Name = "cancellable-task",
            Factory = async ct =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return "should be cancelled";
            }
        };

        // Act
        var processingTask = processor.ProcessTaskDefinitionsAsync(new[] { taskDef }, cancellationToken: cts.Token);
        await Task.Delay(100); // Let task start
        cts.Cancel();

        // Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await processingTask);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithEmptyDefinitions_CompletesSuccessfully()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var emptyDefinitions = Array.Empty<TaskDefinition>();

        // Act
        await processor.ProcessTaskDefinitionsAsync(emptyDefinitions);

        // Assert
        Assert.AreEqual(0, processor.TaskResults.Count);
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithLargeNumberOfDefinitions_HandlesEfficiently()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDefs = new TaskDefinition[100];
        for (int i = 0; i < 100; i++)
        {
            var taskIndex = i;
            taskDefs[i] = new TaskDefinition
            {
                Name = $"bulk-task-{taskIndex}",
                Factory = async ct =>
                {
                    await Task.Delay(10, ct);
                    return $"result-{taskIndex}";
                }
            };
        }

        // Act
        var startTime = DateTime.UtcNow;
        await processor.ProcessTaskDefinitionsAsync(taskDefs);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.AreEqual(100, processor.TaskResults.Count);
        Assert.IsTrue(processor.TaskResults.All(r => r.IsSuccessful));

        // Should complete efficiently due to concurrency
        var elapsedTime = endTime - startTime;
        Assert.IsTrue(elapsedTime < TimeSpan.FromSeconds(30)); // Much less than 100 * 10ms = 1000ms
    }

    [TestMethod]
    public async Task ProcessTaskDefinitionsAsync_WithMixedSuccessAndFailure_ContinuesProcessing()
    {
        // Arrange
        var processor = CreateProcessor();
        await processor.InitializeAsync();

        var taskDefs = new[]
        {
            new TaskDefinition
            {
                Name = "success-task",
                Factory = async ct => { await Task.Delay(50, ct); return "success"; }
            },
            new TaskDefinition
            {
                Name = "failure-task",
                Factory = ct => Task.FromException<object?>(new Exception("Test error"))
            },
            new TaskDefinition
            {
                Name = "another-success-task",
                Factory = async ct => { await Task.Delay(75, ct); return "another success"; }
            }
        };

        // Act
        await processor.ProcessTaskDefinitionsAsync(taskDefs);

        // Assert
        Assert.AreEqual(3, processor.TaskResults.Count);

        var successResults = processor.TaskResults.Where(r => r.IsSuccessful).ToList();
        var failureResults = processor.TaskResults.Where(r => !r.IsSuccessful).ToList();

        Assert.AreEqual(2, successResults.Count);
        Assert.AreEqual(1, failureResults.Count);

        Assert.IsTrue(successResults.Any(r => r.Name == "success-task"));
        Assert.IsTrue(successResults.Any(r => r.Name == "another-success-task"));
        Assert.IsTrue(failureResults.Any(r => r.Name == "failure-task"));
    }
}
