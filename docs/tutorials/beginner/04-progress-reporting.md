# Tutorial 4: Progress Reporting

**Level**: Beginner
**Duration**: 20 minutes
**Prerequisites**:
- Completed [Tutorial 1: Simple Task Execution](01-simple-task-execution.md)
- Completed [Tutorial 2: Batch Processing](02-batch-processing.md)

---

## 🎯 What You'll Learn

By the end of this tutorial, you'll be able to:

- ✅ Track task progress in real-time
- ✅ Report percentage completion
- ✅ Display progress bars
- ✅ Estimate time remaining
- ✅ Provide user feedback during long operations

---

## 📚 Why Progress Reporting Matters

Long-running operations without feedback create poor user experience:

- ❌ Users don't know if the app is frozen or working
- ❌ No visibility into how much work remains
- ❌ Can't identify slow tasks
- ❌ Difficult to debug performance issues

**With progress reporting**:
- ✅ Users see real-time status
- ✅ Estimated completion time
- ✅ Identify bottlenecks
- ✅ Better user experience

---

## 📊 Basic Progress Tracking

### Pattern 1: Simple Counter

Track completed vs. total tasks:

```csharp
using Microsoft.Extensions.Logging;
using TaskListProcessor;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

const int totalTasks = 20;
int completedTasks = 0;
var lockObject = new object();

Console.WriteLine($"Processing {totalTasks} tasks...\n");

var tasks = Enumerable.Range(1, totalTasks).ToDictionary(
    i => $"Task-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        // Simulate work
        await Task.Delay(Random.Shared.Next(100, 500), ct);

        // Thread-safe increment
        lock (lockObject)
        {
            completedTasks++;
            var percent = (double)completedTasks / totalTasks * 100;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Progress: {completedTasks}/{totalTasks} ({percent:F1}%)");
        }

        return $"Task {i} complete";
    })
);

using var processor = new TaskListProcessorEnhanced("Progress Demo", logger);
await processor.ProcessTasksAsync(tasks);

Console.WriteLine($"\n✅ All {totalTasks} tasks completed!");
```

**Output:**
```
Processing 20 tasks...

[14:30:00] Progress: 1/20 (5.0%)
[14:30:00] Progress: 2/20 (10.0%)
[14:30:00] Progress: 3/20 (15.0%)
[14:30:01] Progress: 4/20 (20.0%)
...
[14:30:05] Progress: 20/20 (100.0%)

✅ All 20 tasks completed!
```

---

### Pattern 2: Progress Bar

Visual progress bar for better UX:

```csharp
void DrawProgressBar(int current, int total, int barWidth = 50)
{
    var percent = (double)current / total;
    var filled = (int)(percent * barWidth);
    var empty = barWidth - filled;

    Console.Write($"\r[{new string('█', filled)}{new string('░', empty)}] {percent * 100:F1}% ({current}/{total})");

    if (current == total)
        Console.WriteLine(); // New line when complete
}

const int totalItems = 100;
int processedItems = 0;
var lockObj = new object();

Console.WriteLine($"Processing {totalItems} items...");
DrawProgressBar(0, totalItems);

var tasks = Enumerable.Range(1, totalItems).ToDictionary(
    i => $"Item-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        await Task.Delay(Random.Shared.Next(20, 100), ct);

        lock (lockObj)
        {
            processedItems++;
            DrawProgressBar(processedItems, totalItems);
        }

        return i;
    })
);

using var processor = new TaskListProcessorEnhanced("Progress Bar Demo", logger);
await processor.ProcessTasksAsync(tasks);

Console.WriteLine("✅ Processing complete!");
```

**Output:**
```
Processing 100 items...
[████████████████████████████████████████████░░░░] 89.0% (89/100)
```

*(Updates in real-time)*

---

### Pattern 3: Estimated Time Remaining

Calculate and display ETA:

