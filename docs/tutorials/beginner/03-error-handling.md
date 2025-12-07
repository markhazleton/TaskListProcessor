# Tutorial 3: Error Handling

**Level**: Beginner
**Duration**: 25 minutes
**Prerequisites**:
- Completed [Tutorial 1: Simple Task Execution](01-simple-task-execution.md)
- Completed [Tutorial 2: Batch Processing](02-batch-processing.md)

---

## 🎯 What You'll Learn

By the end of this tutorial, you'll be able to:

- ✅ Handle task failures gracefully
- ✅ Understand error isolation
- ✅ Implement retry logic
- ✅ Use try-catch patterns effectively
- ✅ Log and diagnose errors
- ✅ Recover from partial failures

---

## 📚 Why Error Handling Matters

In real-world applications, **failures are inevitable**:

- 🌐 Network requests time out
- 📊 APIs return errors
- 💾 Databases become unavailable
- 🔒 Resources get exhausted

**Without proper error handling**:
- ❌ One task failure crashes entire application
- ❌ No visibility into what went wrong
- ❌ No recovery mechanism
- ❌ Lost data and incomplete processing

**With TaskListProcessor**:
- ✅ Automatic error isolation per task
- ✅ Detailed exception information
- ✅ Partial success scenarios
- ✅ Retry capabilities

---

## 🛡️ Built-in Error Isolation

TaskListProcessor automatically isolates errors. One task failing doesn't affect others:

```csharp
using Microsoft.Extensions.Logging;
using TaskListProcessor;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

using var processor = new TaskListProcessorEnhanced("Error Demo", logger);

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["SuccessTask1"] = async ct =>
    {
        await Task.Delay(100, ct);
        return "✅ Task 1 succeeded!";
    },

    ["FailingTask"] = async ct =>
    {
        await Task.Delay(100, ct);
        throw new Exception("💥 This task failed!");
    },

    ["SuccessTask2"] = async ct =>
    {
        await Task.Delay(100, ct);
        return "✅ Task 2 succeeded!";
    }
};

var results = await processor.ProcessTasksAsync(tasks);

// Check results
foreach (var result in results)
{
    Console.WriteLine($"\nTask: {result.Key}");
    Console.WriteLine($"  Success: {result.Value.IsSuccess}");

    if (result.Value.IsSuccess)
    {
        Console.WriteLine($"  Result: {result.Value.Result}");
    }
    else
    {
        Console.WriteLine($"  Error: {result.Value.Exception?.Message}");
    }
}

// Summary
var successful = results.Values.Count(r => r.IsSuccess);
var failed = results.Values.Count(r => !r.IsSuccess);

Console.WriteLine($"\n📊 Summary: {successful} succeeded, {failed} failed");
Console.WriteLine("✅ Application continued running despite the error!");
```

**Output:**
```
Task: SuccessTask1
  Success: True
  Result: ✅ Task 1 succeeded!

Task: FailingTask
  Success: False
  Error: 💥 This task failed!

Task: SuccessTask2
  Success: True
  Result: ✅ Task 2 succeeded!

📊 Summary: 2 succeeded, 1 failed
✅ Application continued running despite the error!
```

**Key takeaway**: TaskListProcessor caught the exception, isolated it to that task, and allowed other tasks to complete successfully.

---

## 🔍 Understanding Exception Details

Each task result contains detailed exception information:

```csharp
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["NetworkError"] = async ct =>
    {
        await Task.Delay(50, ct);
        throw new HttpRequestException("Connection timeout after 30s");
    },

    ["ValidationError"] = async ct =>
    {
        await Task.Delay(50, ct);
        throw new ArgumentException("Invalid email format", "email");
    },

    ["DatabaseError"] = async ct =>
    {
        await Task.Delay(50, ct);
        throw new InvalidOperationException("Database connection failed");
    }
};

using var processor = new TaskListProcessorEnhanced("Exception Details Demo", logger);
var results = await processor.ProcessTasksAsync(tasks);

foreach (var result in results.Where(r => !r.Value.IsSuccess))
{
    var ex = result.Value.Exception;

    Console.WriteLine($"\n❌ Task '{result.Key}' failed:");
    Console.WriteLine($"   Exception Type: {ex?.GetType().Name}");
    Console.WriteLine($"   Message: {ex?.Message}");
    Console.WriteLine($"   Stack Trace: {ex?.StackTrace?[..100]}..."); // First 100 chars

    // Handle specific exception types
    switch (ex)
    {
        case HttpRequestException httpEx:
            Console.WriteLine("   → Retry with exponential backoff");
            break;

        case ArgumentException argEx:
            Console.WriteLine($"   → Fix parameter: {argEx.ParamName}");
            break;

        case InvalidOperationException opEx:
            Console.WriteLine("   → Check database connection");
            break;
    }
}
```

**Output:**
```
❌ Task 'NetworkError' failed:
   Exception Type: HttpRequestException
   Message: Connection timeout after 30s
   Stack Trace: at Program.<>c...
   → Retry with exponential backoff

❌ Task 'ValidationError' failed:
   Exception Type: ArgumentException
   Message: Invalid email format (Parameter 'email')
   Stack Trace: at Program.<>c...
   → Fix parameter: email

❌ Task 'DatabaseError' failed:
   Exception Type: InvalidOperationException
   Message: Database connection failed
   Stack Trace: at Program.<>c...
   → Check database connection
```

---

## 🔄 Retry Patterns

### Pattern 1: Simple Retry

Retry a task a fixed number of times:

```csharp
async Task<object?> ExecuteWithRetry(
    Func<CancellationToken, Task<object?>> taskFactory,
    int maxAttempts,
    CancellationToken ct)
{
    Exception? lastException = null;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            Console.WriteLine($"  Attempt {attempt}/{maxAttempts}...");
            return await taskFactory(ct);
        }
        catch (Exception ex)
        {
            lastException = ex;
            Console.WriteLine($"  ❌ Attempt {attempt} failed: {ex.Message}");

            if (attempt < maxAttempts)
            {
                await Task.Delay(100, ct); // Small delay before retry
            }
        }
    }

    throw lastException ?? new Exception("All retry attempts failed");
}

// Usage
var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["UnreliableAPI"] = async ct =>
    {
        return await ExecuteWithRetry(async ct =>
        {
            // Simulate 70% failure rate
            if (Random.Shared.Next(100) < 70)
                throw new HttpRequestException("API temporarily unavailable");

            return "API data retrieved";
        }, maxAttempts: 3, ct);
    }
};

using var processor = new TaskListProcessorEnhanced("Retry Demo", logger);
var results = await processor.ProcessTasksAsync(tasks);

foreach (var result in results)
{
    Console.WriteLine($"\nTask '{result.Key}': {(result.Value.IsSuccess ? "✅ Success" : "❌ Failed")}");
    if (result.Value.IsSuccess)
        Console.WriteLine($"  Result: {result.Value.Result}");
}
```

**Output (example):**
```
  Attempt 1/3...
  ❌ Attempt 1 failed: API temporarily unavailable
  Attempt 2/3...
  ❌ Attempt 2 failed: API temporarily unavailable
  Attempt 3/3...

Task 'UnreliableAPI': ✅ Success
  Result: API data retrieved
```

---

### Pattern 2: Exponential Backoff

Increase delay between retries exponentially:

```csharp
async Task<object?> ExecuteWithExponentialBackoff(
    Func<CancellationToken, Task<object?>> taskFactory,
    int maxAttempts,
    CancellationToken ct)
{
    Exception? lastException = null;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await taskFactory(ct);
        }
        catch (Exception ex)
        {
            lastException = ex;

            if (attempt < maxAttempts)
            {
                var delayMs = (int)Math.Pow(2, attempt) * 100; // 200, 400, 800, 1600...
                Console.WriteLine($"  Retry {attempt} failed. Waiting {delayMs}ms before retry...");
                await Task.Delay(delayMs, ct);
            }
        }
    }

    throw lastException ?? new Exception("All retry attempts failed");
}

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["ExternalService"] = async ct =>
    {
        return await ExecuteWithExponentialBackoff(async ct =>
        {
            // Simulate intermittent failures
            if (Random.Shared.Next(100) < 60)
                throw new Exception("Service overloaded");

            return "Service responded";
        }, maxAttempts: 4, ct);
    }
};

using var processor = new TaskListProcessorEnhanced("Exponential Backoff Demo", logger);
var results = await processor.ProcessTasksAsync(tasks);
```

**Why exponential backoff?**
- Gives failing services time to recover
- Reduces load on struggling systems
- Industry-standard retry pattern
- Used by AWS, Azure, Google Cloud

---

### Pattern 3: Retry with Different Strategies

Different error types need different retry strategies:

```csharp
async Task<object?> SmartRetry(
    Func<CancellationToken, Task<object?>> taskFactory,
    CancellationToken ct)
{
    const int maxAttempts = 3;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await taskFactory(ct);
        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("timeout"))
        {
            // Network timeouts: retry with longer timeout
            Console.WriteLine($"  Timeout on attempt {attempt}. Retrying...");
            await Task.Delay(1000 * attempt, ct);
        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("500") ||
                                                    httpEx.Message.Contains("503"))
        {
            // Server errors: retry with exponential backoff
            Console.WriteLine($"  Server error on attempt {attempt}. Backing off...");
            await Task.Delay((int)Math.Pow(2, attempt) * 500, ct);
        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("401") ||
                                                    httpEx.Message.Contains("403"))
        {
            // Auth errors: don't retry
            Console.WriteLine("  Authentication error. Not retrying.");
            throw;
        }
        catch (ArgumentException)
        {
            // Validation errors: don't retry
            Console.WriteLine("  Validation error. Not retrying.");
            throw;
        }
    }

    throw new Exception($"Failed after {maxAttempts} attempts");
}
```

---

## 🏥 Fallback Strategies

When retries fail, provide fallback values:

```csharp
async Task<object?> ExecuteWithFallback(
    Func<CancellationToken, Task<object?>> primaryTask,
    Func<CancellationToken, Task<object?>> fallbackTask,
    CancellationToken ct)
{
    try
    {
        return await primaryTask(ct);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Primary task failed: {ex.Message}");
        Console.WriteLine("  Attempting fallback...");

        try
        {
            return await fallbackTask(ct);
        }
        catch (Exception fallbackEx)
        {
            Console.WriteLine($"  Fallback also failed: {fallbackEx.Message}");
            throw new AggregateException("Both primary and fallback failed", ex, fallbackEx);
        }
    }
}

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["WeatherData"] = async ct =>
    {
        return await ExecuteWithFallback(
            // Primary: External API
            primaryTask: async ct =>
            {
                // Simulate API failure
                throw new HttpRequestException("Weather API down");
            },
            // Fallback: Cached data
            fallbackTask: async ct =>
            {
                await Task.Delay(50, ct);
                return new { Temperature = 72, Source = "Cache", Timestamp = DateTime.UtcNow.AddHours(-1) };
            },
            ct
        );
    }
};

using var processor = new TaskListProcessorEnhanced("Fallback Demo", logger);
var results = await processor.ProcessTasksAsync(tasks);

foreach (var result in results)
{
    if (result.Value.IsSuccess)
    {
        Console.WriteLine($"\n✅ {result.Key}: {result.Value.Result}");
    }
}
```

**Output:**
```
  Primary task failed: Weather API down
  Attempting fallback...

✅ WeatherData: { Temperature = 72, Source = Cache, Timestamp = 12/7/2025 1:30:45 PM }
```

---

## 📝 Logging Errors Effectively

