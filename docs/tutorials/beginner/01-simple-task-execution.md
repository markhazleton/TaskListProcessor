# Tutorial 1: Simple Task Execution

**Level**: Beginner
**Duration**: 15 minutes
**Prerequisites**:
- .NET 10.0 SDK installed
- Basic C# knowledge
- Completed [5-Minute Quick Start](../../getting-started/01-quick-start-5-minutes.md)

---

## 🎯 What You'll Learn

By the end of this tutorial, you'll be able to:

- ✅ Create a basic TaskListProcessor instance
- ✅ Define simple task factories
- ✅ Execute tasks concurrently
- ✅ Understand task execution results
- ✅ Handle basic cancellation

---

## 📚 Concepts Covered

### Task Factories

In TaskListProcessor, tasks are defined as **factory functions** that return `Task<object?>`:

```csharp
Func<CancellationToken, Task<object?>> taskFactory
```

**Why factories?**
- **Lazy execution**: Tasks don't run until you want them to
- **Cancellation support**: Built-in cancellation token support
- **Flexible return types**: Any object can be returned
- **Reusability**: Same factory can be used multiple times

### Concurrent Execution

TaskListProcessor runs tasks **concurrently** (in parallel) by default, which means:
- Multiple tasks execute at the same time
- Faster overall completion than sequential execution
- Configurable concurrency limits to prevent overload

---

## 🔨 Step-by-Step Tutorial

### Step 1: Set Up Your Project

Create a new console application:

```bash
dotnet new console -n SimpleTaskExecution
cd SimpleTaskExecution
dotnet add package TaskListProcessor
```

### Step 2: Create Your First Processor

Open `Program.cs` and replace the contents with:

```csharp
using Microsoft.Extensions.Logging;
using TaskListProcessor;

// Create a logger for visibility
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

// Create the processor
using var processor = new TaskListProcessorEnhanced(
    name: "My First Processor",
    logger: logger
);

Console.WriteLine("TaskListProcessor created successfully!");
```

**What's happening here?**
- We create a logger to see what's happening inside the processor
- We create a `TaskListProcessorEnhanced` instance with a descriptive name
- The `using` statement ensures proper cleanup when we're done

### Step 3: Define Simple Tasks

Add task definitions after the processor creation:

```csharp
// Define a dictionary of tasks
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Greet"] = async ct =>
    {
        await Task.Delay(100, ct); // Simulate work
        return "Hello from Task 1!";
    },

    ["Calculate"] = async ct =>
    {
        await Task.Delay(150, ct); // Simulate work
        var result = 42 * 2;
        return $"Calculation result: {result}";
    },

    ["FetchData"] = async ct =>
    {
        await Task.Delay(200, ct); // Simulate API call
        return new { Id = 1, Name = "Sample Data", Timestamp = DateTime.UtcNow };
    }
};

Console.WriteLine($"Defined {tasks.Count} tasks");
```

**Understanding the task dictionary**:
- **Key** (`string`): Unique name for the task (used in logs and results)
- **Value** (`Func<CancellationToken, Task<object?>>`): The task factory function
- Each task accepts a `CancellationToken` for graceful cancellation
- Each task returns `Task<object?>` - any type of result

### Step 4: Execute Tasks

Add the execution code:

```csharp
// Create a cancellation token source (optional, for timeout control)
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

Console.WriteLine("\nStarting task execution...\n");

// Execute all tasks concurrently
var results = await processor.ProcessTasksAsync(tasks, cts.Token);

Console.WriteLine("\n✅ All tasks completed!");
```

**What happens during execution?**
1. Processor validates all tasks
2. Tasks start executing concurrently (in parallel)
3. Processor waits for all tasks to complete
4. Results are collected and returned
5. Any errors are isolated per task

### Step 5: View Results

Add code to examine the results:

```csharp
// Display results
Console.WriteLine("\n📊 Results:\n");
foreach (var result in results)
{
    Console.WriteLine($"Task: {result.Key}");
    Console.WriteLine($"  Status: {result.Value.Status}");
    Console.WriteLine($"  Duration: {result.Value.Duration}ms");

    if (result.Value.IsSuccess && result.Value.Result != null)
    {
        Console.WriteLine($"  Result: {result.Value.Result}");
    }
    else if (result.Value.Exception != null)
    {
        Console.WriteLine($"  Error: {result.Value.Exception.Message}");
    }

    Console.WriteLine();
}

// Summary statistics
var successful = results.Values.Count(r => r.IsSuccess);
var failed = results.Values.Count(r => !r.IsSuccess);
var totalDuration = results.Values.Sum(r => r.Duration);

Console.WriteLine("📈 Summary:");
Console.WriteLine($"  Total tasks: {results.Count}");
Console.WriteLine($"  Successful: {successful}");
Console.WriteLine($"  Failed: {failed}");
Console.WriteLine($"  Total time: {totalDuration}ms");
```

### Step 6: Run Your Application

Execute your program:

```bash
dotnet run
```

**Expected output:**

```
TaskListProcessor created successfully!
Defined 3 tasks

Starting task execution...

info: TaskListProcessorEnhanced[0]
      Starting processor: My First Processor
info: TaskListProcessorEnhanced[0]
      Executing task: Greet
info: TaskListProcessorEnhanced[0]
      Executing task: Calculate
info: TaskListProcessorEnhanced[0]
      Executing task: FetchData
info: TaskListProcessorEnhanced[0]
      Task completed: Greet (105ms)
info: TaskListProcessorEnhanced[0]
      Task completed: Calculate (153ms)
info: TaskListProcessorEnhanced[0]
      Task completed: FetchData (204ms)

✅ All tasks completed!

📊 Results:

Task: Greet
  Status: RanToCompletion
  Duration: 105ms
  Result: Hello from Task 1!

Task: Calculate
  Status: RanToCompletion
  Duration: 153ms
  Result: Calculation result: 84

Task: FetchData
  Status: RanToCompletion
  Duration: 204ms
  Result: { Id = 1, Name = Sample Data, Timestamp = 12/7/2025 2:30:45 PM }

📈 Summary:
  Total tasks: 3
  Successful: 3
  Failed: 0
  Total time: 462ms
```

---

## 🧪 Experiment: See Concurrent Execution

To prove tasks run concurrently (not sequentially), modify the tasks to have longer delays:

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Task1"] = async ct =>
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task1 started");
        await Task.Delay(1000, ct);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task1 completed");
        return "Task1 result";
    },

    ["Task2"] = async ct =>
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task2 started");
        await Task.Delay(1000, ct);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task2 completed");
        return "Task2 result";
    },

    ["Task3"] = async ct =>
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task3 started");
        await Task.Delay(1000, ct);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task3 completed");
        return "Task3 result";
    }
};
```

**Run it and observe**:
- All tasks start at nearly the same time
- All tasks complete at nearly the same time (~1 second total)
- If they ran sequentially, it would take ~3 seconds total

**Output:**
```
[14:30:45.123] Task1 started
[14:30:45.125] Task2 started
[14:30:45.127] Task3 started
[14:30:46.128] Task1 completed
[14:30:46.130] Task2 completed
[14:30:46.132] Task3 completed
```

✅ **Proof of concurrency**: ~1 second total instead of 3 seconds!

---

## 🎓 Complete Example

Here's the full working example you can copy-paste:

```csharp
using Microsoft.Extensions.Logging;
using TaskListProcessor;

// Create logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

// Create processor
using var processor = new TaskListProcessorEnhanced(
    name: "Simple Task Execution Demo",
    logger: logger
);

// Define tasks
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Greet"] = async ct =>
    {
        await Task.Delay(100, ct);
        return "Hello from Task 1!";
    },

    ["Calculate"] = async ct =>
    {
        await Task.Delay(150, ct);
        return $"Calculation result: {42 * 2}";
    },

    ["FetchData"] = async ct =>
    {
        await Task.Delay(200, ct);
        return new { Id = 1, Name = "Sample Data", Timestamp = DateTime.UtcNow };
    }
};

// Execute tasks
Console.WriteLine("\nStarting task execution...\n");
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var results = await processor.ProcessTasksAsync(tasks, cts.Token);

// Display results
Console.WriteLine("\n📊 Results:\n");
foreach (var result in results)
{
    Console.WriteLine($"Task: {result.Key}");
    Console.WriteLine($"  Status: {result.Value.Status}");
    Console.WriteLine($"  Duration: {result.Value.Duration}ms");
    if (result.Value.IsSuccess && result.Value.Result != null)
    {
        Console.WriteLine($"  Result: {result.Value.Result}");
    }
    Console.WriteLine();
}