```csharp
using System.Diagnostics;

const int totalTasks = 50;
int completedTasks = 0;
var lockObj = new object();
var sw = Stopwatch.StartNew();

Console.WriteLine($"Processing {totalTasks} tasks...\n");

var tasks = Enumerable.Range(1, totalTasks).ToDictionary(
    i => $"Task-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        await Task.Delay(Random.Shared.Next(100, 300), ct);

        lock (lockObj)
        {
            completedTasks++;
            var percent = (double)completedTasks / totalTasks * 100;

            // Calculate ETA
            var elapsed = sw.Elapsed;
            var avgTimePerTask = elapsed.TotalSeconds / completedTasks;
            var remainingTasks = totalTasks - completedTasks;
            var etaSeconds = avgTimePerTask * remainingTasks;
            var eta = TimeSpan.FromSeconds(etaSeconds);

            Console.WriteLine($"Progress: {completedTasks}/{totalTasks} ({percent:F1}%) | " +
                            $"Elapsed: {elapsed:mm\\:ss} | " +
                            $"ETA: {eta:mm\\:ss}");
        }

        return i;
    })
);

using var processor = new TaskListProcessorEnhanced("ETA Demo", logger);
await processor.ProcessTasksAsync(tasks);

sw.Stop();
Console.WriteLine($"\n✅ Completed in {sw.Elapsed:mm\\:ss}");
```

**Output:**
```
Processing 50 tasks...

Progress: 5/50 (10.0%) | Elapsed: 00:01 | ETA: 00:09
Progress: 10/50 (20.0%) | Elapsed: 00:02 | ETA: 00:08
Progress: 15/50 (30.0%) | Elapsed: 00:03 | ETA: 00:07
...
Progress: 50/50 (100.0%) | Elapsed: 00:10 | ETA: 00:00

✅ Completed in 00:10
```

---

## 📈 Batch Progress Tracking

Track progress across multiple batches:

```csharp
var allItems = Enumerable.Range(1, 500).ToList();
const int batchSize = 50;
var batches = allItems.Chunk(batchSize).ToList();

Console.WriteLine($"Processing {allItems.Count} items in {batches.Count} batches...\n");

int totalProcessed = 0;
var sw = Stopwatch.StartNew();

for (int batchNum = 0; batchNum < batches.Count; batchNum++)
{
    var batch = batches[batchNum];
    Console.WriteLine($"Batch {batchNum + 1}/{batches.Count}:");

    int batchProcessed = 0;
    var lockObj = new object();

    var tasks = batch.ToDictionary(
        item => $"Item-{item}",
        item => new Func<CancellationToken, Task<object?>>(async ct =>
        {
            await Task.Delay(Random.Shared.Next(10, 50), ct);

            lock (lockObj)
            {
                batchProcessed++;
                totalProcessed++;

                var batchPercent = (double)batchProcessed / batch.Length * 100;
                var totalPercent = (double)totalProcessed / allItems.Count * 100;

                Console.Write($"\r  Batch: {batchPercent:F0}% | Overall: {totalPercent:F1}% ({totalProcessed}/{allItems.Count})");
            }

            return item;
        })
    );

    using var processor = new TaskListProcessorEnhanced("Batch Processor", logger);
    await processor.ProcessTasksAsync(tasks);

    Console.WriteLine(); // New line after batch
}

sw.Stop();
Console.WriteLine($"\n✅ All {allItems.Count} items processed in {sw.Elapsed:mm\\:ss}");
```

**Output:**
```
Processing 500 items in 10 batches...

Batch 1/10:
  Batch: 100% | Overall: 10.0% (50/500)
Batch 2/10:
  Batch: 100% | Overall: 20.0% (100/500)
...
Batch 10/10:
  Batch: 100% | Overall: 100.0% (500/500)

✅ All 500 items processed in 00:05
```

---

## 🎨 Advanced Progress Displays

### Multi-Line Progress Dashboard

```csharp
class ProgressTracker
{
    private int _completed;
    private int _failed;
    private int _inProgress;
    private readonly int _total;
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private readonly object _lock = new();

    public ProgressTracker(int total)
    {
        _total = total;
        _inProgress = total;
    }

    public void TaskStarted()
    {
        // Optional: track actively running tasks
    }

    public void TaskCompleted(bool success)
    {
        lock (_lock)
        {
            _inProgress--;
            if (success)
                _completed++;
            else
                _failed++;

            DisplayProgress();
        }
    }

    private void DisplayProgress()
    {
        var percent = (double)(_completed + _failed) / _total * 100;
        var elapsed = _sw.Elapsed;
        var avgTime = elapsed.TotalSeconds / (_completed + _failed);
        var eta = TimeSpan.FromSeconds(avgTime * _inProgress);

        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║         TASK PROCESSING DASHBOARD           ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine($"║ Total Tasks:     {_total,10}                   ║");
        Console.WriteLine($"║ Completed:       {_completed,10} ✅               ║");
        Console.WriteLine($"║ Failed:          {_failed,10} ❌               ║");
        Console.WriteLine($"║ In Progress:     {_inProgress,10} ⏳               ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine($"║ Progress:        {percent,10:F1}%                ║");
        Console.WriteLine($"║ Elapsed:         {elapsed:hh\\:mm\\:ss}                  ║");
        Console.WriteLine($"║ ETA:             {eta:hh\\:mm\\:ss}                  ║");
        Console.WriteLine($"║ Success Rate:    {(double)_completed / (_completed + _failed) * 100,10:F1}%                ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
    }
}

// Usage
var tracker = new ProgressTracker(100);

var tasks = Enumerable.Range(1, 100).ToDictionary(
    i => $"Task-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        await Task.Delay(Random.Shared.Next(50, 200), ct);

        var success = Random.Shared.Next(100) < 95; // 95% success rate
        tracker.TaskCompleted(success);

        if (!success)
            throw new Exception("Task failed");

        return i;
    })
);

using var processor = new TaskListProcessorEnhanced("Dashboard Demo", logger);
await processor.ProcessTasksAsync(tasks);
```

