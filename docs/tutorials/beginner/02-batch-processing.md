# Tutorial 2: Batch Processing

**Level**: Beginner
**Duration**: 20 minutes
**Prerequisites**:
- Completed [Tutorial 1: Simple Task Execution](01-simple-task-execution.md)
- Understanding of collections in C#

---

## 🎯 What You'll Learn

By the end of this tutorial, you'll be able to:

- ✅ Process large collections efficiently
- ✅ Understand batch processing strategies
- ✅ Control concurrency to prevent overload
- ✅ Implement different batching patterns
- ✅ Measure performance improvements

---

## 📚 Why Batch Processing?

Imagine you need to process 10,000 user records. If you try to process all of them simultaneously, you'll:

- ❌ Overwhelm your system (too many concurrent operations)
- ❌ Exceed API rate limits
- ❌ Run out of memory
- ❌ Cause timeouts and errors

**Batch processing** solves this by:

- ✅ Processing items in manageable chunks
- ✅ Controlling concurrency limits
- ✅ Providing better progress tracking
- ✅ Improving overall stability

---

## 🔨 Batch Processing Patterns

### Pattern 1: Simple Chunking

Process a large collection in fixed-size batches.

**Use Case**: Processing 1,000 customer orders in batches of 100.

```csharp
using Microsoft.Extensions.Logging;
using TaskListProcessor;

// Create logger and processor
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

using var processor = new TaskListProcessorEnhanced(
    name: "Order Processor",
    logger: logger
);

// Sample data: 1,000 orders
var orders = Enumerable.Range(1, 1000)
    .Select(i => new Order
    {
        Id = i,
        CustomerId = i % 100,
        Amount = Random.Shared.Next(10, 500)
    })
    .ToList();

Console.WriteLine($"Processing {orders.Count} orders...");

// Batch size
const int batchSize = 100;
var batches = orders
    .Select((order, index) => new { order, index })
    .GroupBy(x => x.index / batchSize)
    .Select(g => g.Select(x => x.order).ToList())
    .ToList();

Console.WriteLine($"Created {batches.Count} batches of size {batchSize}");

// Process each batch
var batchNumber = 0;
foreach (var batch in batches)
{
    batchNumber++;
    Console.WriteLine($"\nProcessing batch {batchNumber}/{batches.Count}...");

    // Create tasks for this batch
    var tasks = batch.ToDictionary(
        order => $"Order-{order.Id}",
        order => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(Random.Shared.Next(10, 50), ct);
            // Simulate order processing
            return new { OrderId = order.Id, Status = "Processed", ProcessedAt = DateTime.UtcNow };
        })
    );

    // Process batch
    var results = await processor.ProcessTasksAsync(tasks);

    var successful = results.Values.Count(r => r.IsSuccess);
    Console.WriteLine($"✅ Batch {batchNumber} complete: {successful}/{batch.Count} successful");
}

Console.WriteLine($"\n🎉 All {orders.Count} orders processed!");

// Order class definition
public record Order
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public decimal Amount { get; init; }
}
```

**Output:**
```
Processing 1000 orders...
Created 10 batches of size 100

Processing batch 1/10...
✅ Batch 1 complete: 100/100 successful

Processing batch 2/10...
✅ Batch 2 complete: 100/100 successful

...

🎉 All 1000 orders processed!
```

---

### Pattern 2: Controlled Concurrency

Process items with a maximum concurrency limit (like a throttle).

**Use Case**: API calls with rate limit of 10 requests/second.

