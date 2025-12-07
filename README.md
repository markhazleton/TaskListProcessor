# 🚀 TaskListProcessor

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square)](https://github.com/markhazleton/TaskListProcessor)
[![NuGet](https://img.shields.io/badge/NuGet-Coming%20Soon-orange?style=flat-square)](https://www.nuget.org/packages/TaskListProcessor)

> **A modern, enterprise-grade .NET 10.0 library for orchestrating asynchronous operations with comprehensive telemetry, circuit breakers, dependency injection, and advanced scheduling capabilities.**

---

## 📑 Table of Contents

- [🚀 TaskListProcessor](#-tasklistprocessor)
  - [📑 Table of Contents](#-table-of-contents)
  - [✨ Overview](#-overview)
  - [🎯 Why TaskListProcessor?](#-why-tasklistprocessor)
  - [🔥 Key Features](#-key-features)
  - [🏗️ Architecture](#️-architecture)
  - [⚡ Quick Start](#-quick-start)
  - [📖 Comprehensive Examples](#-comprehensive-examples)
  - [🔧 Advanced Usage](#-advanced-usage)
  - [📊 Performance & Telemetry](#-performance--telemetry)
  - [🛠️ API Reference](#️-api-reference)
  - [🧪 Testing](#-testing)
  - [🔄 Migration Guide](#-migration-guide)
  - [🏗️ Project Structure](#️-project-structure)
  - [🤝 Contributing](#-contributing)
  - [📜 License](#-license)
  - [🔗 Resources](#-resources)

---

## ✨ Overview

TaskListProcessor is a production-ready .NET 10.0 library designed to solve complex asynchronous orchestration challenges in modern applications. Built with enterprise-grade patterns including dependency injection, circuit breakers, task scheduling, and comprehensive telemetry, it provides a robust foundation for high-throughput, fault-tolerant systems.

### 🎯 Why TaskListProcessor?

**The Problem:** Modern applications require sophisticated coordination of multiple async operations—API calls, database queries, file I/O, microservice interactions—while maintaining resilience, observability, and performance under varying loads.

**The Solution:** TaskListProcessor provides a battle-tested, enterprise-ready framework with:

- 🛡️ **Fault Isolation**: Circuit breakers and individual task failure isolation
- 📊 **Enterprise Observability**: OpenTelemetry integration with rich metrics and tracing
- ⚡ **Advanced Scheduling**: Priority-based, dependency-aware task execution
- 🎯 **Type Safety**: Strongly-typed results with comprehensive error categorization
- 🔧 **Dependency Injection**: Native .NET DI integration with decorator pattern support
- 🏗️ **Interface Segregation**: Clean, focused interfaces following SOLID principles

## 🔥 Key Features

### Core Processing Capabilities

- **🚀 Concurrent Execution**: Parallel task processing with configurable concurrency limits and load balancing
- **🛡️ Circuit Breaker Pattern**: Automatic failure detection and cascading failure prevention
- **📊 Rich Telemetry**: Comprehensive timing, success rates, error tracking, and OpenTelemetry integration
- **🎯 Type Safety**: Strongly-typed results with full IntelliSense support and error categorization
- **⏱️ Timeout & Cancellation**: Built-in support for graceful shutdown and per-task timeouts
- **🔄 Task Dependencies**: Dependency resolution with topological sorting and execution ordering

### Enterprise Architecture Features

- **🏗️ Dependency Injection**: Native .NET DI integration with fluent configuration API
- **🎨 Interface Segregation**: Clean, focused interfaces following SOLID principles
- **� Decorator Pattern**: Pluggable cross-cutting concerns (logging, metrics, circuit breakers)
- **📈 Advanced Scheduling**: Priority-based, FIFO, LIFO, and custom scheduling strategies
- **🧵 Thread Safety**: Lock-free concurrent collections and thread-safe operations
- **💾 Memory Optimization**: Object pooling and efficient memory management

### Developer Experience

- **� Structured Logging**: Integration with Microsoft.Extensions.Logging and Serilog
- **🔍 Health Checks**: Built-in health monitoring and diagnostic capabilities
- **� Streaming Results**: Async enumerable support for real-time result processing
- **🧪 Testing Support**: Comprehensive test helpers and mock-friendly interfaces
- **📖 Rich Documentation**: Extensive XML documentation and practical examples

---

## 🚦 Getting Started

### 🎓 Choose Your Learning Path

<table>
<tr>
<td width="33%" valign="top">

#### 🟢 **New to TaskListProcessor?**

Start here for a guided introduction:

1. **[5-Minute Quick Start](docs/getting-started/01-quick-start-5-minutes.md)** ⚡
   Get your first processor running in 5 minutes

2. **[Fundamentals](docs/getting-started/02-fundamentals.md)** 📚
   Understand core concepts and architecture

3. **[Your First Real Processor](docs/getting-started/03-your-first-processor.md)** 🎓
   Build a production-ready Travel Dashboard

4. **[Common Pitfalls](docs/getting-started/04-common-pitfalls.md)** ⚠️
   Avoid the 10 most common mistakes

**Time to productivity**: ~70 minutes

</td>
<td width="33%" valign="top">

#### 🟡 **Ready for More?**

Explore intermediate features:

1. **[Dependency Injection](docs/tutorials/intermediate/01-dependency-injection.md)** 🔧
   Integrate with ASP.NET Core

2. **[Circuit Breaker Pattern](docs/tutorials/intermediate/02-circuit-breaker-pattern.md)** 🛡️
   Build resilient applications

3. **[Advanced Scheduling](docs/tutorials/intermediate/03-advanced-scheduling.md)** 📅
   Priority and dependency-based execution

4. **[Task Dependencies](docs/tutorials/intermediate/04-task-dependencies.md)** 🔗
   Coordinate complex workflows

5. **[Streaming Results](docs/tutorials/intermediate/05-streaming-results.md)** 📡
   Process results in real-time

</td>
<td width="33%" valign="top">

#### 🔴 **Production Ready?**

Advanced topics and optimization:

1. **[Memory Optimization](docs/tutorials/advanced/01-memory-optimization.md)** 💾
   Handle large-scale processing

2. **[Load Balancing](docs/tutorials/advanced/02-load-balancing.md)** ⚖️
   Distribute work efficiently

3. **[OpenTelemetry Integration](docs/tutorials/advanced/03-opentelemetry-integration.md)** 📊
   Enterprise observability

4. **[Performance Tuning](docs/tutorials/advanced/05-performance-tuning.md)** 🚀
   Optimize for production

5. **[Production Patterns](docs/tutorials/advanced/06-production-patterns.md)** 🏭
   Battle-tested strategies

</td>
</tr>
</table>

### 📖 Complete Documentation

- **[Getting Started Hub](docs/getting-started/00-README.md)** - All learning paths and resources
- **[Interactive Examples](examples/TaskListProcessor.Web)** - Try it in your browser (run the web demo)
- **[Architecture Deep Dive](docs/architecture/design-principles.md)** - SOLID principles and design patterns
- **[Performance Guide](docs/architecture/performance-considerations.md)** - Benchmarks and optimization
- **[FAQ](docs/troubleshooting/faq.md)** - 40+ common questions answered
- **[API Reference](docs/api-reference/)** - Complete API documentation

### 🆘 Need Help?

- **Quick answers**: [FAQ](docs/troubleshooting/faq.md)
- **Common issues**: [Troubleshooting Guide](docs/troubleshooting/common-issues.md)
- **Ask questions**: [GitHub Discussions](https://github.com/markhazleton/TaskListProcessor/discussions)
- **Report bugs**: [GitHub Issues](https://github.com/markhazleton/TaskListProcessor/issues)

---

## 🏗️ Architecture

TaskListProcessor implements a modern, enterprise-ready architecture with clear separation of concerns:

```ascii
┌─────────────────────────────────────────────────────────────────┐
│                    Dependency Injection Layer                  │
│        services.AddTaskListProcessor().WithAllDecorators()     │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Decorator Chain                            │
│  LoggingDecorator → MetricsDecorator → CircuitBreakerDecorator  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Interface Segregation Layer                 │
│  ITaskProcessor │ ITaskBatchProcessor │ ITaskStreamProcessor   │
│              ITaskTelemetryProvider                           │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Core Processing Engine                       │
│            TaskListProcessorEnhanced (Backward Compatible)     │
└─────────────────────────────────────────────────────────────────┘
                                │
              ┌─────────────────┼─────────────────┐
              │                 │                 │
    ┌─────────▼────────┐ ┌─────▼──────┐ ┌───────▼──────┐
    │ TaskDefinition   │ │TaskTelemetry│ │TaskProgress  │
    │ + Dependencies   │ │ + Metrics   │ │ + Reporting  │
    │ + Priority       │ │ + Tracing   │ │ + Streaming  │
    │ + Scheduling     │ │ + Health    │ │ + Estimates  │
    └──────────────────┘ └────────────┘ └──────────────┘
```

### Core Components

- **Interface Layer**: Clean, focused interfaces for different processing scenarios
- **Decorator Layer**: Cross-cutting concerns (logging, metrics, circuit breakers)
- **Processing Engine**: Thread-safe orchestration with advanced scheduling
- **Telemetry System**: Comprehensive observability and health monitoring
- **Dependency Resolution**: Topological sorting and execution ordering
- **Circuit Breaker**: Cascading failure prevention and automatic recovery

## ⚡ Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/markhazleton/TaskListProcessor.git
cd TaskListProcessor

# Build the solution
dotnet build

# Run the demo
dotnet run --project examples/TaskListProcessor.Console
```

### Basic Usage (Direct Instantiation)

```csharp
using TaskListProcessing.Core;
using Microsoft.Extensions.Logging;

// Set up logging (optional but recommended)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

// Create the processor
using var processor = new TaskListProcessorEnhanced("My Tasks", logger);

// Define your tasks using the factory pattern
var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Weather Data"] = async ct => await GetWeatherAsync("London"),
    ["Stock Prices"] = async ct => await GetStockPricesAsync("MSFT"),
    ["User Data"] = async ct => await GetUserDataAsync(userId)
};

// Execute all tasks concurrently
await processor.ProcessTasksAsync(taskFactories, cancellationToken);

// Access results and telemetry
foreach (var result in processor.TaskResults)
{
    Console.WriteLine($"{result.Name}: {(result.IsSuccessful ? "✅" : "❌")}");
}
```

### Dependency Injection Usage (Recommended)

```csharp
using TaskListProcessing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Program.cs
var builder = Host.CreateApplicationBuilder(args);

// Configure TaskListProcessor with decorators
builder.Services.AddTaskListProcessor(options =>
{
    options.MaxConcurrentTasks = 10;
    options.EnableDetailedTelemetry = true;
    options.CircuitBreakerOptions = new() { FailureThreshold = 3 };
})
.WithLogging()
.WithMetrics()
.WithCircuitBreaker();

var host = builder.Build();

// Usage in your services
public class MyService
{
    private readonly ITaskBatchProcessor _processor;
    
    public MyService(ITaskBatchProcessor processor)
    {
        _processor = processor;
    }
    
    public async Task ProcessDataAsync()
    {
        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["API Call"] = async ct => await CallApiAsync(ct),
            ["DB Query"] = async ct => await QueryDatabaseAsync(ct)
        };
        
        await _processor.ProcessTasksAsync(tasks);
    }
}
```

## 📖 Comprehensive Examples

### Travel Dashboard (Real-world Scenario)

This example demonstrates fetching weather and activities data for multiple cities:

```csharp
using var processor = new TaskListProcessorEnhanced("Travel Dashboard", logger);
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

var cities = new[] { "London", "Paris", "Tokyo", "New York" };
var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

// Create tasks for each city
foreach (var city in cities)
{
    taskFactories[$"{city} Weather"] = ct => weatherService.GetWeatherAsync(city, ct);
    taskFactories[$"{city} Activities"] = ct => activitiesService.GetActivitiesAsync(city, ct);
}

// Execute and handle results
try 
{
    await processor.ProcessTasksAsync(taskFactories, cts.Token);
    
    // Group results by city
    var cityData = processor.TaskResults
        .GroupBy(r => r.Name.Split(' ')[0])
        .ToDictionary(g => g.Key, g => g.ToList());
    
    // Display results with rich formatting
    foreach (var (city, results) in cityData)
    {
        Console.WriteLine($"\n🌍 {city}:");
        foreach (var result in results)
        {
            var status = result.IsSuccessful ? "✅" : "❌";
            Console.WriteLine($"  {status} {result.Name.Split(' ')[1]}");
        }
    }
}
catch (OperationCanceledException)
{
    logger.LogWarning("Operation timed out after 2 minutes");
}
```

### Individual Task Execution

For scenarios requiring fine-grained control:

```csharp
// Execute a single task with full telemetry
var result = await processor.ExecuteTaskAsync(
    "Critical API Call", 
    httpClient.GetStringAsync("https://api.example.com/data"),
    cancellationToken);

if (result.IsSuccessful)
{
    var data = result.Data;
    logger.LogInformation("Received {DataLength} characters", data?.Length ?? 0);
}
else
{
    logger.LogError("API call failed: {Error}", result.ErrorMessage);
}
```

## 🔧 Advanced Usage

### Task Dependencies & Scheduling

```csharp
using TaskListProcessing.Models;
using TaskListProcessing.Scheduling;

// Configure with dependency resolution
var options = new TaskListProcessorOptions
{
    DependencyResolver = new TopologicalTaskDependencyResolver(),
    SchedulingStrategy = TaskSchedulingStrategy.Priority,
    MaxConcurrentTasks = Environment.ProcessorCount * 2
};

using var processor = new TaskListProcessorEnhanced("Advanced Tasks", logger, options);

// Define tasks with dependencies and priorities
var taskDefinitions = new[]
{
    new TaskDefinition
    {
        Name = "Initialize",
        Factory = async ct => await InitializeAsync(ct),
        Priority = TaskPriority.High
    },
    new TaskDefinition
    {
        Name = "Process Data",
        Factory = async ct => await ProcessDataAsync(ct),
        Dependencies = new[] { "Initialize" },
        Priority = TaskPriority.Medium
    },
    new TaskDefinition
    {
        Name = "Generate Report",
        Factory = async ct => await GenerateReportAsync(ct),
        Dependencies = new[] { "Process Data" },
        Priority = TaskPriority.Low
    }
};

await processor.ProcessTaskDefinitionsAsync(taskDefinitions);
```

### Circuit Breaker Configuration

```csharp
var options = new TaskListProcessorOptions
{
    CircuitBreakerOptions = new CircuitBreakerOptions
    {
        FailureThreshold = 5,
        RecoveryTimeout = TimeSpan.FromMinutes(2),
        MinimumThroughput = 10
    }
};

using var processor = new TaskListProcessorEnhanced("Resilient Tasks", logger, options);

// Tasks will automatically trigger circuit breaker on repeated failures
var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Resilient API"] = async ct => await CallExternalApiAsync(ct),
    ["Fallback Service"] = async ct => await CallFallbackServiceAsync(ct)
};

await processor.ProcessTasksAsync(taskFactories);

// Check circuit breaker status
var cbStats = processor.CircuitBreakerStats;
if (cbStats?.State == CircuitBreakerState.Open)
{
    Console.WriteLine($"Circuit breaker opened at {cbStats.OpenedAt}");
}
```

### Streaming Results

```csharp
using TaskListProcessing.Interfaces;

// Inject the stream processor
public class StreamingService
{
    private readonly ITaskStreamProcessor _streamProcessor;
    
    public StreamingService(ITaskStreamProcessor streamProcessor)
    {
        _streamProcessor = streamProcessor;
    }
    
    public async Task ProcessWithStreamingAsync()
    {
        var tasks = CreateLongRunningTasks();
        
        // Process results as they complete
        await foreach (var result in _streamProcessor.ProcessTasksStreamAsync(tasks))
        {
            Console.WriteLine($"Completed: {result.Name} - {result.IsSuccessful}");
            
            // Process result immediately without waiting for all tasks
            await HandleResultAsync(result);
        }
    }
}
```

### Health Monitoring

```csharp
var options = new TaskListProcessorOptions
{
    HealthCheckOptions = new HealthCheckOptions
    {
        MinSuccessRate = 0.8, // 80% success rate threshold
        MaxAverageExecutionTime = TimeSpan.FromSeconds(5),
        IncludeCircuitBreakerState = true
    }
};

using var processor = new TaskListProcessorEnhanced("Health Monitored", logger, options);

// After processing tasks
var healthResult = processor.PerformHealthCheck();
if (!healthResult.IsHealthy)
{
    Console.WriteLine($"Health check failed: {healthResult.Message}");
}

// Get detailed telemetry
var telemetrySummary = processor.GetTelemetrySummary();
Console.WriteLine($"Success rate: {telemetrySummary.SuccessRate:F1}%");
Console.WriteLine($"Average execution time: {telemetrySummary.AverageExecutionTime:F0}ms");
```

## 📊 Performance & Telemetry

TaskListProcessor provides comprehensive telemetry out of the box:

### Built-in Metrics

```csharp
// Access telemetry after execution
var telemetry = processor.Telemetry;
var successRate = telemetry.Count(t => t.IsSuccessful) / (double)telemetry.Count * 100;
var averageTime = telemetry.Average(t => t.DurationMs);
var throughput = telemetry.Count / telemetry.Max(t => t.DurationMs) * 1000;

Console.WriteLine($"📊 Success Rate: {successRate:F1}%");
Console.WriteLine($"⏱️ Average Time: {averageTime:F0}ms");
Console.WriteLine($"🚀 Throughput: {throughput:F1} tasks/second");
```

### Sample Telemetry Output

```text
=== 📊 TELEMETRY SUMMARY ===
📈 Total Tasks: 16
✅ Successful: 13 (81.2%)
❌ Failed: 3
⏱️ Average Time: 1,305ms
🏃 Fastest: 157ms | 🐌 Slowest: 2,841ms
⏰ Total Execution Time: 20,884ms

=== 📋 DETAILED TELEMETRY ===
✅ Successful Tasks (sorted by execution time):
  🚀 London Things To Do: 157ms
  🚀 Dallas Things To Do: 339ms
  ⚡ Chicago Things To Do: 557ms
  🏃 London Weather: 1,242ms
  ...

❌ Failed Tasks:
  💥 Sydney Things To Do: ArgumentException after 807ms
  💥 Tokyo Things To Do: ArgumentException after 424ms
```

## 🛠️ API Reference

### Core Interfaces

| Interface | Description | Key Methods |
|-----------|-------------|-------------|
| `ITaskProcessor` | Single task execution | `ExecuteTaskAsync<T>()` |
| `ITaskBatchProcessor` | Batch processing | `ProcessTasksAsync()`, `ProcessTaskDefinitionsAsync()` |
| `ITaskStreamProcessor` | Streaming results | `ProcessTasksStreamAsync()` |
| `ITaskTelemetryProvider` | Telemetry & health | `GetTelemetrySummary()`, `PerformHealthCheck()` |

### TaskListProcessorEnhanced (Backward Compatible)

| Method | Description | Returns |
|--------|-------------|---------|
| `ProcessTasksAsync(taskFactories, progress, ct)` | Execute multiple tasks concurrently | `Task` |
| `ProcessTaskDefinitionsAsync(definitions, progress, ct)` | Execute tasks with dependencies | `Task` |
| `ExecuteTaskAsync<T>(name, task, ct)` | Execute single task with telemetry | `Task<EnhancedTaskResult<T>>` |
| `ProcessTasksStreamAsync(taskFactories, ct)` | Stream results as they complete | `IAsyncEnumerable<EnhancedTaskResult<object>>` |
| `GetTelemetrySummary()` | Get comprehensive telemetry | `TelemetrySummary` |
| `PerformHealthCheck()` | Check processor health | `HealthCheckResult` |

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `MaxConcurrentTasks` | `int` | `Environment.ProcessorCount * 2` | Maximum concurrent tasks |
| `DefaultTaskTimeout` | `TimeSpan` | `5 minutes` | Default task timeout |
| `EnableDetailedTelemetry` | `bool` | `true` | Enable comprehensive telemetry |
| `CircuitBreakerOptions` | `CircuitBreakerOptions?` | `null` | Circuit breaker configuration |
| `SchedulingStrategy` | `TaskSchedulingStrategy` | `FirstInFirstOut` | Task scheduling strategy |
| `DependencyResolver` | `ITaskDependencyResolver?` | `null` | Dependency resolution |

### Data Models

| Model | Description | Key Properties |
|-------|-------------|----------------|
| `EnhancedTaskResult<T>` | Task execution result | `Data`, `IsSuccessful`, `ErrorMessage`, `ErrorCategory` |
| `TaskTelemetry` | Telemetry data | `TaskName`, `ElapsedMilliseconds`, `IsSuccessful` |
| `TaskProgress` | Progress information | `CompletedTasks`, `TotalTasks`, `CurrentTask` |
| `TaskDefinition` | Task with metadata | `Name`, `Factory`, `Dependencies`, `Priority` |
| `CircuitBreakerStats` | Circuit breaker state | `State`, `FailureCount`, `OpenedAt` |
| `TelemetrySummary` | Aggregated telemetry | `SuccessRate`, `AverageExecutionTime`, `TotalTasks` |

## 🧪 Testing

Run the comprehensive test suite:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Integration
```

### Test Coverage

- ✅ **Unit Tests**: Core functionality and edge cases
- ✅ **Integration Tests**: End-to-end scenarios  
- ✅ **Performance Tests**: Throughput and latency validation
- ✅ **Stress Tests**: High-concurrency scenarios

## 🏗️ Project Structure

TaskListProcessor follows a clean architecture with clear separation of concerns:

```text
TaskListProcessor/
├── src/
│   ├── TaskListProcessing/                    # Core library
│   │   ├── Core/                             # Core implementations
│   │   │   ├── TaskListProcessorEnhanced.cs  # Main orchestrator
│   │   │   ├── TaskProcessor.cs              # Single task execution
│   │   │   ├── TaskBatchProcessor.cs         # Batch processing
│   │   │   ├── TaskStreamProcessor.cs        # Streaming results
│   │   │   └── TaskTelemetryProvider.cs      # Telemetry collection
│   │   ├── Interfaces/                       # Interface segregation
│   │   │   ├── ITaskProcessor.cs             # Single task interface
│   │   │   ├── ITaskBatchProcessor.cs        # Batch processing interface
│   │   │   ├── ITaskStreamProcessor.cs       # Streaming interface
│   │   │   └── ITaskTelemetryProvider.cs     # Telemetry interface
│   │   ├── Extensions/                       # DI integration
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── TaskProcessorBuilder.cs
│   │   ├── Models/                           # Data models
│   │   │   ├── EnhancedTaskResult.cs
│   │   │   ├── TaskDefinition.cs
│   │   │   ├── TaskProgress.cs
│   │   │   └── HealthCheckResult.cs
│   │   ├── Scheduling/                       # Task scheduling
│   │   │   ├── TaskSchedulingStrategy.cs
│   │   │   ├── AdvancedTaskScheduler.cs
│   │   │   └── TopologicalTaskDependencyResolver.cs
│   │   ├── CircuitBreaker/                   # Circuit breaker pattern
│   │   │   ├── CircuitBreaker.cs
│   │   │   └── CircuitBreakerOptions.cs
│   │   ├── LoadBalancing/                    # Load balancing
│   │   │   ├── LoadBalancingStrategy.cs
│   │   │   └── LoadBalancingTaskDistributor.cs
│   │   ├── Telemetry/                        # Telemetry & metrics
│   │   │   ├── TaskTelemetry.cs
│   │   │   ├── TelemetrySummary.cs
│   │   │   └── SchedulerStats.cs
│   │   ├── Options/                          # Configuration
│   │   │   ├── TaskListProcessorOptions.cs
│   │   │   ├── CircuitBreakerOptions.cs
│   │   │   └── HealthCheckOptions.cs
│   │   ├── Decorators/                       # Cross-cutting concerns
│   │   │   ├── LoggingTaskProcessorDecorator.cs
│   │   │   └── MetricsTaskProcessorDecorator.cs
│   │   ├── Testing/                          # Test utilities
│   │   │   └── TaskListProcessorTestHelpers.cs
│   │   └── Utilities/                        # Helper classes
│   ├── CityWeatherService/                   # Example service
│   │   ├── WeatherService.cs
│   │   └── CityWeatherService.csproj
│   └── CityThingsToDo/                       # Example service
│       ├── CityThingsToDoService.cs
│       └── CityThingsToDo.csproj
├── examples/
│   └── TaskListProcessor.Console/            # Demo application
│       ├── Program.cs
│       └── Utilities/
│           ├── AppConfiguration.cs
│           ├── OutputFormatter.cs
│           ├── ResultsDisplay.cs
│           └── TelemetryDisplay.cs
├── tests/
│   ├── TaskListProcessing.Tests/             # Core library tests
│   │   ├── InterfaceSegregationTests.cs
│   │   └── TaskListProcessing.Tests.csproj
│   ├── CityWeatherService.Tests/             # Service tests
│   │   ├── WeatherServiceTests.cs
│   │   └── CityWeatherService.Tests.csproj
│   └── CityThingsToDo.Tests/                 # Service tests
│       ├── CityThingsToDoServiceTests.cs
│       └── CityThingsToDo.Tests.csproj
├── docs/                                     # Documentation
│   ├── PHASE1_README.md                      # Phase 1 features
│   ├── MIGRATION_GUIDE.md                    # Migration guide
│   └── CLEANUP_SUMMARY.md                    # Cleanup notes
└── README.md                                 # This file
```

### Key Architecture Principles

- **Interface Segregation**: Clean, focused interfaces for different scenarios
- **Dependency Injection**: Native .NET DI with fluent configuration
- **Single Responsibility**: Each component has a clear, focused purpose
- **Extensibility**: Decorator pattern for cross-cutting concerns
- **Testability**: Mockable interfaces and comprehensive test coverage

## 🔄 Migration Guide

### From Legacy Processors to Modern Interfaces

**Recommended Migration Path:**

1. **Migrate to Dependency Injection** (Recommended)

```csharp
// Old approach
var processor = new TaskListProcessorEnhanced("Tasks", logger);

// New approach
services.AddTaskListProcessor(options =>
{
    options.MaxConcurrentTasks = 10;
    options.EnableDetailedTelemetry = true;
})
.WithLogging()
.WithMetrics();

// In your service
public class MyService
{
    private readonly ITaskBatchProcessor _processor;
    public MyService(ITaskBatchProcessor processor) => _processor = processor;
}
```

2. **Direct Interface Usage** (Alternative)

```csharp
// Single task processing
var taskProcessor = new TaskProcessor("SingleTasks", logger);
var result = await taskProcessor.ExecuteTaskAsync("task", someTask);

// Batch processing
var batchProcessor = new TaskBatchProcessor("BatchTasks", logger);
await batchProcessor.ProcessTasksAsync(taskFactories);

// Streaming results
var streamProcessor = new TaskStreamProcessor("StreamTasks", logger);
await foreach (var result in streamProcessor.ProcessTasksStreamAsync(tasks))
{
    // Process results as they complete
}
```

3. **Backward Compatibility** (For existing code)

```csharp
// TaskListProcessorEnhanced still works with all existing features
using var processor = new TaskListProcessorEnhanced("Legacy", logger);
await processor.ProcessTasksAsync(taskFactories);
```

### New Features in Current Version

- **Interface Segregation**: Clean, focused interfaces for different scenarios
- **Dependency Injection**: Native .NET DI integration with fluent configuration
- **Task Dependencies**: Topological sorting and dependency resolution
- **Circuit Breaker**: Automatic failure detection and recovery
- **Advanced Scheduling**: Priority-based, dependency-aware task execution
- **Streaming Results**: Real-time result processing via async enumerables
- **Enhanced Telemetry**: OpenTelemetry integration and health monitoring
- **Memory Optimization**: Object pooling and efficient resource management

### Breaking Changes

1. **Namespace Changes**: Main classes moved to `TaskListProcessing.Core`
2. **Interface Requirements**: New interfaces may require additional dependencies
3. **Configuration Options**: Enhanced options structure with validation
4. **Result Types**: Enhanced error categorization and telemetry data

## 🤝 Contributing

We welcome contributions! Here's how to get started:

### Development Setup

```bash
# Clone and setup
git clone https://github.com/markhazleton/TaskListProcessor.git
cd TaskListProcessor

# Restore dependencies
dotnet restore

# Build solution  
dotnet build

# Run tests
dotnet test
```

### Contribution Guidelines

1. **🍴 Fork** the repository
2. **🌿 Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **✨ Make** your changes with tests
4. **✅ Verify** all tests pass (`dotnet test`)
5. **📝 Commit** your changes (`git commit -m 'Add amazing feature'`)
6. **🚀 Push** to the branch (`git push origin feature/amazing-feature`)
7. **🎯 Open** a Pull Request

### Code Standards

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Add XML documentation for public APIs
- Include unit tests for new features
- Maintain backward compatibility when possible

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```text
MIT License

Copyright (c) 2024 Mark Hazleton

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

## 🔗 Resources

### 📚 Documentation & Articles

- [📖 Phase 1 Features Guide](docs/PHASE1_README.md) - Interface segregation and dependency injection
- [📝 Migration Guide](docs/MIGRATION_GUIDE.md) - Detailed migration instructions
- [🔄 Cleanup Summary](docs/CLEANUP_SUMMARY.md) - Recent improvements and changes
- [🌐 Complete Technical Article](https://markhazleton.com/task-list-processor.html) - Deep dive into architecture and patterns
- [� Best Practices Guide](https://markhazleton.com/async-best-practices.html) - Async programming patterns

### 🎯 Examples & Demos

- [🖥️ Console Demo](examples/TaskListProcessor.Console/) - Interactive demonstration
- [🌐 Web Dashboard Example](examples/TaskListProcessor.Web/) - ASP.NET Core integration
- [📊 Performance Benchmarks](benchmarks/) - Performance analysis and comparisons

### 👥 Community & Support

- [🐛 Report Issues](https://github.com/markhazleton/TaskListProcessor/issues) - Bug reports and feature requests
- [💬 Discussions](https://github.com/markhazleton/TaskListProcessor/discussions) - Community support and Q&A
- [📧 Contact Mark](mailto:mark@markhazleton.com) - Direct contact for enterprise support

### 🔗 Mark Hazleton Online

- [🌐 Website](https://markhazleton.com) - Blog and technical articles
- [💼 LinkedIn](https://www.linkedin.com/in/markhazleton) - Professional network
- [📺 YouTube](https://www.youtube.com/@MarkHazleton) - Technical tutorials and demos
- [📸 Instagram](https://www.instagram.com/markhazleton/) - Behind the scenes content

---

## 🚀 Get Started Today

Ready to supercharge your async operations? Get started with TaskListProcessor:

```bash
git clone https://github.com/markhazleton/TaskListProcessor.git
cd TaskListProcessor
dotnet run --project examples/TaskListProcessor.Console
```

**See it in action with our interactive demo that showcases:**

- 🌍 Multi-city travel data aggregation with dependency resolution
- ⚡ Concurrent API calls with circuit breaker protection
- 📊 Rich telemetry with OpenTelemetry integration
- 🎯 Type-safe result processing with error categorization
- ⏱️ Advanced scheduling with priority-based execution
- 🔄 Streaming results via async enumerables
- 🏗️ Dependency injection with decorator pattern support

*Built with ❤️ by [Mark Hazleton](https://markhazleton.com) • Follow for more .NET content and best practices*

---

## 📈 Project Stats

![GitHub stars](https://img.shields.io/github/stars/markhazleton/TaskListProcessor?style=social)
![GitHub forks](https://img.shields.io/github/forks/markhazleton/TaskListProcessor?style=social)
![GitHub issues](https://img.shields.io/github/issues/markhazleton/TaskListProcessor)
![GitHub last commit](https://img.shields.io/github/last-commit/markhazleton/TaskListProcessor)

*⭐ If this project helped you, please consider giving it a star!*