**Output** (updating in real-time):
```
╔══════════════════════════════════════════════╗
║         TASK PROCESSING DASHBOARD           ║
╠══════════════════════════════════════════════╣
║ Total Tasks:            100                   ║
║ Completed:               89 ✅               ║
║ Failed:                   5 ❌               ║
║ In Progress:              6 ⏳               ║
╠══════════════════════════════════════════════╣
║ Progress:              94.0%                ║
║ Elapsed:         00:00:12                  ║
║ ETA:             00:00:01                  ║
║ Success Rate:          94.7%                ║
╚══════════════════════════════════════════════╝
```

---

## 🔔 Progress Callbacks

Send progress updates to external systems:

```csharp
class ProgressReporter
{
    public event EventHandler<ProgressEventArgs>? ProgressChanged;

    private int _completed;
    private readonly int _total;
    private readonly object _lock = new();

    public ProgressReporter(int total)
    {
        _total = total;
    }

    public void ReportProgress()
    {
        int current;
        lock (_lock)
        {
            _completed++;
            current = _completed;
        }

        var args = new ProgressEventArgs
        {
            Current = current,
            Total = _total,
            Percentage = (double)current / _total * 100
        };

        ProgressChanged?.Invoke(this, args);
    }
}

class ProgressEventArgs : EventArgs
{
    public int Current { get; init; }
    public int Total { get; init; }
    public double Percentage { get; init; }
}

// Usage
var reporter = new ProgressReporter(50);

// Subscribe to progress updates
reporter.ProgressChanged += (sender, args) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Progress: {args.Percentage:F1}% ({args.Current}/{args.Total})");

    // Could also:
    // - Update UI
    // - Send webhook
    // - Update database
    // - Trigger notifications
};

var tasks = Enumerable.Range(1, 50).ToDictionary(
    i => $"Task-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        await Task.Delay(Random.Shared.Next(50, 150), ct);
        reporter.ReportProgress();
        return i;
    })
);

using var processor = new TaskListProcessorEnhanced("Callback Demo", logger);
await processor.ProcessTasksAsync(tasks);
```

---

## 📱 Real-World Example: File Processor with Progress

```csharp
using System.Diagnostics;

// Simulate processing 200 files
var files = Enumerable.Range(1, 200)
    .Select(i => $"file{i:000}.dat")
    .ToList();

Console.WriteLine($"Processing {files.Count} files...\n");

int processed = 0;
int errors = 0;
long totalBytes = 0;
var lockObj = new object();
var sw = Stopwatch.StartNew();

var tasks = files.ToDictionary(
    file => file,
    file => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        try
        {
            // Simulate file processing
            var bytes = Random.Shared.Next(1000, 10000);
            await Task.Delay(Random.Shared.Next(50, 200), ct);

            lock (lockObj)
            {
                processed++;
                totalBytes += bytes;

                var percent = (double)processed / files.Count * 100;
                var elapsed = sw.Elapsed;
                var filesPerSec = processed / elapsed.TotalSeconds;
                var eta = TimeSpan.FromSeconds((files.Count - processed) / filesPerSec);

                Console.Write($"\r[{new string('█', (int)(percent / 2))}{new string('░', 50 - (int)(percent / 2))}] " +
                            $"{percent:F1}% | " +
                            $"{processed}/{files.Count} files | " +
                            $"{totalBytes / 1024:N0} KB | " +
                            $"{filesPerSec:F1} files/sec | " +
                            $"ETA: {eta:mm\\:ss}");
            }

            return new { File = file, Bytes = bytes, Success = true };
        }
        catch (Exception ex)
        {
            lock (lockObj)
            {
                errors++;
            }
            throw;
        }
    })
);

using var processor = new TaskListProcessorEnhanced("File Processor", logger);
var results = await processor.ProcessTasksAsync(tasks);

sw.Stop();

Console.WriteLine($"\n\n✅ Processing complete!");
Console.WriteLine($"   Files processed: {processed}");
Console.WriteLine($"   Errors: {errors}");
Console.WriteLine($"   Total size: {totalBytes / 1024:N0} KB");
Console.WriteLine($"   Duration: {sw.Elapsed:mm\\:ss}");
Console.WriteLine($"   Throughput: {processed / sw.Elapsed.TotalSeconds:F1} files/second");
```