Good error logging helps diagnose issues:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["ProcessOrder"] = async ct =>
    {
        try
        {
            logger.LogInformation("Starting order processing...");

            // Simulate work
            await Task.Delay(100, ct);

            // Simulate error
            throw new InvalidOperationException("Payment gateway timeout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Order processing failed. OrderId: {OrderId}, CustomerId: {CustomerId}",
                123, 456);
            throw; // Re-throw for TaskListProcessor to handle
        }
    }
};

using var processor = new TaskListProcessorEnhanced("Order Processor", logger);
var results = await processor.ProcessTasksAsync(tasks);

// Additional error reporting
foreach (var result in results.Where(r => !r.Value.IsSuccess))
{
    logger.LogWarning("Task '{TaskName}' failed with {ExceptionType}: {Message}",
        result.Key,
        result.Value.Exception?.GetType().Name,
        result.Value.Exception?.Message);

    // Could also send to error tracking service
    // await ErrorTracker.ReportAsync(result.Value.Exception);
}
```

**Best practices for logging**:
- ✅ Log at appropriate levels (Debug, Info, Warning, Error)
- ✅ Include contextual data (IDs, timestamps, parameters)
- ✅ Use structured logging with named parameters
- ✅ Don't log sensitive data (passwords, credit cards)
- ✅ Include correlation IDs for distributed tracing

---

## 🧩 Practice Exercises

### Exercise 1: Resilient File Processor

Create a file processor that handles missing files gracefully:

```csharp
var files = new[] { "file1.txt", "missing.txt", "file2.txt" };

var tasks = files.ToDictionary(
    file => file,
    file => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        try
        {
            // TODO:
            // 1. Check if file exists
            // 2. If missing, return error result
            // 3. If exists, simulate reading (Task.Delay)
            // 4. Return file content
            throw new NotImplementedException();
        }
        catch (FileNotFoundException ex)
        {
            // TODO: Log and return graceful error
            throw new NotImplementedException();
        }
    })
);

// Process and report which files succeeded/failed
```

### Exercise 2: API with Retry Logic

Implement retry logic for unreliable API:

```csharp
async Task<object?> CallApiWithRetry(string endpoint, CancellationToken ct)
{
    // TODO:
    // 1. Attempt API call (simulate with 50% failure rate)
    // 2. Retry up to 3 times with exponential backoff
    // 3. Log each attempt
    // 4. Return data or throw after max retries
    throw new NotImplementedException();
}
```

### Exercise 3: Partial Failure Recovery

Process a batch where some items fail:

```csharp
var items = Enumerable.Range(1, 100).ToList();

// TODO:
// 1. Process all items
// 2. Identify failed items
// 3. Retry only failed items (max 2 retries)
// 4. Report final success/failure count
// 5. Save failed items to "failed.txt" for manual review
```

---

## ⚠️ Common Mistakes

### 1. **Swallowing Exceptions**

❌ **Wrong**:
```csharp
["BadTask"] = async ct =>
{
    try
    {
        await RiskyOperation();
        return "Success";
    }
    catch
    {
        return "Success"; // Lying about failure!
    }
}
```

✅ **Correct**:
```csharp
["GoodTask"] = async ct =>
{
    try
    {
        await RiskyOperation();
        return "Success";
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Operation failed");
        throw; // Let TaskListProcessor handle it
    }
}
```

### 2. **Infinite Retries**

❌ **Wrong**:
```csharp
while (true) // Never stops!
{
    try
    {
        return await operation();
    }
    catch
    {
        await Task.Delay(1000);
    }
}
```

✅ **Correct**:
```csharp
const int maxRetries = 3;
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        return await operation();
    }
    catch
    {
        if (i == maxRetries - 1) throw;
        await Task.Delay(1000);
    }
}
```

### 3. **Not Checking CancellationToken**

❌ **Wrong**:
```csharp
for (int i = 0; i < 100; i++)
{
    await Task.Delay(1000); // Ignores cancellation!
}
```

✅ **Correct**:
```csharp
for (int i = 0; i < 100; i++)
{
    ct.ThrowIfCancellationRequested();
    await Task.Delay(1000, ct);
}
```

### 4. **Generic Error Messages**

❌ **Wrong**:
```csharp
throw new Exception("Error"); // Useless!
```

✅ **Correct**:
```csharp
throw new InvalidOperationException(
    $"Failed to process order {orderId} for customer {customerId}: Payment gateway timeout after 30s");
