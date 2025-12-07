# Common Pitfalls & How to Avoid Them

Learn from others' mistakes! This guide covers the most common pitfalls when using TaskListProcessor and shows you how to avoid them with clear before/after examples.

---

## Table of Contents

- [1. Blocking Async Code](#1-blocking-async-code)
- [2. Improper Error Handling](#2-improper-error-handling)
- [3. Memory Leaks with Large Result Sets](#3-memory-leaks-with-large-result-sets)
- [4. Not Using Cancellation Tokens](#4-not-using-cancellation-tokens)
- [5. Ignoring Task Dependencies](#5-ignoring-task-dependencies)
- [6. Over-Concurrent Execution](#6-over-concurrent-execution)
- [7. Poor Logging Practices](#7-poor-logging-practices)
- [8. Synchronous Operations in Async Context](#8-synchronous-operations-in-async-context)
- [9. Improper Disposal](#9-improper-disposal)
- [10. Not Checking Result Success](#10-not-checking-result-success)

---

## 1. Blocking Async Code

### ❌ **Wrong**: Using `.Result` or `.Wait()`

```csharp
// DON'T DO THIS - Causes deadlocks and blocks threads
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["BlockingTask"] = ct =>
    {
        var data = httpClient.GetStringAsync("https://api.example.com").Result; // ❌ BLOCKING!
        return Task.FromResult<object?>(data);
    }
};
```

**Problems**:
- Blocks the thread pool thread
- Can cause deadlocks in UI applications
- Reduces throughput and scalability
- Defeats the purpose of async/await

### ✅ **Correct**: Proper async/await

```csharp
// DO THIS - Truly asynchronous
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["AsyncTask"] = async ct =>
    {
        var data = await httpClient.GetStringAsync("https://api.example.com", ct); // ✅ ASYNC!
        return data;
    }
};
```

**Benefits**:
- Non-blocking
- Scalable
- No deadlock risk
- Efficient thread pool usage

---

## 2. Improper Error Handling

### ❌ **Wrong**: Catching and hiding exceptions

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["BadErrorHandling"] = async ct =>
    {
        try
        {
            return await CallApiAsync(ct);
        }
        catch
        {
            return null; // ❌ Error is hidden!
        }
    }
};

await processor.ProcessTasksAsync(tasks);

// You'll never know this task failed!
```

**Problems**:
- Errors are silently swallowed
- No telemetry for failures
- Impossible to debug
- Results appear successful when they're not

### ✅ **Correct**: Let TaskListProcessor handle errors

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["GoodErrorHandling"] = async ct =>
    {
        // Just let exceptions bubble up
        return await CallApiAsync(ct); // ✅ Processor handles errors
    }
};

await processor.ProcessTasksAsync(tasks);

// Check results
foreach (var result in processor.TaskResults)
{
    if (!result.IsSuccessful)
    {
        logger.LogError("Task {Name} failed: {Error}",
            result.Name, result.ErrorMessage);
    }
}
```

**Benefits**:
- Automatic error capture
- Detailed telemetry
- Error categorization
- Easy to identify failures

---

## 3. Memory Leaks with Large Result Sets

### ❌ **Wrong**: Holding onto processor instances

```csharp
// DON'T DO THIS - Memory leak!
var processor = new TaskListProcessorEnhanced("MyProcessor", logger);

for (int i = 0; i < 1000; i++)
{
    var tasks = CreateTasks();
    await processor.ProcessTasksAsync(tasks);

    // ❌ Results keep accumulating in processor.TaskResults!
}
// processor.TaskResults now has 1000+ results in memory
```

**Problems**:
- Memory grows unbounded
- Eventually causes OutOfMemoryException
- Performance degrades over time
- Results never cleaned up

### ✅ **Correct**: Use `using` for proper disposal

```csharp
// DO THIS - Proper cleanup
for (int i = 0; i < 1000; i++)
{
    using var processor = new TaskListProcessorEnhanced("MyProcessor", logger);

    var tasks = CreateTasks();
    await processor.ProcessTasksAsync(tasks);

    // Process results immediately
    ProcessResults(processor.TaskResults);

    // ✅ Processor disposed at end of iteration, memory released
}
```

**Benefits**:
- Bounded memory usage
- Automatic cleanup
- Scalable for long-running processes
- No memory leaks

---

## 4. Not Using Cancellation Tokens

### ❌ **Wrong**: Ignoring cancellation

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["LongRunning"] = async ct => // ❌ Parameter ignored!
    {
        await Task.Delay(TimeSpan.FromMinutes(10)); // Can't be cancelled!
        return await FetchDataAsync();
    }
};

// User waits forever if they want to cancel
```

**Problems**:
- Tasks can't be cancelled
- Wasted resources
- Poor user experience
- Timeout issues

### ✅ **Correct**: Use and pass cancellation tokens

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["LongRunning"] = async ct => // ✅ Use the token!
    {
        await Task.Delay(TimeSpan.FromMinutes(10), ct); // Cancellable
        return await FetchDataAsync(ct); // Pass it along
    }
};

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await processor.ProcessTasksAsync(tasks, cancellationToken: cts.Token);
```

**Benefits**:
- Responsive cancellation
- Timeout support
- Resource cleanup
- Better user experience

---

## 5. Ignoring Task Dependencies

### ❌ **Wrong**: Assuming execution order

```csharp
// DON'T DO THIS - Race condition!
string sharedData = null;

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Initialize"] = async ct =>
    {
        await Task.Delay(500, ct);
        sharedData = "Initialized"; // ❌ May run after Process!
        return "OK";
    },

    ["Process"] = async ct =>
    {
        // ❌ sharedData might still be null!
        return $"Processing: {sharedData}";
    }
};
```

**Problems**:
- Race conditions
- Unpredictable results
- Intermittent failures
- Hard to debug

### ✅ **Correct**: Use TaskDefinition with dependencies

```csharp
using TaskListProcessing.Models;

var taskDefinitions = new[]
{
    new TaskDefinition
    {
        Name = "Initialize",
        Factory = async ct =>
        {
            await Task.Delay(500, ct);
            return "Initialized"; // ✅ Data in result
        },
        Priority = TaskPriority.High
    },

    new TaskDefinition
    {
        Name = "Process",
        Factory = async ct =>
        {
            // Get data from Initialize task result
            var initResult = processor.TaskResults
                .First(r => r.Name == "Initialize");
            return $"Processing: {initResult.Data}";
        },
        Dependencies = new[] { "Initialize" }, // ✅ Explicit dependency
        Priority = TaskPriority.Medium
    }
};

await processor.ProcessTaskDefinitionsAsync(taskDefinitions);
```

**Benefits**:
- Guaranteed execution order
- No race conditions
- Clear dependencies
- Predictable results

---

## 6. Over-Concurrent Execution

### ❌ **Wrong**: No concurrency limits

```csharp
// DON'T DO THIS - Overwhelms resources
var processor = new TaskListProcessorEnhanced("Unlimited", logger);

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

// Create 10,000 tasks that all hit the database
for (int i = 0; i < 10000; i++)
{
    int id = i;
    tasks[$"Query_{id}"] = async ct => await database.QueryAsync(id, ct);
}

await processor.ProcessTasksAsync(tasks); // ❌ All 10k run at once!
```

**Problems**:
- Exhausts connection pools
- Overwhelms external APIs
- Memory pressure
- Possible system crash

### ✅ **Correct**: Set concurrency limits

```csharp
// DO THIS - Controlled concurrency
var options = new TaskListProcessorOptions
{
    MaxConcurrentTasks = 10, // ✅ Limit to 10 concurrent tasks
    EnableDetailedTelemetry = true
};

var processor = new TaskListProcessorEnhanced("Limited", logger, options);

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

for (int i = 0; i < 10000; i++)
{
    int id = i;
    tasks[$"Query_{id}"] = async ct => await database.QueryAsync(id, ct);
}

await processor.ProcessTasksAsync(tasks); // ✅ Max 10 at a time
```

**Benefits**:
- Controlled resource usage
- Respects API rate limits
- Stable performance
- Prevents resource exhaustion

---

## 7. Poor Logging Practices

### ❌ **Wrong**: No logger or poor logging

```csharp
// DON'T DO THIS - No visibility
var processor = new TaskListProcessorEnhanced("MyProcessor", null); // ❌ No logger!

var tasks = CreateTasks();
await processor.ProcessTasksAsync(tasks);

// When something goes wrong, you have no idea what happened
```

**Problems**:
- No diagnostic information
- Can't troubleshoot issues
- Missing performance data
- Blind to errors

### ✅ **Correct**: Proper logging setup

```csharp
// DO THIS - Comprehensive logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<Program>();

var processor = new TaskListProcessorEnhanced("MyProcessor", logger); // ✅ Logger provided

var tasks = CreateTasks();
await processor.ProcessTasksAsync(tasks);

// Full visibility into execution
```

**Benefits**:
- Detailed execution logs
- Performance metrics
- Error diagnostics
- Troubleshooting data

---

## 8. Synchronous Operations in Async Context

### ❌ **Wrong**: CPU-bound work in async tasks

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["HeavyComputation"] = async ct =>
    {
        // ❌ Blocking CPU-bound work!
        var result = 0;
        for (int i = 0; i < 1_000_000_000; i++)
        {
            result += i;
        }
        return result;
    }
};
```

**Problems**:
- Blocks thread pool threads
- Wastes async infrastructure
- Reduces concurrency
- Poor performance

### ✅ **Correct**: Use Task.Run for CPU-bound work

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["HeavyComputation"] = async ct =>
    {
        // ✅ Offload to thread pool
        return await Task.Run(() =>
        {
            var result = 0;
            for (int i = 0; i < 1_000_000_000; i++)
            {
                result += i;
            }
            return (object?)result;
        }, ct);
    }
};
```

**Benefits**:
- Proper thread pool usage
- Better concurrency
- Async benefits preserved
- Correct semantic meaning

---

## 9. Improper Disposal

### ❌ **Wrong**: Not disposing resources

```csharp
// DON'T DO THIS - Resource leak!
public async Task ProcessData()
{
    var processor = new TaskListProcessorEnhanced("MyProcessor", logger);
    var httpClient = new HttpClient(); // ❌ Not disposed!

    var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
    {
        ["FetchData"] = async ct => await httpClient.GetStringAsync("https://api.example.com")
    };

    await processor.ProcessTasksAsync(tasks);

    // ❌ processor and httpClient never disposed!
}
```

**Problems**:
- Socket exhaustion
- Memory leaks
- File handle leaks
- Resource starvation

### ✅ **Correct**: Use `using` statements

```csharp
// DO THIS - Proper disposal
public async Task ProcessData()
{
    using var httpClient = new HttpClient(); // ✅ Will be disposed
    using var processor = new TaskListProcessorEnhanced("MyProcessor", logger); // ✅ Will be disposed

    var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
    {
        ["FetchData"] = async ct => await httpClient.GetStringAsync("https://api.example.com", ct)
    };

    await processor.ProcessTasksAsync(tasks);

    // ✅ Both disposed automatically at end of method
}
```

**Benefits**:
- Guaranteed cleanup
- No resource leaks
- Proper lifecycle management
- Exception-safe disposal

---

## 10. Not Checking Result Success

### ❌ **Wrong**: Assuming all tasks succeeded

```csharp
await processor.ProcessTasksAsync(tasks);

// ❌ Assuming success without checking!
foreach (var result in processor.TaskResults)
{
    var data = result.Data.ToString(); // ❌ Could be null if failed!
    ProcessData(data);
}
```

**Problems**:
- NullReferenceException
- Processing invalid data
- Silent failures
- Data corruption

### ✅ **Correct**: Always check success status

```csharp
await processor.ProcessTasksAsync(tasks);

// ✅ Check success before using data
foreach (var result in processor.TaskResults)
{
    if (result.IsSuccessful)
    {
        var data = result.Data;
        ProcessData(data);
    }
    else
    {
        logger.LogError("Task {Name} failed: {Error}",
            result.Name, result.ErrorMessage);

        // Handle failure appropriately
        HandleFailure(result.Name, result.ErrorMessage);
    }
}
```

**Benefits**:
- Safe data access
- Proper error handling
- No unexpected exceptions
- Robust error recovery

---

## Quick Checklist ✅

Before deploying to production, verify:

- [ ] All async operations use `await`, never `.Result` or `.Wait()`
- [ ] Exceptions are not caught and hidden in task factories
- [ ] Processor instances are properly disposed with `using`
- [ ] Cancellation tokens are used and passed through
- [ ] Task dependencies are explicitly defined when needed
- [ ] Concurrency limits are set appropriately
- [ ] Logging is configured and enabled
- [ ] CPU-bound work is offloaded with `Task.Run`
- [ ] All disposable resources use `using` statements
- [ ] Result success is checked before accessing data

---

## Anti-Pattern Examples

For comprehensive anti-pattern examples with full before/after refactorings, see:

- [Blocking Calls Anti-Pattern](../examples/anti-patterns/blocking-calls.md)
- [Memory Leaks Anti-Pattern](../examples/anti-patterns/memory-leaks.md)
- [Poor Error Handling](../examples/anti-patterns/poor-error-handling.md)
- [Deadlock Scenarios](../examples/anti-patterns/deadlock-scenarios.md)

---

## Additional Resources

- 📖 [Fundamentals Guide](02-fundamentals.md) - Core concepts
- 🧪 [Error Handling Tutorial](../tutorials/beginner/03-error-handling.md) - Detailed error handling
- 🔧 [Best Practices](../best-practices/async-await-patterns.md) - Async/await patterns
- ❓ [FAQ](../troubleshooting/faq.md) - Frequently asked questions

---

## Summary

The most common pitfalls are:

1. **Blocking** - Always use `await`, never `.Result`
2. **Error Hiding** - Let TaskListProcessor handle errors
3. **Memory Leaks** - Use `using` for disposal
4. **Ignoring Cancellation** - Always use and pass cancellation tokens
5. **Race Conditions** - Use explicit dependencies
6. **Over-Concurrency** - Set appropriate limits
7. **No Logging** - Always provide a logger
8. **Sync in Async** - Use `Task.Run` for CPU-bound work
9. **No Disposal** - Use `using` statements
10. **Not Checking Success** - Always validate results

**Remember**: Most issues stem from not fully embracing asynchronous patterns. When in doubt, follow the examples in this guide!

---

*Built with ❤️ by [Mark Hazleton](https://markhazleton.com)*
