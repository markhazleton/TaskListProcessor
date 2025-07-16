# TaskListProcessor Migration Guide

## Phase 1: Interface Segregation & Dependency Injection Integration

This guide helps you migrate from the monolithic `TaskListProcessorEnhanced` to the new segregated interfaces with proper dependency injection support.

## Overview of Changes

### New Interfaces

- `ITaskProcessor` - For individual task execution
- `ITaskBatchProcessor` - For batch task processing
- `ITaskStreamProcessor` - For streaming task results
- `ITaskTelemetryProvider` - For telemetry and health monitoring

### New Service Registration

- `ServiceCollectionExtensions.AddTaskListProcessor()` - Fluent DI registration
- `ITaskProcessorBuilder` - Builder pattern for decorator configuration

## Migration Steps

### Step 1: Update Service Registration

**Before (Manual instantiation):**

```csharp
var options = new TaskListProcessorOptions { MaxConcurrentTasks = 10 };
var processor = new TaskListProcessorEnhanced("MyProcessor", logger, options);
```

**After (Dependency injection):**

```csharp
// In ConfigureServices/Program.cs
services.AddTaskListProcessor(options =>
{
    options.MaxConcurrentTasks = 10;
})
.WithLogging()
.WithMetrics()
.WithCircuitBreaker();

// In your class
public class MyService
{
    private readonly ITaskProcessor _taskProcessor;
    private readonly ITaskBatchProcessor _batchProcessor;
    
    public MyService(ITaskProcessor taskProcessor, ITaskBatchProcessor batchProcessor)
    {
        _taskProcessor = taskProcessor;
        _batchProcessor = batchProcessor;
    }
}
```

### Step 2: Update Individual Task Execution

**Before:**

```csharp
var result = await processor.ExecuteTaskAsync<string>("myTask", myTask);
```

**After:**

```csharp
var result = await _taskProcessor.ExecuteTaskAsync<string>("myTask", myTask);
```

### Step 3: Update Batch Processing

**Before:**

```csharp
await processor.ProcessTasksAsync(taskFactories, progress, cancellationToken);
```

**After:**

```csharp
await _batchProcessor.ProcessTasksAsync(taskFactories, progress, cancellationToken);
```

### Step 4: Update Streaming Processing

**Before:**

```csharp
await foreach (var result in processor.ProcessTasksStreamAsync(taskFactories, cancellationToken))
{
    // Process result
}
```

**After:**

```csharp
await foreach (var result in _streamProcessor.ProcessTasksStreamAsync(taskFactories, cancellationToken))
{
    // Process result
}
```

### Step 5: Update Telemetry Access

**Before:**

```csharp
var telemetry = processor.Telemetry;
var summary = processor.GetTelemetrySummary();
var health = processor.PerformHealthCheck();
```

**After:**

```csharp
var telemetry = _telemetryProvider.Telemetry;
var summary = _telemetryProvider.GetTelemetrySummary();
var health = _telemetryProvider.PerformHealthCheck();
```

## Decorator Configuration

### Available Decorators

1. **Logging Decorator** - Adds structured logging
2. **Metrics Decorator** - Adds metrics collection (placeholder)
3. **Circuit Breaker Decorator** - Adds circuit breaker pattern (placeholder)
4. **Retry Decorator** - Adds retry logic (placeholder)
5. **Telemetry Export Decorator** - Adds automatic telemetry export

### Fluent Configuration

```csharp
services.AddTaskListProcessor(options =>
{
    options.MaxConcurrentTasks = 10;
    options.EnableDetailedTelemetry = true;
})
.WithLogging()                // Adds comprehensive logging
.WithMetrics()               // Adds metrics collection
.WithCircuitBreaker()        // Adds circuit breaker protection
.WithRetryPolicy()           // Adds retry capabilities
.WithTelemetryExport();      // Adds automatic telemetry export

// Or use all decorators at once
services.AddTaskListProcessor()
    .WithAllDecorators();
```

### Named Processors

For multiple processor instances:

```csharp
services.AddTaskListProcessor("ProcessorA", options =>
{
    options.MaxConcurrentTasks = 5;
})
.WithLogging();

services.AddTaskListProcessor("ProcessorB", options =>
{
    options.MaxConcurrentTasks = 20;
})
.WithAllDecorators();
```

## Backward Compatibility

The `TaskListProcessorEnhanced` class is still available and registered in DI for backward compatibility:

```csharp
// This still works
public class LegacyService
{
    private readonly TaskListProcessorEnhanced _processor;
    
    public LegacyService(TaskListProcessorEnhanced processor)
    {
        _processor = processor;
    }
}
```

## Testing with New Interfaces

### Unit Testing

```csharp
[Test]
public async Task TestTaskExecution()
{
    // Arrange
    var mockProcessor = new Mock<ITaskProcessor>();
    var service = new MyService(mockProcessor.Object);
    
    // Act & Assert
    // Test with mocked interface
}
```

### Integration Testing

```csharp
[Test]
public async Task TestWithRealProcessor()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddTaskListProcessor()
        .WithLogging();
    
    var provider = services.BuildServiceProvider();
    var processor = provider.GetRequiredService<ITaskProcessor>();
    
    // Act & Assert
    // Test with real processor
}
```

## Best Practices

1. **Use appropriate interface** - Don't inject `ITaskBatchProcessor` if you only need single task execution
2. **Configure decorators based on needs** - Only add decorators you actually need
3. **Monitor telemetry** - Use `ITaskTelemetryProvider` for monitoring and health checks
4. **Leverage DI lifetime management** - Processors are registered as singletons by default
5. **Test with interfaces** - Mock the interfaces for better unit testing

## Common Patterns

### Processing Tasks with Progress Reporting

```csharp
public async Task ProcessUserTasks(IEnumerable<UserTask> userTasks)
{
    var progress = new Progress<TaskProgress>(p => 
        Console.WriteLine($"Progress: {p.CompletedTasks}/{p.TotalTasks}"));
    
    var taskFactories = userTasks.ToDictionary(
        t => t.Id.ToString(),
        t => (Func<CancellationToken, Task<object?>>)(ct => ProcessUserTask(t, ct)));
    
    await _batchProcessor.ProcessTasksAsync(taskFactories, progress);
}
```

### Streaming Results Processing

```csharp
public async Task ProcessAndStream(IDictionary<string, Func<CancellationToken, Task<object?>>> tasks)
{
    await foreach (var result in _streamProcessor.ProcessTasksStreamAsync(tasks))
    {
        if (result.IsSuccessful)
        {
            await ProcessResult(result.Data);
        }
        else
        {
            await HandleError(result.ErrorMessage);
        }
    }
}
```

### Health Monitoring

```csharp
public async Task<bool> CheckSystemHealth()
{
    var healthResult = _telemetryProvider.PerformHealthCheck();
    var summary = _telemetryProvider.GetTelemetrySummary();
    
    return healthResult.IsHealthy && summary.SuccessRate > 0.95;
}
```

## Next Steps

1. **Start with DI registration** - Add `AddTaskListProcessor()` to your service configuration
2. **Gradually migrate interfaces** - Replace direct `TaskListProcessorEnhanced` usage with appropriate interfaces
3. **Add decorators as needed** - Start with logging, then add other decorators based on requirements
4. **Monitor and optimize** - Use telemetry provider to monitor performance and adjust configuration
5. **Remove legacy code** - Once fully migrated, remove direct `TaskListProcessorEnhanced` dependencies

This migration maintains full backward compatibility while providing a cleaner, more maintainable architecture following SOLID principles.
