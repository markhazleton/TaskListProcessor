# 5-Minute Quick Start

Get up and running with TaskListProcessor in just 5 minutes! This guide will take you from zero to executing your first concurrent tasks.

---

## What You'll Build

By the end of this quick start, you'll have:
- ✅ A working TaskListProcessor implementation
- ✅ Multiple tasks running concurrently
- ✅ Automatic error handling and telemetry
- ✅ Beautiful formatted output with success/failure indicators

**Time to complete**: ~5 minutes

---

## Prerequisites

- .NET 10.0 SDK installed ([Download here](https://dotnet.microsoft.com/download/dotnet/10.0))
- A code editor (Visual Studio, VS Code, or Rider)
- Basic knowledge of C# and async/await

---

## Step 1: Create a New Console Application (1 minute)

Open your terminal and run:

```bash
dotnet new console -n MyFirstProcessor
cd MyFirstProcessor
```

---

## Step 2: Add TaskListProcessor Reference (1 minute)

### Option A: Add Project Reference (For Local Development)

If you've cloned the TaskListProcessor repository:

```bash
dotnet add reference path/to/TaskListProcessor/src/TaskListProcessing/TaskListProcessing.csproj
```

### Option B: Copy the Core Files (Quick Start)

1. Copy the `TaskListProcessing` folder from `src/TaskListProcessing` to your project
2. Add a project reference:

```bash
dotnet add reference TaskListProcessing/TaskListProcessing.csproj
```

---

## Step 3: Write Your First Processor (2 minutes)

Replace the contents of `Program.cs` with:

```csharp
using TaskListProcessing.Core;
using Microsoft.Extensions.Logging;

// Setup simple logging
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<Program>();

Console.WriteLine("🚀 TaskListProcessor - Quick Start Demo\n");

// Create the processor
using var processor = new TaskListProcessorEnhanced("Quick Demo", logger);

// Define your tasks
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Fetch Weather"] = async ct =>
    {
        await Task.Delay(500, ct); // Simulate API call
        return "Sunny, 72°F";
    },

    ["Get Stock Price"] = async ct =>
    {
        await Task.Delay(300, ct); // Simulate API call
        return "$150.25";
    },

    ["Load User Data"] = async ct =>
    {
        await Task.Delay(700, ct); // Simulate database query
        return new { Name = "John Doe", Email = "john@example.com" };
    },

    ["Check System Status"] = async ct =>
    {
        await Task.Delay(200, ct); // Simulate health check
        return "All systems operational";
    }
};

// Execute all tasks concurrently
Console.WriteLine("⏳ Processing tasks...\n");
await processor.ProcessTasksAsync(tasks);

// Display results
Console.WriteLine("📊 Results:\n");
foreach (var result in processor.TaskResults)
{
    var status = result.IsSuccessful ? "✅" : "❌";
    var duration = $"({result.ExecutionTime.TotalMilliseconds:F0}ms)";

    Console.WriteLine($"{status} {result.Name,-20} {duration}");

    if (result.IsSuccessful)
    {
        Console.WriteLine($"   Data: {result.Data}");
    }
    else
    {
        Console.WriteLine($"   Error: {result.ErrorMessage}");
    }
}

// Display summary
Console.WriteLine($"\n📈 Summary:");
Console.WriteLine($"   Total Tasks: {processor.TaskResults.Count}");
Console.WriteLine($"   Successful: {processor.TaskResults.Count(r => r.IsSuccessful)}");
Console.WriteLine($"   Failed: {processor.TaskResults.Count(r => !r.IsSuccessful)}");
Console.WriteLine($"   Total Time: {processor.TaskResults.Max(r => r.ExecutionTime).TotalMilliseconds:F0}ms");

Console.WriteLine("\n✨ Demo complete!");
```

---

## Step 4: Run Your Application (1 minute)

```bash
dotnet run
```

**Expected Output**:

```
🚀 TaskListProcessor - Quick Start Demo

⏳ Processing tasks...

📊 Results:

✅ Check System Status   (200ms)
   Data: All systems operational
✅ Get Stock Price       (300ms)
   Data: $150.25
✅ Fetch Weather         (500ms)
   Data: Sunny, 72°F
✅ Load User Data        (700ms)
   Data: { Name = John Doe, Email = john@example.com }

📈 Summary:
   Total Tasks: 4
   Successful: 4
   Failed: 0
   Total Time: 700ms

✨ Demo complete!
```

---

## What Just Happened? 🎯

Congratulations! You just:

1. **Defined 4 tasks** that would normally take 1,700ms (500+300+700+200) if run sequentially
2. **Executed them concurrently** - completed in just ~700ms (the longest task)
3. **Got automatic error handling** - any failed task wouldn't crash the others
4. **Received telemetry** - execution times, success/failure status
5. **Typed-safe results** - strongly-typed result objects

---

## Understanding the Code

### The Processor
```csharp
using var processor = new TaskListProcessorEnhanced("Quick Demo", logger);
```
- Creates a processor instance with a name and logger
- `using` ensures proper cleanup

### Task Definition
```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Task Name"] = async ct => { /* your async code */ }
};
```
- Dictionary key = task name
- Value = async function that returns `Task<object?>`
- `CancellationToken` enables graceful cancellation

### Execution
```csharp
await processor.ProcessTasksAsync(tasks);
```
- Executes all tasks concurrently
- Handles errors automatically
- Collects telemetry

### Results
```csharp
foreach (var result in processor.TaskResults)
{
    Console.WriteLine(result.Name);        // Task name
    Console.WriteLine(result.IsSuccessful); // Success status
    Console.WriteLine(result.Data);         // Result data
    Console.WriteLine(result.ErrorMessage); // Error (if failed)
}
```

---

## Try These Experiments 🧪

### 1. Add Error Handling

Modify one task to throw an exception:

```csharp
["Failing Task"] = async ct =>
{
    await Task.Delay(100, ct);
    throw new InvalidOperationException("Something went wrong!");
}
```

**Result**: Other tasks continue to completion, error is captured in results.

### 2. Add More Tasks

Add 10 more tasks and see how concurrent execution scales:

```csharp
for (int i = 1; i <= 10; i++)
{
    int taskNum = i;
    tasks[$"Task {taskNum}"] = async ct =>
    {
        await Task.Delay(Random.Shared.Next(100, 1000), ct);
        return $"Result from task {taskNum}";
    };
}
```

### 3. Add Cancellation

Add a timeout to see cancellation in action:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
await processor.ProcessTasksAsync(tasks, cancellationToken: cts.Token);
```

### 4. Add Progress Reporting

Track progress in real-time:

```csharp
var progress = new Progress<TaskProgress>(p =>
{
    Console.WriteLine($"Progress: {p.CompletedTasks}/{p.TotalTasks} - {p.CurrentTask}");
});

await processor.ProcessTasksAsync(tasks, progress);
```

---

## Next Steps 🚀

Now that you've got the basics, explore more advanced features:

### **Continue Learning**
- 📖 [Fundamentals Guide](02-fundamentals.md) - Core concepts explained
- 🎓 [Your First Real Processor](03-your-first-processor.md) - Build a production-ready example
- ⚠️ [Common Pitfalls](04-common-pitfalls.md) - Avoid common mistakes

### **Beginner Tutorials**
- [Simple Task Execution](../tutorials/beginner/01-simple-task-execution.md)
- [Batch Processing](../tutorials/beginner/02-batch-processing.md)
- [Error Handling](../tutorials/beginner/03-error-handling.md)
- [Progress Reporting](../tutorials/beginner/04-progress-reporting.md)
- [Basic Telemetry](../tutorials/beginner/05-basic-telemetry.md)

### **Real-World Examples**
- [Travel Dashboard](../examples/real-world-scenarios/travel-dashboard.md) - Multi-city data aggregation
- [API Aggregation](../examples/real-world-scenarios/api-aggregation.md) - Combining multiple APIs
- [Batch Processing](../examples/real-world-scenarios/batch-data-processing.md) - Large dataset processing

### **Advanced Features**
- [Dependency Injection](../tutorials/intermediate/01-dependency-injection.md)
- [Circuit Breaker Pattern](../tutorials/intermediate/02-circuit-breaker-pattern.md)
- [Advanced Scheduling](../tutorials/intermediate/03-advanced-scheduling.md)

---

## Common Issues & Solutions

### Issue: "The type or namespace name 'TaskListProcessing' could not be found"

**Solution**: Ensure you've added the project reference correctly:
```bash
dotnet add reference path/to/TaskListProcessing/TaskListProcessing.csproj
dotnet restore
```

### Issue: Tasks not running concurrently

**Solution**: Make sure you're using `await` inside your task functions:
```csharp
// ✅ Correct - truly async
["Task"] = async ct => await SomeAsyncMethod(ct)

// ❌ Wrong - blocks thread
["Task"] = async ct => SomeAsyncMethod(ct).Result
```

### Issue: Application hangs

**Solution**: Check for deadlocks. Always use `ConfigureAwait(false)` or ensure proper async context:
```csharp
await SomeAsyncMethod().ConfigureAwait(false);
```

---

## Get Help & Support

- 📖 [Full Documentation](../../README.md)
- ❓ [FAQ](../troubleshooting/faq.md)
- 🐛 [Report Issues](https://github.com/markhazleton/TaskListProcessor/issues)
- 💬 [GitHub Discussions](https://github.com/markhazleton/TaskListProcessor/discussions)

---

## Summary

In just 5 minutes, you learned how to:
- ✅ Set up TaskListProcessor
- ✅ Define and execute concurrent tasks
- ✅ Handle results and errors
- ✅ View telemetry and performance metrics

**Total code**: ~40 lines
**Concurrent execution**: ✅
**Error handling**: ✅
**Telemetry**: ✅
**Production-ready**: Ready to extend!

---

**Ready for more?** Continue to [Fundamentals Guide](02-fundamentals.md) to dive deeper into core concepts!

---

*Built with ❤️ by [Mark Hazleton](https://markhazleton.com)*
