# Tutorial 5: Basic Telemetry

**Level**: Beginner
**Duration**: 25 minutes
**Prerequisites**:
- Completed [Tutorial 1: Simple Task Execution](01-simple-task-execution.md)
- Completed [Tutorial 4: Progress Reporting](04-progress-reporting.md)

---

## 🎯 What You'll Learn

By the end of this tutorial, you'll be able to:

- ✅ Collect performance metrics
- ✅ Calculate task execution statistics
- ✅ Identify slow tasks and bottlenecks
- ✅ Generate telemetry reports
- ✅ Monitor success/failure rates
- ✅ Track resource usage

---

## 📚 Why Telemetry Matters

Without telemetry, you're flying blind:

- ❌ Don't know which tasks are slow
- ❌ Can't measure performance improvements
- ❌ No visibility into failure patterns
- ❌ Can't optimize based on data

**With telemetry**:
- ✅ Identify bottlenecks
- ✅ Measure before/after improvements
- ✅ Detect anomalies
- ✅ Make data-driven decisions

---

## 📊 Basic Metrics Collection

### Collecting Execution Times

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TaskListProcessor;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["FastTask"] = async ct =>
    {
        await Task.Delay(50, ct);
        return "Fast result";
    },

    ["MediumTask"] = async ct =>
    {
        await Task.Delay(200, ct);
        return "Medium result";
    },

    ["SlowTask"] = async ct =>
    {
        await Task.Delay(500, ct);
        return "Slow result";
    }
};

using var processor = new TaskListProcessorEnhanced("Metrics Demo", logger);
var results = await processor.ProcessTasksAsync(tasks);

// Analyze execution times
Console.WriteLine("📊 Execution Times:\n");
foreach (var result in results.OrderBy(r => r.Value.Duration))
{
    Console.WriteLine($"{result.Key,-15} {result.Value.Duration,6}ms");
}

// Calculate statistics
var durations = results.Values.Select(r => r.Duration).ToList();
var avgDuration = durations.Average();
var minDuration = durations.Min();
var maxDuration = durations.Max();

Console.WriteLine($"\n📈 Statistics:");
Console.WriteLine($"   Average: {avgDuration:F0}ms");
Console.WriteLine($"   Min:     {minDuration}ms");
Console.WriteLine($"   Max:     {maxDuration}ms");
Console.WriteLine($"   Range:   {maxDuration - minDuration}ms");
```

**Output:**
```
📊 Execution Times:

FastTask         52ms
MediumTask      203ms
SlowTask        502ms

📈 Statistics:
   Average: 252ms
   Min:     52ms
   Max:     502ms
   Range:   450ms
```

---

## 📉 Success/Failure Metrics

Track success rates and failure patterns:

```csharp
var tasks = Enumerable.Range(1, 100).ToDictionary(
    i => $"Task-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        await Task.Delay(Random.Shared.Next(10, 50), ct);

        // Simulate 15% failure rate
        if (Random.Shared.Next(100) < 15)
            throw new Exception($"Task {i} failed");

        return i;
    })
);

using var processor = new TaskListProcessorEnhanced("Success Rate Demo", logger);
var sw = Stopwatch.StartNew();
var results = await processor.ProcessTasksAsync(tasks);
sw.Stop();

// Calculate metrics
var total = results.Count;
var successful = results.Values.Count(r => r.IsSuccess);
var failed = results.Values.Count(r => !r.IsSuccess);
var successRate = (double)successful / total * 100;