```

---

## 🎓 Complete Example: Resilient Data Processor

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TaskListProcessor;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

// Simulate processing 50 records with 20% failure rate
var records = Enumerable.Range(1, 50).ToList();

var tasks = records.ToDictionary(
    id => $"Record-{id}",
    id => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        return await ExecuteWithRetryAndFallback(
            taskName: $"Record-{id}",
            operation: async ct =>
            {
                await Task.Delay(Random.Shared.Next(50, 150), ct);

                // Simulate 20% failure rate
                if (Random.Shared.Next(100) < 20)
                    throw new Exception("Database timeout");

                return new { RecordId = id, Processed = true, Timestamp = DateTime.UtcNow };
            },
            fallback: async ct =>
            {
                // Fallback: queue for later
                await Task.Delay(10, ct);
                return new { RecordId = id, Queued = true, Timestamp = DateTime.UtcNow };
            },
            ct
        );
    })
);

using var processor = new TaskListProcessorEnhanced("Resilient Processor", logger);
var sw = Stopwatch.StartNew();
var results = await processor.ProcessTasksAsync(tasks);
sw.Stop();

// Analyze results
var processed = results.Values.Count(r => r.IsSuccess && r.Result?.ToString()?.Contains("Processed") == true);
var queued = results.Values.Count(r => r.IsSuccess && r.Result?.ToString()?.Contains("Queued") == true);
var failed = results.Values.Count(r => !r.IsSuccess);

Console.WriteLine($"\n📊 Results:");
Console.WriteLine($"   Processed: {processed}");
Console.WriteLine($"   Queued for retry: {queued}");
Console.WriteLine($"   Failed: {failed}");
Console.WriteLine($"   Duration: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Success rate: {(double)(processed + queued) / records.Count * 100:F1}%");

// Helper method
async Task<object?> ExecuteWithRetryAndFallback(
    string taskName,
    Func<CancellationToken, Task<object?>> operation,
    Func<CancellationToken, Task<object?>> fallback,
    CancellationToken ct)
{
    const int maxAttempts = 3;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation(ct);
        }
        catch (Exception ex)
        {
            if (attempt < maxAttempts)
            {
                var delay = (int)Math.Pow(2, attempt) * 100;
                logger.LogWarning("{Task} attempt {Attempt} failed: {Error}. Retrying in {Delay}ms...",
                    taskName, attempt, ex.Message, delay);
                await Task.Delay(delay, ct);
            }
            else
            {
                logger.LogWarning("{Task} failed after {Attempts} attempts. Using fallback.",
                    taskName, maxAttempts);
                return await fallback(ct);
            }
        }
    }

    throw new InvalidOperationException("Should not reach here");
}
```

---

## 🎯 Key Takeaways

✅ **Error isolation** prevents cascading failures
✅ **Retry logic** handles transient errors
✅ **Exponential backoff** gives systems time to recover
✅ **Fallbacks** provide graceful degradation
✅ **Detailed logging** enables diagnosis
✅ **Exception details** available in results

---

## 📚 What's Next?

**Next Tutorial**: [04-progress-reporting.md](04-progress-reporting.md)
Learn how to track and report progress in real-time.

**Related Reading**:
- [Common Pitfalls](../../getting-started/04-common-pitfalls.md) - Error handling mistakes
- [FAQ: Error Handling](../../troubleshooting/faq.md#error-handling)

---

**Completed**: Tutorial 3 - Error Handling ✅
**Next**: Tutorial 4 - Progress Reporting →