```csharp
using System.Collections.Concurrent;

var urls = Enumerable.Range(1, 100)
    .Select(i => $"https://api.example.com/data/{i}")
    .ToList();

Console.WriteLine($"Fetching {urls.Count} URLs with max 10 concurrent requests...");

const int maxConcurrency = 10;
var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
var results = new ConcurrentBag<string>();

var tasks = urls.ToDictionary(
    url => url,
    url => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        await semaphore.WaitAsync(ct); // Limit concurrency
        try
        {
            // Simulate HTTP request
            await Task.Delay(Random.Shared.Next(100, 500), ct);
            var data = $"Data from {url}";
            results.Add(data);
            return data;
        }
        finally
        {
            semaphore.Release(); // Release slot
        }
    })
);

using var processor = new TaskListProcessorEnhanced("API Fetcher", logger);
await processor.ProcessTasksAsync(tasks);

Console.WriteLine($"✅ Fetched {results.Count} URLs");
```

**Why this works**:
- `SemaphoreSlim` limits concurrent execution to 10
- Tasks wait their turn before executing
- Prevents overwhelming the API
- Maintains steady throughput

---

### Pattern 3: Time-Based Batching

Process items in batches with delays between batches.

**Use Case**: Sending emails without triggering spam filters.

```csharp
var emails = Enumerable.Range(1, 500)
    .Select(i => new Email
    {
        To = $"user{i}@example.com",
        Subject = "Important Update",
        Body = $"Hello user {i}!"
    })
    .ToList();

const int batchSize = 50;
const int delayBetweenBatchesMs = 2000; // 2 seconds

var batches = emails
    .Select((email, index) => new { email, index })
    .GroupBy(x => x.index / batchSize)
    .Select(g => g.Select(x => x.email).ToList())
    .ToList();

Console.WriteLine($"Sending {emails.Count} emails in {batches.Count} batches...");
Console.WriteLine($"Delay between batches: {delayBetweenBatchesMs}ms\n");

using var processor = new TaskListProcessorEnhanced("Email Sender", logger);

for (int i = 0; i < batches.Count; i++)
{
    var batch = batches[i];
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending batch {i + 1}/{batches.Count}...");

    var tasks = batch.ToDictionary(
        email => email.To,
        email => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(Random.Shared.Next(50, 150), ct);
            // Simulate email sending
            return new { Email = email.To, Sent = true, SentAt = DateTime.UtcNow };
        })
    );

    var results = await processor.ProcessTasksAsync(tasks);
    var successful = results.Values.Count(r => r.IsSuccess);

    Console.WriteLine($"✅ Batch {i + 1} complete: {successful}/{batch.Count} sent");

    // Wait before next batch (except last batch)
    if (i < batches.Count - 1)
    {
        Console.WriteLine($"⏳ Waiting {delayBetweenBatchesMs}ms before next batch...\n");
        await Task.Delay(delayBetweenBatchesMs);
    }
}

Console.WriteLine($"\n🎉 All {emails.Count} emails sent!");

public record Email
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
}
```

**Output:**
```
Sending 500 emails in 10 batches...
Delay between batches: 2000ms

[14:30:00] Sending batch 1/10...
✅ Batch 1 complete: 50/50 sent
⏳ Waiting 2000ms before next batch...

[14:30:02] Sending batch 2/10...
✅ Batch 2 complete: 50/50 sent
⏳ Waiting 2000ms before next batch...

...

🎉 All 500 emails sent!
```

---

### Pattern 4: Dynamic Batch Sizing

Adjust batch size based on performance or errors.

**Use Case**: Database imports where batch size depends on record complexity.