Console.WriteLine("📊 Success Metrics:\n");
Console.WriteLine($"Total Tasks:    {total}");
Console.WriteLine($"Successful:     {successful} ({successRate:F1}%)");
Console.WriteLine($"Failed:         {failed} ({100 - successRate:F1}%)");
Console.WriteLine($"Total Time:     {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"Avg Time/Task:  {sw.ElapsedMilliseconds / (double)total:F1}ms");
Console.WriteLine($"Throughput:     {total / sw.Elapsed.TotalSeconds:F1} tasks/sec");
```

**Output:**
```
📊 Success Metrics:

Total Tasks:    100
Successful:     85 (85.0%)
Failed:         15 (15.0%)
Total Time:     523ms
Avg Time/Task:  5.2ms
Throughput:     191.2 tasks/sec
```

---

## 🎯 Telemetry Dashboard

Create a comprehensive telemetry report:

```csharp
class TelemetryCollector
{
    private readonly List<TaskMetric> _metrics = new();
    private readonly object _lock = new();
    private readonly Stopwatch _overallTimer = Stopwatch.StartNew();

    public void RecordTask(string name, long durationMs, bool success, Exception? exception = null)
    {
        lock (_lock)
        {
            _metrics.Add(new TaskMetric
            {
                Name = name,
                DurationMs = durationMs,
                Success = success,
                Exception = exception,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public TelemetryReport GenerateReport()
    {
        _overallTimer.Stop();

        lock (_lock)
        {
            var successful = _metrics.Where(m => m.Success).ToList();
            var failed = _metrics.Where(m => !m.Success).ToList();
            var durations = _metrics.Select(m => m.DurationMs).OrderBy(d => d).ToList();

            return new TelemetryReport
            {
                TotalTasks = _metrics.Count,
                SuccessfulTasks = successful.Count,
                FailedTasks = failed.Count,
                SuccessRate = successful.Count / (double)_metrics.Count * 100,

                TotalDurationMs = _overallTimer.ElapsedMilliseconds,
                AverageDurationMs = durations.Average(),
                MedianDurationMs = GetMedian(durations),
                MinDurationMs = durations.Min(),
                MaxDurationMs = durations.Max(),
                P95DurationMs = GetPercentile(durations, 95),
                P99DurationMs = GetPercentile(durations, 99),

                TasksPerSecond = _metrics.Count / _overallTimer.Elapsed.TotalSeconds,

                FailuresByType = failed
                    .GroupBy(f => f.Exception?.GetType().Name ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),

                SlowestTasks = _metrics
                    .OrderByDescending(m => m.DurationMs)
                    .Take(5)
                    .Select(m => new { m.Name, m.DurationMs })
                    .ToList()
            };
        }
    }

    private static double GetMedian(List<long> sortedValues)
    {
        int mid = sortedValues.Count / 2;
        return sortedValues.Count % 2 == 0
            ? (sortedValues[mid - 1] + sortedValues[mid]) / 2.0
            : sortedValues[mid];
    }

    private static long GetPercentile(List<long> sortedValues, int percentile)
    {
        int index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }
}

record TaskMetric
{
    public required string Name { get; init; }
    public required long DurationMs { get; init; }
    public required bool Success { get; init; }
    public Exception? Exception { get; init; }
    public required DateTime Timestamp { get; init; }
}

record TelemetryReport
{
    public int TotalTasks { get; init; }
    public int SuccessfulTasks { get; init; }
    public int FailedTasks { get; init; }
    public double SuccessRate { get; init; }

    public long TotalDurationMs { get; init; }
    public double AverageDurationMs { get; init; }
    public double MedianDurationMs { get; init; }
    public long MinDurationMs { get; init; }
    public long MaxDurationMs { get; init; }
    public long P95DurationMs { get; init; }
    public long P99DurationMs { get; init; }

    public double TasksPerSecond { get; init; }

    public Dictionary<string, int> FailuresByType { get; init; } = new();
    public List<object> SlowestTasks { get; init; } = new();
}

// Usage
var telemetry = new TelemetryCollector();

var tasks = Enumerable.Range(1, 200).ToDictionary(
    i => $"Task-{i}",
    i => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await Task.Delay(Random.Shared.Next(10, 500), ct);

            // Simulate occasional errors
            if (Random.Shared.Next(100) < 10)
                throw new InvalidOperationException("Random failure");

            sw.Stop();
            telemetry.RecordTask($"Task-{i}", sw.ElapsedMilliseconds, true);
            return i;
        }
        catch (Exception ex)
        {
            sw.Stop();
            telemetry.RecordTask($"Task-{i}", sw.ElapsedMilliseconds, false, ex);
            throw;
        }
    })
);

using var processor = new TaskListProcessorEnhanced("Telemetry Demo", logger);
await processor.ProcessTasksAsync(tasks);

// Generate and display report
var report = telemetry.GenerateReport();

Console.WriteLine("\n╔════════════════════════════════════════════════╗");
Console.WriteLine("║           TELEMETRY REPORT                     ║");
Console.WriteLine("╠════════════════════════════════════════════════╣");
Console.WriteLine($"║ Total Tasks:        {report.TotalTasks,10}                   ║");
Console.WriteLine($"║ Successful:         {report.SuccessfulTasks,10} ({report.SuccessRate,5:F1}%)         ║");
Console.WriteLine($"║ Failed:             {report.FailedTasks,10} ({100 - report.SuccessRate,5:F1}%)         ║");
Console.WriteLine("╠════════════════════════════════════════════════╣");
Console.WriteLine($"║ Total Time:         {report.TotalDurationMs,10}ms                ║");
Console.WriteLine($"║ Avg Time/Task:      {report.AverageDurationMs,10:F1}ms                ║");
Console.WriteLine($"║ Median Time:        {report.MedianDurationMs,10:F1}ms                ║");
Console.WriteLine($"║ Min Time:           {report.MinDurationMs,10}ms                ║");
Console.WriteLine($"║ Max Time:           {report.MaxDurationMs,10}ms                ║");
Console.WriteLine($"║ P95 Time:           {report.P95DurationMs,10}ms                ║");
Console.WriteLine($"║ P99 Time:           {report.P99DurationMs,10}ms                ║");
Console.WriteLine("╠════════════════════════════════════════════════╣");
Console.WriteLine($"║ Throughput:         {report.TasksPerSecond,10:F1} tasks/sec        ║");
Console.WriteLine("╠════════════════════════════════════════════════╣");

if (report.FailuresByType.Any())
{
    Console.WriteLine("║ Failures by Type:                              ║");
    foreach (var failure in report.FailuresByType)
    {
        Console.WriteLine($"║   {failure.Key,-25} {failure.Value,10}         ║");
    }
    Console.WriteLine("╠════════════════════════════════════════════════╣");
}

Console.WriteLine("║ Top 5 Slowest Tasks:                           ║");
foreach (var task in report.SlowestTasks)
{
    var name = task.GetType().GetProperty("Name")?.GetValue(task)?.ToString() ?? "";
    var duration = task.GetType().GetProperty("DurationMs")?.GetValue(task)?.ToString() ?? "";
    Console.WriteLine($"║   {name,-30} {duration,6}ms      ║");
}
Console.WriteLine("╚════════════════════════════════════════════════╝");
```

**Output:**
```
╔════════════════════════════════════════════════╗
║           TELEMETRY REPORT                     ║
╠════════════════════════════════════════════════╣
║ Total Tasks:               200                   ║
║ Successful:                180 ( 90.0%)         ║
║ Failed:                     20 ( 10.0%)         ║
╠════════════════════════════════════════════════╣
║ Total Time:               1250ms                ║
║ Avg Time/Task:            255.3ms                ║
║ Median Time:              248.0ms                ║
║ Min Time:                  12ms                ║
║ Max Time:                 498ms                ║
║ P95 Time:                 475ms                ║
║ P99 Time:                 492ms                ║
╠════════════════════════════════════════════════╣
║ Throughput:               160.0 tasks/sec        ║
╠════════════════════════════════════════════════╣
║ Failures by Type:                              ║
║   InvalidOperationException                20         ║
╠════════════════════════════════════════════════╣
║ Top 5 Slowest Tasks:                           ║
║   Task-142                       498ms      ║
║   Task-87                        495ms      ║
║   Task-156                       489ms      ║
║   Task-23                        485ms      ║
║   Task-199                       482ms      ║
╚════════════════════════════════════════════════╝
```

---

## 📈 Percentile Analysis

Understand performance distribution:

```csharp
var results = await processor.ProcessTasksAsync(tasks);
var durations = results.Values.Select(r => r.Duration).OrderBy(d => d).ToList();

Console.WriteLine("📊 Performance Percentiles:\n");
Console.WriteLine($"P50 (Median): {GetPercentile(durations, 50)}ms");
Console.WriteLine($"P75:          {GetPercentile(durations, 75)}ms");
Console.WriteLine($"P90:          {GetPercentile(durations, 90)}ms");
Console.WriteLine($"P95:          {GetPercentile(durations, 95)}ms");
Console.WriteLine($"P99:          {GetPercentile(durations, 99)}ms");
Console.WriteLine($"P99.9:        {GetPercentile(durations, 99.9)}ms");

Console.WriteLine("\n💡 What this means:");
Console.WriteLine("   - P50: Half of tasks complete in this time or less");
Console.WriteLine("   - P95: 95% of tasks complete in this time or less");
Console.WriteLine("   - P99: Only 1% of tasks are slower than this");

long GetPercentile(List<long> sortedValues, double percentile)
{
    int index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
    return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
}
```

---

## 🔍 Identifying Bottlenecks

Find and fix slow tasks:

```csharp
var results = await processor.ProcessTasksAsync(tasks);

// Identify slow tasks (> 2x average)
var avgDuration = results.Values.Average(r => r.Duration);
var slowTasks = results
    .Where(r => r.Value.Duration > avgDuration * 2)
    .OrderByDescending(r => r.Value.Duration)
    .ToList();

if (slowTasks.Any())
{
    Console.WriteLine($"⚠️  Found {slowTasks.Count} slow tasks (>2x average):\n");

    foreach (var task in slowTasks.Take(10))
    {
        var slowdown = task.Value.Duration / avgDuration;
        Console.WriteLine($"{task.Key,-20} {task.Value.Duration,6}ms ({slowdown:F1}x slower than average)");
    }

    Console.WriteLine($"\n💡 Recommendation: Investigate these tasks for optimization opportunities");
}
else
{
    Console.WriteLine("✅ No significant bottlenecks detected");
}
```

---

## 📊 Time-Series Analysis

Track metrics over time:

```csharp
class TimeSeriesCollector
{
    private readonly List<TimeSeriesPoint> _points = new();
    private readonly object _lock = new();

    public void RecordPoint(DateTime timestamp, string metric, double value)
    {
        lock (_lock)
        {
            _points.Add(new TimeSeriesPoint
            {
                Timestamp = timestamp,
                Metric = metric,
                Value = value
            });
        }
    }

    public void DisplayTimeSeries(string metric, int bucketSeconds = 1)
    {
        var metricPoints = _points
            .Where(p => p.Metric == metric)
            .OrderBy(p => p.Timestamp)
            .ToList();

        if (!metricPoints.Any())
        {
            Console.WriteLine($"No data for metric: {metric}");
            return;
        }

        var start = metricPoints.First().Timestamp;
        var buckets = metricPoints
            .GroupBy(p => (int)(p.Timestamp - start).TotalSeconds / bucketSeconds)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Bucket = g.Key,
                Count = g.Count(),
                Avg = g.Average(p => p.Value)
            })
            .ToList();

        Console.WriteLine($"\n📈 {metric} over time ({bucketSeconds}s buckets):\n");
        Console.WriteLine("Time | Count | Avg Value | Graph");
        Console.WriteLine("-----|-------|-----------|" + new string('-', 30));

        foreach (var bucket in buckets)
        {
            var barLength = (int)(bucket.Avg / 10); // Scale for display
            var bar = new string('█', Math.Min(barLength, 30));
            Console.WriteLine($"{bucket.Bucket,3}s | {bucket.Count,5} | {bucket.Avg,8:F1}ms | {bar}");
        }
    }
}