// Summary
var successful = results.Values.Count(r => r.IsSuccess);
Console.WriteLine($"✅ {successful}/{results.Count} tasks completed successfully!");
```

---

## ⚠️ Common Beginner Mistakes

### 1. **Blocking Async Code**

❌ **Wrong**:
```csharp
["BadTask"] = async ct =>
{
    var result = SomeAsyncMethod().Result; // BLOCKS!
    return result;
}
```

✅ **Correct**:
```csharp
["GoodTask"] = async ct =>
{
    var result = await SomeAsyncMethod(); // ASYNC!
    return result;
}
```

### 2. **Ignoring CancellationToken**

❌ **Wrong**:
```csharp
["BadTask"] = async ct =>
{
    await Task.Delay(5000); // Ignores cancellation!
    return "Done";
}
```

✅ **Correct**:
```csharp
["GoodTask"] = async ct =>
{
    await Task.Delay(5000, ct); // Respects cancellation!
    return "Done";
}
```

### 3. **Not Handling Exceptions**

❌ **Wrong** (crashes entire app):
```csharp
["BadTask"] = async ct =>
{
    throw new Exception("Unhandled!"); // Crashes!
}
```

✅ **Correct** (handled by processor):
```csharp
["GoodTask"] = async ct =>
{
    try
    {
        // Your code that might fail
        return await RiskyOperationAsync();
    }
    catch (Exception ex)
    {
        // Log or handle
        throw; // Processor will catch and isolate
    }
}
```

**Good news**: TaskListProcessor automatically isolates exceptions! One task failing won't crash others.

### 4. **Duplicate Task Names**

❌ **Wrong**:
```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["MyTask"] = async ct => "First",
    ["MyTask"] = async ct => "Second" // Overwrites first!
};
```

✅ **Correct**:
```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["MyTask1"] = async ct => "First",
    ["MyTask2"] = async ct => "Second"
};
```

---

## 🧩 Practice Exercises

### Exercise 1: Temperature Converter

Create a processor that converts a temperature to different units:

```csharp
var celsius = 25.0;

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["ToFahrenheit"] = async ct => {
        await Task.Delay(50, ct);
        return (celsius * 9 / 5) + 32;
    },

    ["ToKelvin"] = async ct => {
        await Task.Delay(50, ct);
        return celsius + 273.15;
    },

    // Add Rankine conversion yourself!
};
```

### Exercise 2: Multiple API Simulations

Simulate fetching data from different APIs:

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["WeatherAPI"] = async ct => {
        await Task.Delay(Random.Shared.Next(100, 500), ct);
        return new { Temperature = 72, Condition = "Sunny" };
    },

    // Add: StockAPI, NewsAPI, CryptoAPI
};
```

### Exercise 3: Task Timing Analysis

Measure how much faster concurrent execution is:

1. Run 10 tasks that each take 500ms
2. Calculate total duration
3. Compare to sequential (10 × 500ms = 5000ms)
4. Calculate speedup ratio

---

## 🎯 Key Takeaways

✅ **Task factories** are functions that create tasks when needed
✅ **Concurrent execution** runs tasks in parallel for speed
✅ **CancellationToken** allows graceful task cancellation
✅ **Results dictionary** contains status, duration, and results for each task
✅ **Isolated errors** mean one task failing doesn't crash others

---

## 📚 What's Next?

Now that you understand simple task execution, you're ready for:

**Next Tutorial**: [02-batch-processing.md](02-batch-processing.md)
Learn how to process large collections efficiently with batching strategies.

**Related Reading**:
- [Fundamentals](../../getting-started/02-fundamentals.md) - Deep dive into core concepts
- [Common Pitfalls](../../getting-started/04-common-pitfalls.md) - Avoid these mistakes
- [FAQ](../../troubleshooting/faq.md) - Common questions answered

---

## 🆘 Need Help?

- **Found a bug?** [Report it](https://github.com/markhazleton/TaskListProcessor/issues/new?template=bug_report.yml)
- **Have a question?** Check the [FAQ](../../troubleshooting/faq.md)
- **Want to contribute?** See [Contributing Guide](../../CONTRIBUTING.md)

---

**Completed**: Tutorial 1 - Simple Task Execution ✅
**Next**: Tutorial 2 - Batch Processing →