```csharp
var records = Enumerable.Range(1, 1000)
    .Select(i => new DatabaseRecord { Id = i, Data = $"Record {i}" })
    .ToList();

using var processor = new TaskListProcessorEnhanced("DB Importer", logger);

int batchSize = 100; // Start with 100
int minBatchSize = 10;
int maxBatchSize = 200;
int processedCount = 0;

while (processedCount < records.Count)
{
    var batch = records.Skip(processedCount).Take(batchSize).ToList();

    Console.WriteLine($"\nProcessing batch of {batch.Count} records (total: {processedCount}/{records.Count})...");

    var tasks = batch.ToDictionary(
        record => $"Record-{record.Id}",
        record => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(Random.Shared.Next(10, 100), ct);

            // Simulate occasional errors
            if (Random.Shared.Next(100) < 5) // 5% error rate
            {
                throw new Exception("Database timeout");
            }

            return new { RecordId = record.Id, Imported = true };
        })
    );

    var startTime = DateTime.UtcNow;
    var results = await processor.ProcessTasksAsync(tasks);
    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

    var successful = results.Values.Count(r => r.IsSuccess);
    var failed = results.Values.Count(r => !r.IsSuccess);
    var successRate = (double)successful / batch.Count * 100;

    Console.WriteLine($"✅ {successful}/{batch.Count} successful ({successRate:F1}%)");
    Console.WriteLine($"⏱️  Duration: {duration:F0}ms");

    // Adjust batch size based on performance
    if (successRate >= 95 && duration < 1000)
    {
        // Increase batch size if performing well
        batchSize = Math.Min(batchSize + 20, maxBatchSize);
        Console.WriteLine($"📈 Increasing batch size to {batchSize}");
    }
    else if (successRate < 80 || duration > 2000)
    {
        // Decrease batch size if errors or slow
        batchSize = Math.Max(batchSize - 20, minBatchSize);
        Console.WriteLine($"📉 Decreasing batch size to {batchSize}");
    }

    processedCount += batch.Count;
}

Console.WriteLine($"\n🎉 Import complete: {processedCount} records processed");

public record DatabaseRecord
{
    public required int Id { get; init; }
    public required string Data { get; init; }
}
```

**Smart batching benefits**:
- Starts with reasonable default
- Increases size when things go well
- Decreases size when errors occur
- Self-optimizing based on conditions

---

## 📊 Performance Comparison

Let's prove batch processing is faster and safer:

```csharp
using System.Diagnostics;

var itemCount = 1000;
var items = Enumerable.Range(1, itemCount).ToList();

Console.WriteLine($"Processing {itemCount} items...\n");

// Strategy 1: All at once (dangerous!)
Console.WriteLine("=== Strategy 1: All At Once ===");
var sw1 = Stopwatch.StartNew();
try
{
    var allTasks = items.ToDictionary(
        i => $"Item-{i}",
        i => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(10, ct);
            return i * 2;
        })
    );

    using var processor1 = new TaskListProcessorEnhanced("All At Once", logger);
    await processor1.ProcessTasksAsync(allTasks);

    sw1.Stop();
    Console.WriteLine($"✅ Completed in {sw1.ElapsedMilliseconds}ms");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Failed: {ex.Message}");
}

Console.WriteLine();

// Strategy 2: Batched (safe!)
Console.WriteLine("=== Strategy 2: Batched (100 per batch) ===");
var sw2 = Stopwatch.StartNew();

const int batchSize = 100;
var batches = items
    .Select((item, index) => new { item, index })
    .GroupBy(x => x.index / batchSize)
    .Select(g => g.Select(x => x.item).ToList())
    .ToList();

using var processor2 = new TaskListProcessorEnhanced("Batched", logger);

foreach (var batch in batches)
{
    var batchTasks = batch.ToDictionary(
        i => $"Item-{i}",
        i => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(10, ct);
            return i * 2;
        })
    );

    await processor2.ProcessTasksAsync(batchTasks);
}

sw2.Stop();
Console.WriteLine($"✅ Completed in {sw2.ElapsedMilliseconds}ms");
Console.WriteLine($"📊 Overhead: {sw2.ElapsedMilliseconds - sw1.ElapsedMilliseconds}ms ({(double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds:F2}x)");
Console.WriteLine("\n💡 Batching adds small overhead but provides:");
Console.WriteLine("   - Better memory management");
Console.WriteLine("   - Progress tracking");
Console.WriteLine("   - Error isolation per batch");
Console.WriteLine("   - Resource control");
```

---

## 🧩 Practice Exercises

### Exercise 1: CSV Import