record TimeSeriesPoint
{
    public required DateTime Timestamp { get; init; }
    public required string Metric { get; init; }
    public required double Value { get; init; }
}
```

---

## 🧩 Practice Exercises

### Exercise 1: Performance Comparison

Compare performance before and after optimization:

```csharp
// TODO:
// 1. Run tasks and collect baseline metrics
// 2. "Optimize" (reduce Task.Delay)
// 3. Run again and collect new metrics
// 4. Calculate % improvement
// 5. Display comparison table
```

### Exercise 2: Failure Pattern Analysis

Analyze when failures occur:

```csharp
// TODO:
// 1. Run 500 tasks with random failures
// 2. Group failures by time bucket (every 30s)
// 3. Identify if failures cluster at certain times
// 4. Calculate failure rate per time period
// 5. Display findings
```

### Exercise 3: Resource Usage Tracking

Track memory and CPU during processing:

```csharp
// TODO:
// 1. Sample memory usage every second
// 2. Track CPU usage
// 3. Correlate with task execution
// 4. Identify resource-intensive tasks
// 5. Generate resource usage report
```

---

## ⚠️ Common Mistakes

### 1. **Not Using Percentiles**

❌ **Wrong**:
```csharp
var avgTime = durations.Average(); // Misleading if outliers exist
Console.WriteLine($"Typical time: {avgTime}ms");
```

✅ **Correct**:
```csharp
var p50 = GetPercentile(durations, 50); // Median, not affected by outliers
var p95 = GetPercentile(durations, 95); // Understand tail latency
Console.WriteLine($"Typical time (P50): {p50}ms");
Console.WriteLine($"95% complete within: {p95}ms");
```

### 2. **Ignoring Failed Tasks in Metrics**

❌ **Wrong**:
```csharp
var successfulDurations = results.Values
    .Where(r => r.IsSuccess)
    .Select(r => r.Duration); // Ignores failed task durations!
