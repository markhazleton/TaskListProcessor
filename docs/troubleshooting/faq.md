# Frequently Asked Questions (FAQ)

Quick answers to common questions about TaskListProcessor. Can't find your answer? [Ask in GitHub Discussions](https://github.com/markhazleton/TaskListProcessor/discussions)!

---

## Table of Contents

### Getting Started
- [What is TaskListProcessor?](#what-is-tasklistprocessor)
- [When should I use TaskListProcessor?](#when-should-i-use-tasklistprocessor)
- [What are the prerequisites?](#what-are-the-prerequisites)
- [How do I install TaskListProcessor?](#how-do-i-install-tasklistprocessor)

### Basic Usage
- [How do I create my first processor?](#how-do-i-create-my-first-processor)
- [How do I define tasks?](#how-do-i-define-tasks)
- [How do I execute tasks?](#how-do-i-execute-tasks)
- [How do I access results?](#how-do-i-access-results)

### Error Handling
- [How does error handling work?](#how-does-error-handling-work)
- [How do I handle individual task failures?](#how-do-i-handle-individual-task-failures)
- [Will one failed task stop all others?](#will-one-failed-task-stop-all-others)
- [How do I categorize errors?](#how-do-i-categorize-errors)

### Performance
- [How many tasks can run concurrently?](#how-many-tasks-can-run-concurrently)
- [How do I set concurrency limits?](#how-do-i-set-concurrency-limits)
- [What's the performance overhead?](#whats-the-performance-overhead)
- [How do I optimize for throughput?](#how-do-i-optimize-for-throughput)

### Advanced Features
- [How do I use dependency injection?](#how-do-i-use-dependency-injection)
- [How do task dependencies work?](#how-do-task-dependencies-work)
- [What is the circuit breaker pattern?](#what-is-the-circuit-breaker-pattern)
- [How do I use streaming results?](#how-do-i-use-streaming-results)
- [How do I implement progress reporting?](#how-do-i-implement-progress-reporting)

### Telemetry & Monitoring
- [What telemetry is collected?](#what-telemetry-is-collected)
- [How do I access telemetry data?](#how-do-i-access-telemetry-data)
- [Can I integrate with OpenTelemetry?](#can-i-integrate-with-opentelemetry)
- [How do I implement health checks?](#how-do-i-implement-health-checks)

### Troubleshooting
- [Why are my tasks not running concurrently?](#why-are-my-tasks-not-running-concurrently)
- [Why is my application hanging?](#why-is-my-application-hanging)
- [How do I debug task execution?](#how-do-i-debug-task-execution)
- [Why am I getting OutOfMemoryException?](#why-am-i-getting-outofmemoryexception)

### Best Practices
- [Should I reuse processor instances?](#should-i-reuse-processor-instances)
- [How do I handle long-running tasks?](#how-do-i-handle-long-running-tasks)
- [What's the best way to handle timeouts?](#whats-the-best-way-to-handle-timeouts)
- [How do I test code using TaskListProcessor?](#how-do-i-test-code-using-tasklistprocessor)

---

## Getting Started

### What is TaskListProcessor?

TaskListProcessor is a production-ready .NET 10.0 library for orchestrating asynchronous operations with built-in support for:
- Concurrent task execution
- Automatic error handling and isolation
- Circuit breaker pattern
- Comprehensive telemetry
- Dependency injection
- Task dependencies and scheduling

**Think of it as**: A robust framework for managing multiple async operations that would be tedious and error-prone to code manually.

### When should I use TaskListProcessor?

Use TaskListProcessor when you need to:

✅ **Good Use Cases**:
- Execute multiple independent async operations concurrently
- Aggregate data from multiple APIs or services
- Process batches of data with parallelism
- Coordinate microservice calls
- Build ETL pipelines
- Implement resilient distributed operations

❌ **Not Ideal For**:
- Single async operation (just use `await`)
- Highly sequential operations (no concurrency benefit)
- Simple fire-and-forget scenarios
- Real-time stream processing (consider Reactive Extensions)

### What are the prerequisites?

- **.NET 10.0 SDK** or later
- **C# 12** or later
- Basic understanding of **async/await**
- Any modern IDE (Visual Studio, VS Code, Rider)

### How do I install TaskListProcessor?

**Option 1: Add Project Reference**
```bash
dotnet add reference path/to/TaskListProcessing/TaskListProcessing.csproj
```

**Option 2: Copy Source**
Copy the `TaskListProcessing` folder to your project and add a project reference.

**Option 3: NuGet** (Coming Soon)
```bash
dotnet add package TaskListProcessor
```

---

## Basic Usage

### How do I create my first processor?

```csharp
using TaskListProcessing.Core;
using Microsoft.Extensions.Logging;

// With logging (recommended)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
using var processor = new TaskListProcessorEnhanced("MyProcessor", logger);

// Without logging (not recommended)
using var processor = new TaskListProcessorEnhanced("MyProcessor", null);
```

### How do I define tasks?

Tasks are defined as a dictionary where:
- **Key**: Task name (string)
- **Value**: Async function returning `Task<object?>`

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Task 1"] = async ct =>
    {
        await Task.Delay(100, ct);
        return "Result 1";
    },

    ["Task 2"] = async ct =>
    {
        var data = await FetchDataAsync(ct);
        return data;
    }
};
```

### How do I execute tasks?

```csharp
// Simple execution
await processor.ProcessTasksAsync(tasks);

// With cancellation
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
await processor.ProcessTasksAsync(tasks, cancellationToken: cts.Token);

// With progress reporting
var progress = new Progress<TaskProgress>(p =>
    Console.WriteLine($"{p.CompletedTasks}/{p.TotalTasks}"));
await processor.ProcessTasksAsync(tasks, progress);
```

### How do I access results?

```csharp
await processor.ProcessTasksAsync(tasks);

foreach (var result in processor.TaskResults)
{
    Console.WriteLine($"Task: {result.Name}");
    Console.WriteLine($"Success: {result.IsSuccessful}");

    if (result.IsSuccessful)
    {
        Console.WriteLine($"Data: {result.Data}");
        Console.WriteLine($"Duration: {result.ExecutionTime.TotalMilliseconds}ms");
    }
    else
    {
        Console.WriteLine($"Error: {result.ErrorMessage}");
        Console.WriteLine($"Category: {result.ErrorCategory}");
    }
}
```

---

## Error Handling

### How does error handling work?

TaskListProcessor automatically:
1. Catches exceptions from tasks
2. Categorizes errors
3. Stores error details in results
4. Continues processing other tasks
5. Logs errors (if logger provided)

**You don't need try/catch** in your task factories. Just let exceptions bubble up:

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["MayFail"] = async ct =>
    {
        // No try/catch needed - just let it throw
        return await RiskyOperationAsync(ct);
    }
};
```

### How do I handle individual task failures?

```csharp
await processor.ProcessTasksAsync(tasks);

var failed = processor.TaskResults.Where(r => !r.IsSuccessful);

foreach (var failure in failed)
{
    logger.LogError("Task {Name} failed: {Error} ({Category})",
        failure.Name,
        failure.ErrorMessage,
        failure.ErrorCategory);

    // Take corrective action based on error category
    switch (failure.ErrorCategory)
    {
        case ErrorCategory.Timeout:
            // Retry with longer timeout
            break;
        case ErrorCategory.Network:
            // Check connectivity, retry
            break;
        case ErrorCategory.Validation:
            // Fix input data
            break;
    }
}
```

### Will one failed task stop all others?

**No!** Task failures are isolated. Other tasks continue to completion.

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Success"] = async ct => "OK",
    ["Failure"] = async ct => throw new Exception("Boom!"),
    ["AlsoSuccess"] = async ct => "Also OK"
};

await processor.ProcessTasksAsync(tasks);

// Result:
// Success: ✅ "OK"
// Failure: ❌ Exception
// AlsoSuccess: ✅ "Also OK"
```

### How do I categorize errors?

Errors are automatically categorized. You can also throw specific exceptions:

```csharp
["MyTask"] = async ct =>
{
    if (invalidInput)
        throw new ArgumentException("Invalid input"); // → ErrorCategory.Validation

    if (!await networkAvailable)
        throw new HttpRequestException("No network"); // → ErrorCategory.Network

    if (await IsTimedOut(ct))
        throw new TimeoutException("Timed out"); // → ErrorCategory.Timeout

    return await ProcessAsync(ct);
}
```

---

## Performance

### How many tasks can run concurrently?

**Default**: `Environment.ProcessorCount * 2`
- On a 8-core CPU: 16 concurrent tasks

**Configure** via options:
```csharp
var options = new TaskListProcessorOptions
{
    MaxConcurrentTasks = 10 // Limit to 10
};

var processor = new TaskListProcessorEnhanced("MyProcessor", logger, options);
```

### How do I set concurrency limits?

```csharp
var options = new TaskListProcessorOptions
{
    MaxConcurrentTasks = 5, // Max 5 tasks at once
    DefaultTaskTimeout = TimeSpan.FromSeconds(30)
};

using var processor = new TaskListProcessorEnhanced("Limited", logger, options);
```

**When to limit**:
- External API rate limits
- Database connection pool limits
- Memory constraints
- Resource throttling

### What's the performance overhead?

**Minimal overhead**:
- **Task scheduling**: < 1ms per task
- **Telemetry collection**: < 0.1ms per task
- **Result aggregation**: O(1) per task

**Benchmark** (1000 tasks):
- Total overhead: ~50ms
- Per-task overhead: ~0.05ms

The overhead is **negligible** compared to typical I/O operations (network, database).

### How do I optimize for throughput?

**1. Increase concurrency** (if resources allow):
```csharp
var options = new TaskListProcessorOptions
{
    MaxConcurrentTasks = Environment.ProcessorCount * 4
};
```

**2. Use streaming** for large batches:
```csharp
await foreach (var result in processor.ProcessTasksStreamAsync(tasks))
{
    // Process immediately, don't wait for all
    await HandleResultAsync(result);
}
```

**3. Disable detailed telemetry** (if not needed):
```csharp
var options = new TaskListProcessorOptions
{
    EnableDetailedTelemetry = false
};
```

**4. Use task priorities**:
```csharp
var definitions = new[]
{
    new TaskDefinition
    {
        Name = "Critical",
        Factory = criticalTask,
        Priority = TaskPriority.High
    }
};
```

---

## Advanced Features

### How do I use dependency injection?

**ASP.NET Core / Generic Host**:

```csharp
// Program.cs or Startup.cs
builder.Services.AddTaskListProcessor(options =>
{
    options.MaxConcurrentTasks = 10;
    options.EnableDetailedTelemetry = true;
})
.WithLogging()
.WithMetrics()
.WithCircuitBreaker();

// In your service
public class MyService
{
    private readonly ITaskBatchProcessor _processor;

    public MyService(ITaskBatchProcessor processor)
    {
        _processor = processor;
    }

    public async Task ProcessAsync()
    {
        var tasks = CreateTasks();
        await _processor.ProcessTasksAsync(tasks);
    }
}
```

### How do task dependencies work?

Use `TaskDefinition` to specify dependencies:

```csharp
var definitions = new[]
{
    new TaskDefinition
    {
        Name = "Step1",
        Factory = step1Task,
        Priority = TaskPriority.High
    },
    new TaskDefinition
    {
        Name = "Step2",
        Factory = step2Task,
        Dependencies = new[] { "Step1" }, // Runs after Step1
        Priority = TaskPriority.Medium
    },
    new TaskDefinition
    {
        Name = "Step3",
        Factory = step3Task,
        Dependencies = new[] { "Step2" } // Runs after Step2
    }
};

await processor.ProcessTaskDefinitionsAsync(definitions);
```

**Execution order**: Step1 → Step2 → Step3

### What is the circuit breaker pattern?

The circuit breaker prevents cascading failures by:
1. Monitoring task failure rates
2. "Opening" (stopping execution) when failures exceed threshold
3. Automatically "closing" (resuming) after recovery timeout

```csharp
var options = new TaskListProcessorOptions
{
    CircuitBreakerOptions = new CircuitBreakerOptions
    {
        FailureThreshold = 5,           // Open after 5 failures
        RecoveryTimeout = TimeSpan.FromMinutes(2) // Try again after 2 min
    }
};

using var processor = new TaskListProcessorEnhanced("Resilient", logger, options);
```

### How do I use streaming results?

Process results as they complete instead of waiting for all:

```csharp
await foreach (var result in processor.ProcessTasksStreamAsync(tasks))
{
    Console.WriteLine($"Completed: {result.Name}");

    if (result.IsSuccessful)
    {
        // Process immediately
        await SaveToDatabase(result.Data);
    }
}
```

**Benefits**:
- Lower memory usage
- Earlier result processing
- Better responsiveness

### How do I implement progress reporting?

```csharp
var progress = new Progress<TaskProgress>(p =>
{
    Console.WriteLine($"Progress: {p.CompletedTasks}/{p.TotalTasks}");
    Console.WriteLine($"Current: {p.CurrentTask}");
    Console.WriteLine($"Percentage: {p.PercentComplete:F1}%");
});

await processor.ProcessTasksAsync(tasks, progress);
```

---

## Telemetry & Monitoring

### What telemetry is collected?

For each task:
- Task name
- Start time
- End time
- Duration (milliseconds)
- Success/failure status
- Error message (if failed)
- Error category (if failed)

Aggregated:
- Total tasks
- Success count
- Failure count
- Average execution time
- Min/max execution time

### How do I access telemetry data?

```csharp
await processor.ProcessTasksAsync(tasks);

// Individual telemetry
foreach (var telemetry in processor.Telemetry)
{
    Console.WriteLine($"{telemetry.TaskName}: {telemetry.DurationMs}ms");
}

// Summary
var summary = processor.GetTelemetrySummary();
Console.WriteLine($"Success Rate: {summary.SuccessRate:F1}%");
Console.WriteLine($"Avg Time: {summary.AverageExecutionTime:F0}ms");
```

### Can I integrate with OpenTelemetry?

**Yes!** OpenTelemetry integration is supported:

```csharp
// Coming soon - full example
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddTaskListProcessorInstrumentation());
```

See [OpenTelemetry Integration Tutorial](../tutorials/advanced/03-opentelemetry-integration.md) for details.

### How do I implement health checks?

```csharp
var options = new TaskListProcessorOptions
{
    HealthCheckOptions = new HealthCheckOptions
    {
        MinSuccessRate = 0.8, // 80% minimum
        MaxAverageExecutionTime = TimeSpan.FromSeconds(5)
    }
};

using var processor = new TaskListProcessorEnhanced("Monitored", logger, options);

await processor.ProcessTasksAsync(tasks);

var health = processor.PerformHealthCheck();
if (!health.IsHealthy)
{
    logger.LogWarning("Health check failed: {Message}", health.Message);
}
```

---

## Troubleshooting

### Why are my tasks not running concurrently?

**Common causes**:

1. **Using `.Result` or `.Wait()`**:
```csharp
// ❌ Wrong - blocks thread
["Task"] = ct => Task.FromResult(SomeMethod().Result)

// ✅ Correct - truly async
["Task"] = async ct => await SomeMethodAsync(ct)
```

2. **Synchronous CPU work**:
```csharp
// ❌ Wrong - blocks async thread
["Task"] = async ct => { ExpensiveComputation(); return "OK"; }

// ✅ Correct - offload to thread pool
["Task"] = async ct => await Task.Run(() => ExpensiveComputation(), ct)
```

3. **Concurrency limit too low**:
```csharp
var options = new TaskListProcessorOptions
{
    MaxConcurrentTasks = 100 // Increase if needed
};
```

### Why is my application hanging?

**Common causes**:

1. **Deadlock from `.Result`**:
```csharp
// ❌ NEVER do this
var result = processor.ProcessTasksAsync(tasks).Result;

// ✅ Always await
var result = await processor.ProcessTasksAsync(tasks);
```

2. **Missing cancellation token**:
```csharp
// ❌ Can't be cancelled
await Task.Delay(TimeSpan.FromHours(1));

// ✅ Cancellable
await Task.Delay(TimeSpan.FromHours(1), ct);
```

3. **Circular task dependencies**:
```csharp
// ❌ Circular dependency
new TaskDefinition { Name = "A", Dependencies = new[] { "B" } }
new TaskDefinition { Name = "B", Dependencies = new[] { "A" } }
```

### How do I debug task execution?

**1. Enable detailed logging**:
```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<Program>();
var processor = new TaskListProcessorEnhanced("Debug", logger);
```

**2. Add breakpoints** in task factories

**3. Use telemetry**:
```csharp
await processor.ProcessTasksAsync(tasks);

// Analyze execution
var slowTasks = processor.Telemetry
    .Where(t => t.DurationMs > 1000)
    .OrderByDescending(t => t.DurationMs);

foreach (var task in slowTasks)
{
    Console.WriteLine($"{task.TaskName}: {task.DurationMs}ms");
}
```

### Why am I getting OutOfMemoryException?

**Common causes**:

1. **Not disposing processor**:
```csharp
// ❌ Wrong - memory leak
var processor = new TaskListProcessorEnhanced("Leak", logger);

// ✅ Correct - automatic disposal
using var processor = new TaskListProcessorEnhanced("NoLeak", logger);
```

2. **Accumulating results**:
```csharp
// ❌ Results accumulate
for (int i = 0; i < 10000; i++)
{
    await processor.ProcessTasksAsync(tasks);
    // Results keep growing!
}

// ✅ Create new processor per batch
for (int i = 0; i < 10000; i++)
{
    using var p = new TaskListProcessorEnhanced($"Batch{i}", logger);
    await p.ProcessTasksAsync(tasks);
    // Disposed after each batch
}
```

3. **Too many concurrent tasks**:
```csharp
var options = new TaskListProcessorOptions
{
    MaxConcurrentTasks = 10 // Reduce if memory constrained
};
```

---

## Best Practices

### Should I reuse processor instances?

**Short answer**: Use `using` and create new instances.

**Long answer**:
- ✅ **Good**: Create per operation or batch
- ❌ **Bad**: Reuse across thousands of operations
- ⚠️ **OK**: Reuse for related tasks in same operation

```csharp
// ✅ Recommended
public async Task ProcessBatch(List<Data> batch)
{
    using var processor = new TaskListProcessorEnhanced("Batch", logger);
    var tasks = CreateTasks(batch);
    await processor.ProcessTasksAsync(tasks);
}

// ⚠️ OK for same operation
public async Task ProcessAll()
{
    using var processor = new TaskListProcessorEnhanced("All", logger);

    await processor.ProcessTasksAsync(batch1Tasks);
    await processor.ProcessTasksAsync(batch2Tasks);
    // Same logical operation
}
```

### How do I handle long-running tasks?

**1. Set appropriate timeouts**:
```csharp
var options = new TaskListProcessorOptions
{
    DefaultTaskTimeout = TimeSpan.FromMinutes(30)
};
```

**2. Implement progress reporting**:
```csharp
["LongTask"] = async ct =>
{
    for (int i = 0; i < 100; i++)
    {
        await ProcessChunk(i, ct);
        // Report progress periodically
    }
    return "Complete";
}
```

**3. Consider breaking into smaller tasks**:
```csharp
// Instead of one 30-minute task
["MonolithicTask"] = async ct => await Process30Minutes(ct);

// Break into chunks
for (int i = 0; i < 30; i++)
{
    tasks[$"Chunk{i}"] = async ct => await ProcessOneMinute(i, ct);
}
```

### What's the best way to handle timeouts?

**Per-operation timeout**:
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
await processor.ProcessTasksAsync(tasks, cancellationToken: cts.Token);
```

**Per-task timeout** (inside task):
```csharp
["TimedTask"] = async ct =>
{
    using var taskCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    taskCts.CancelAfter(TimeSpan.FromSeconds(30));

    return await SomeOperationAsync(taskCts.Token);
}
```

**Global default**:
```csharp
var options = new TaskListProcessorOptions
{
    DefaultTaskTimeout = TimeSpan.FromMinutes(5)
};
```

### How do I test code using TaskListProcessor?

**Unit testing**:
```csharp
[Fact]
public async Task TestTaskExecution()
{
    // Arrange
    using var processor = new TaskListProcessorEnhanced("Test", null);
    var executed = false;

    var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
    {
        ["TestTask"] = async ct =>
        {
            executed = true;
            return "Success";
        }
    };

    // Act
    await processor.ProcessTasksAsync(tasks);

    // Assert
    Assert.True(executed);
    Assert.Single(processor.TaskResults);
    Assert.True(processor.TaskResults.First().IsSuccessful);
}
```

**Mocking** (with DI):
```csharp
[Fact]
public async Task TestWithMock()
{
    // Arrange
    var mockProcessor = new Mock<ITaskBatchProcessor>();
    var service = new MyService(mockProcessor.Object);

    // Act
    await service.ProcessAsync();

    // Assert
    mockProcessor.Verify(p =>
        p.ProcessTasksAsync(It.IsAny<Dictionary<string, Func<CancellationToken, Task<object?>>>>(),
        It.IsAny<IProgress<TaskProgress>>(),
        It.IsAny<CancellationToken>()), Times.Once);
}
```

---

## Still Have Questions?

- 💬 [Ask in GitHub Discussions](https://github.com/markhazleton/TaskListProcessor/discussions)
- 🐛 [Report an Issue](https://github.com/markhazleton/TaskListProcessor/issues)
- 📖 [Read the Documentation](../../README.md)
- 📧 [Contact Mark Hazleton](mailto:mark@markhazleton.com)

---

*Last Updated: 2025-12-07*
*Built with ❤️ by [Mark Hazleton](https://markhazleton.com)*