Process a large CSV file in batches:

```csharp
// Simulate reading 5,000 CSV rows
var csvRows = Enumerable.Range(1, 5000)
    .Select(i => $"{i},Product{i},{Random.Shared.Next(10, 500)}")
    .ToList();

// TODO:
// 1. Split into batches of 500
// 2. Parse each row
// 3. Validate data
// 4. Insert into "database" (simulated)
// 5. Track success/failure counts per batch
```

### Exercise 2: Image Processing

Process a directory of images in batches:

```csharp
// Simulate 1,000 images
var images = Enumerable.Range(1, 1000)
    .Select(i => $"image{i}.jpg")
    .ToList();

// TODO:
// 1. Process in batches of 50
// 2. Simulate resize operation (Task.Delay)
// 3. Track total processing time
// 4. Calculate images/second throughput
```

### Exercise 3: API Rate Limiting

Fetch data from an API with rate limits:

```csharp
// 100 API endpoints to call
// Rate limit: 10 requests per second

// TODO:
// 1. Use SemaphoreSlim for concurrency control
// 2. Measure actual requests/second
// 3. Ensure rate limit compliance
```

---

## ⚠️ Common Mistakes

### 1. **Batch Too Large**

❌ **Wrong**:
```csharp
const int batchSize = 10000; // Too large!
// May cause memory issues or timeouts
```

✅ **Correct**:
```csharp
const int batchSize = 100; // Reasonable
// Test and adjust based on your scenario
```

### 2. **No Concurrency Control**

❌ **Wrong**:
```csharp
// All tasks run simultaneously - may overwhelm resources
var tasks = hugeList.ToDictionary(/*...*/);
await processor.ProcessTasksAsync(tasks);
```

✅ **Correct**:
```csharp
// Controlled batches
foreach (var batch in batches)
{
    var batchTasks = batch.ToDictionary(/*...*/);
    await processor.ProcessTasksAsync(batchTasks);
}
```

### 3. **Ignoring Errors Between Batches**

❌ **Wrong**:
```csharp
foreach (var batch in batches)
{
    await processor.ProcessTasksAsync(batchTasks);
    // What if this batch failed? Continue anyway?
}
```

✅ **Correct**:
```csharp
var totalFailed = 0;
foreach (var batch in batches)
{
    var results = await processor.ProcessTasksAsync(batchTasks);
    var failed = results.Values.Count(r => !r.IsSuccess);
    totalFailed += failed;

    if (failed > batch.Count * 0.5) // More than 50% failed
    {
        Console.WriteLine("⚠️  Too many failures, stopping...");
        break;
    }
}
```

### 4. **Not Measuring Performance**

❌ **Wrong**:
```csharp
// Just hope it's fast enough
await ProcessBatchesAsync();
```

✅ **Correct**:
```csharp
var sw = Stopwatch.StartNew();
await ProcessBatchesAsync();
sw.Stop();

var itemsPerSecond = totalItems / (sw.ElapsedMilliseconds / 1000.0);
Console.WriteLine($"Throughput: {itemsPerSecond:F0} items/second");
```

---

## 🎯 Best Practices

### 1. **Choose Appropriate Batch Size**

| Data Volume | Recommended Batch Size |
|-------------|------------------------|
| < 100 items | No batching needed |
| 100-1,000 | 50-100 per batch |
| 1,000-10,000 | 100-500 per batch |
| 10,000+ | 500-1,000 per batch |

**Factors to consider**:
- Item processing time
- Memory per item
- API rate limits
- Database connection limits

### 2. **Implement Progress Tracking**

```csharp
var totalItems = items.Count;
var processedItems = 0;

foreach (var batch in batches)
{
    await processor.ProcessTasksAsync(batchTasks);
    processedItems += batch.Count;

    var percentComplete = (double)processedItems / totalItems * 100;
    Console.WriteLine($"Progress: {percentComplete:F1}% ({processedItems}/{totalItems})");
}
```