```

✅ **Correct**:
```csharp
var allDurations = results.Values.Select(r => r.Duration); // Include all
var successRate = results.Values.Count(r => r.IsSuccess) / (double)results.Count;
```

### 3. **Not Collecting Enough Context**

❌ **Wrong**:
```csharp
metrics.Add(durationMs); // What task? When? Success?
```

✅ **Correct**:
```csharp
metrics.Add(new TaskMetric
{
    Name = taskName,
    Duration = durationMs,
    Success = isSuccess,
    Timestamp = DateTime.UtcNow,
    Exception = exception
});
```

### 4. **Over-Aggregating Data**

❌ **Wrong**:
```csharp
// Only storing average loses distribution info
var avg = durations.Average();
```

✅ **Correct**:
```csharp
// Keep raw data for richer analysis
var p50 = GetPercentile(durations, 50);
var p95 = GetPercentile(durations, 95);
var p99 = GetPercentile(durations, 99);
var avg = durations.Average();
var stdDev = CalculateStdDev(durations);
```

---

## 🎓 Complete Example: Production Telemetry

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TaskListProcessor;

// Simulate realistic workload
var workload = Enumerable.Range(1, 500).Select(i => new WorkItem
{
    Id = i,
    Type = i % 3 == 0 ? "Heavy" : i % 2 == 0 ? "Medium" : "Light",
    Priority = Random.Shared.Next(1, 4)
}).ToList();

var telemetry = new TelemetryCollector();

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<TaskListProcessorEnhanced>();

var tasks = workload.ToDictionary(
    item => $"{item.Type}-{item.Id}",
    item => new Func<CancellationToken, Task<object?>>(async ct =>
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // Simulate different workload types
            var delay = item.Type switch
            {
                "Heavy" => Random.Shared.Next(200, 500),
                "Medium" => Random.Shared.Next(50, 200),
                "Light" => Random.Shared.Next(10, 50),
                _ => 100
            };

            await Task.Delay(delay, ct);

            // Simulate 5% failure rate
            if (Random.Shared.Next(100) < 5)
                throw new Exception($"{item.Type} processing failed");

            sw.Stop();
            telemetry.RecordTask($"{item.Type}-{item.Id}", sw.ElapsedMilliseconds, true);
            return new { Item = item, ProcessedAt = DateTime.UtcNow };
        }
        catch (Exception ex)
        {
            sw.Stop();
            telemetry.RecordTask($"{item.Type}-{item.Id}", sw.ElapsedMilliseconds, false, ex);
            throw;
        }
    })
);

Console.WriteLine($"Processing {workload.Count} work items...\n");

using var processor = new TaskListProcessorEnhanced("Production Workload", logger);
await processor.ProcessTasksAsync(tasks);

var report = telemetry.GenerateReport();

// Display comprehensive report
Console.WriteLine("\n" + "=".PadRight(60, '='));
Console.WriteLine("PRODUCTION TELEMETRY REPORT".PadLeft(40));
Console.WriteLine("=".PadRight(60, '='));

Console.WriteLine($"\n📊 Overall Metrics:");
Console.WriteLine($"   Total Tasks:     {report.TotalTasks}");
Console.WriteLine($"   Success Rate:    {report.SuccessRate:F1}%");
Console.WriteLine($"   Total Time:      {report.TotalDurationMs}ms");
Console.WriteLine($"   Throughput:      {report.TasksPerSecond:F1} tasks/sec");

Console.WriteLine($"\n⏱️  Performance:");
Console.WriteLine($"   Average:         {report.AverageDurationMs:F1}ms");
Console.WriteLine($"   Median (P50):    {report.MedianDurationMs:F1}ms");
Console.WriteLine($"   P95:             {report.P95DurationMs}ms");
Console.WriteLine($"   P99:             {report.P99DurationMs}ms");
Console.WriteLine($"   Range:           {report.MinDurationMs}ms - {report.MaxDurationMs}ms");

if (report.FailuresByType.Any())
{
    Console.WriteLine($"\n❌ Failures:");
    foreach (var failure in report.FailuresByType)
    {
        Console.WriteLine($"   {failure.Key}: {failure.Value}");
    }
}

Console.WriteLine("\n" + "=".PadRight(60, '='));

record WorkItem
{
    public required int Id { get; init; }
    public required string Type { get; init; }
    public required int Priority { get; init; }
}
```

---

## 🎯 Key Takeaways

✅ **Collect comprehensive metrics** (duration, success, errors)
✅ **Use percentiles** for accurate performance understanding
✅ **Track trends** over time to identify patterns
✅ **Identify bottlenecks** using statistical analysis
✅ **Generate reports** for stakeholders
✅ **Make data-driven** optimization decisions

---

## 📚 What's Next?

**Congratulations!** You've completed all beginner tutorials. You're ready for intermediate topics:

**Next Tutorial**: [Intermediate: Dependency Injection](../intermediate/01-dependency-injection.md)
Learn how to integrate TaskListProcessor with ASP.NET Core and DI containers.

**Related Reading**:
- [Performance Considerations](../../architecture/performance-considerations.md)
- [FAQ: Metrics & Monitoring](../../troubleshooting/faq.md#metrics-monitoring)

---

**Completed**: Tutorial 5 - Basic Telemetry ✅
**Next**: Intermediate Tutorials →