**Output:**
```
Processing 200 files...

[██████████████████████████████████████████████░░] 96.5% | 193/200 files | 1,234 KB | 45.2 files/sec | ETA: 00:00

✅ Processing complete!
   Files processed: 200
   Errors: 0
   Total size: 1,280 KB
   Duration: 00:04
   Throughput: 46.3 files/second
```

---

## 🧩 Practice Exercises

### Exercise 1: Image Thumbnail Generator

Create a progress tracker for thumbnail generation:

```csharp
var images = Enumerable.Range(1, 150)
    .Select(i => $"photo{i}.jpg")
    .ToList();

// TODO:
// 1. Display progress bar
// 2. Show current/total count
// 3. Display processing speed (images/sec)
// 4. Show ETA
// 5. Track total MB processed
```

### Exercise 2: Database Migration

Track progress of migrating records:

```csharp
// TODO:
// 1. Process in batches of 100
// 2. Show batch progress
// 3. Show overall progress
// 4. Display records/second
// 5. Estimate time to completion
```

### Exercise 3: Multi-Stage Progress

Track progress across multiple stages:

```csharp
// Stages: Download → Process → Upload
// TODO:
// 1. Track progress per stage
// 2. Show overall progress (all stages)
// 3. Display current stage name
// 4. Show ETA for current stage and total
```

---

## ⚠️ Common Mistakes

### 1. **Not Thread-Safe**

❌ **Wrong**:
```csharp
int counter = 0; // Race condition!
counter++;
Console.WriteLine(counter);
```

✅ **Correct**:
```csharp
int counter = 0;
var lockObj = new object();

lock (lockObj)
{
    counter++;
    Console.WriteLine(counter);
}
```

### 2. **Too Frequent Updates**

❌ **Wrong**:
```csharp
// Updates console for every task (100k times!)
foreach (var item in millionItems)
{
    Console.WriteLine($"Progress: {counter++}");
}
```

✅ **Correct**:
```csharp
// Update every 1% or every second
if (counter % 1000 == 0 || elapsed.TotalSeconds > lastUpdate + 1)
{
    Console.WriteLine($"Progress: {counter}");
}
```

### 3. **Blocking UI Thread**

❌ **Wrong** (in UI apps):
```csharp
// Blocks UI while processing
var results = await processor.ProcessTasksAsync(tasks); // UI freezes!
```

✅ **Correct**:
```csharp
// Report progress asynchronously
await Task.Run(async () =>
{
    var results = await processor.ProcessTasksAsync(tasks);
});
```

### 4. **Inaccurate ETA**

❌ **Wrong**:
```csharp
// ETA based on first few fast tasks
var eta = (elapsed / completed) * remaining; // Inaccurate!
```

✅ **Correct**:
```csharp
// Use moving average or wait for sufficient samples
if (completed > Math.Min(total * 0.1, 10))
{
    var eta = (elapsed / completed) * remaining;
}
```

---

## 🎯 Key Takeaways

✅ **Progress tracking** improves user experience
✅ **Thread-safe counters** prevent race conditions
✅ **Progress bars** provide visual feedback
✅ **ETA calculations** set expectations
✅ **Batch tracking** shows multi-level progress

---

## 📚 What's Next?

**Next Tutorial**: [05-basic-telemetry.md](05-basic-telemetry.md)
Learn how to collect and analyze performance metrics.

**Related Reading**:
- [FAQ: Performance](../../troubleshooting/faq.md#performance)
- [Performance Considerations](../../architecture/performance-considerations.md)

---

**Completed**: Tutorial 4 - Progress Reporting ✅
**Next**: Tutorial 5 - Basic Telemetry →