### 3. **Handle Partial Failures**

```csharp
var failedItems = new List<Item>();

foreach (var batch in batches)
{
    var results = await processor.ProcessTasksAsync(batchTasks);

    foreach (var result in results.Where(r => !r.Value.IsSuccess))
    {
        failedItems.Add(/* original item */);
    }
}

// Retry failed items
if (failedItems.Any())
{
    Console.WriteLine($"Retrying {failedItems.Count} failed items...");
    // Process failedItems
}
```

### 4. **Add Retry Logic**

```csharp
const int maxRetries = 3;

for (int attempt = 1; attempt <= maxRetries; attempt++)
{
    var results = await processor.ProcessTasksAsync(batchTasks);
    var failed = results.Values.Count(r => !r.IsSuccess);

    if (failed == 0)
        break;

    if (attempt < maxRetries)
    {
        Console.WriteLine($"Retry {attempt}/{maxRetries}: {failed} items failed");
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
    }
}
```

---

## 🎓 Complete Example

Here's a production-ready batch processing example:

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TaskListProcessor;

// Sample: Process 2,000 customer records in batches
var customers = Enumerable.Range(1, 2000)
    .Select(i => new Customer
    {
        Id = i,
        Name = $"Customer {i}",
        Email = $"customer{i}@example.com"
    })
    .ToList();

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

using var processor = new TaskListProcessorEnhanced("Customer Processor", logger);

const int batchSize = 100;
var batches = customers.Chunk(batchSize).ToList(); // .NET 6+ method

Console.WriteLine($"Processing {customers.Count} customers in {batches.Count} batches of {batchSize}...\n");

var sw = Stopwatch.StartNew();
var totalProcessed = 0;
var totalFailed = 0;

for (int i = 0; i < batches.Count; i++)
{
    var batch = batches[i];
    var batchNum = i + 1;

    var tasks = batch.ToDictionary(
        customer => $"Customer-{customer.Id}",
        customer => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(Random.Shared.Next(10, 50), ct);

            // Simulate occasional errors (2% failure rate)
            if (Random.Shared.Next(100) < 2)
                throw new Exception("Processing failed");

            return new { CustomerId = customer.Id, Processed = true };
        })
    );

    var results = await processor.ProcessTasksAsync(tasks);

    var successful = results.Values.Count(r => r.IsSuccess);
    var failed = results.Values.Count(r => !r.IsSuccess);

    totalProcessed += successful;
    totalFailed += failed;

    var progress = (double)totalProcessed / customers.Count * 100;
    Console.WriteLine($"Batch {batchNum}/{batches.Count}: {successful}/{batch.Length} successful | Progress: {progress:F1}%");
}

sw.Stop();

Console.WriteLine($"\n✅ Processing complete!");
Console.WriteLine($"   Total: {totalProcessed + totalFailed}");
Console.WriteLine($"   Successful: {totalProcessed}");
Console.WriteLine($"   Failed: {totalFailed}");
Console.WriteLine($"   Duration: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Throughput: {totalProcessed / (sw.ElapsedMilliseconds / 1000.0):F0} customers/second");

public record Customer
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}
```

---

## 🎯 Key Takeaways

✅ **Batch processing** prevents resource exhaustion
✅ **Controlled concurrency** respects system limits
✅ **Progress tracking** provides visibility
✅ **Error isolation** per batch enables retry logic
✅ **Dynamic batching** adapts to conditions

---

## 📚 What's Next?

**Next Tutorial**: [03-error-handling.md](03-error-handling.md)
Learn comprehensive error handling strategies and resilience patterns.

**Related Reading**:
- [Performance Considerations](../../architecture/performance-considerations.md)
- [FAQ: Batch Processing](../../troubleshooting/faq.md#batch-processing)

---

**Completed**: Tutorial 2 - Batch Processing ✅
**Next**: Tutorial 3 - Error Handling →
