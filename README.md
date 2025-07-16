# 🚀 TaskListProcessor

[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square)](https://github.com/markhazleton/TaskListProcessor)
[![NuGet](https://img.shields.io/badge/NuGet-Coming%20Soon-orange?style=flat-square)](https://www.nuget.org/packages/TaskListProcessor)

> **A modern, thread-safe .NET library for orchestrating asynchronous operations with enterprise-grade telemetry, error handling, and cancellation support.**

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
  - [🤝 Contributing](#-contributing)
  - [📜 License](#-license)
  - [🔗 Resources](#-resources)

---

## ✨ Overview

TaskListProcessor is a production-ready .NET library designed to solve the common challenge of orchestrating multiple asynchronous operations while maintaining observability, reliability, and performance. Whether you're building dashboards, data aggregation pipelines, or distributed systems, TaskListProcessor provides the robust foundation you need.

### 🎯 Why TaskListProcessor?

**The Problem:** Modern applications often need to coordinate multiple async operations—API calls, database queries, file I/O—while handling failures gracefully and providing meaningful telemetry. Traditional approaches using `Task.WhenAll()` are fragile and lack observability.

**The Solution:** TaskListProcessor provides a battle-tested framework with:

- 🛡️ **Isolation**: One task failure doesn't crash the entire operation
- 📊 **Observability**: Rich telemetry and structured logging
- ⚡ **Performance**: Optimized concurrent execution with configurable parallelism
- 🎯 **Type Safety**: Strongly-typed results with comprehensive error information
- 🔧 **Extensibility**: Plugin architecture for custom scenarios

## 🔥 Key Features

### Core Capabilities

- **🚀 Concurrent Execution**: Parallel task processing with configurable concurrency limits
- **🛡️ Fault Isolation**: Individual task failures don't affect other operations
- **📊 Rich Telemetry**: Comprehensive timing, success rates, and error tracking
- **🎯 Type Safety**: Strongly-typed results with full IntelliSense support
- **⏱️ Timeout & Cancellation**: Built-in support for graceful shutdown and timeouts

### Enterprise Features

- **🧵 Thread Safety**: Concurrent collections and lock-free operations
- **📝 Structured Logging**: Integration with Microsoft.Extensions.Logging
- **🔄 Resource Management**: Proper IDisposable implementation and cleanup
- **📈 Performance Monitoring**: Built-in metrics for throughput and latency
- **🎨 Extensible Design**: Plugin-based architecture for custom scenarios

### Developer Experience

- **📖 Comprehensive Documentation**: Inline XML docs and rich examples
- **🧪 Thorough Testing**: Extensive unit and integration test coverage
- **🎪 Interactive Demo**: Full-featured console application showcasing all features
- **🔍 Debug Support**: Rich diagnostic information and error context

## 🏗️ Architecture

TaskListProcessor follows modern .NET design principles with a clean, extensible architecture:

```ascii
┌─────────────────────────────────────────────────────────────┐
│                TaskListProcessorEnhanced                   │
├─────────────────────────────────────────────────────────────┤
│ + ProcessTasksAsync()     │ Thread-safe task orchestration │
│ + ExecuteTaskAsync<T>()   │ Individual task execution     │
│ + WhenAllWithLoggingAsync() │ Batch processing utilities  │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
    ┌─────────▼────────┐ ┌────▼─────┐ ┌──────▼──────┐
    │ EnhancedTaskResult │ │TaskTelemetry│ │TaskListOptions│
    │ + Data           │ │ + Duration │ │ + MaxConcurrency│
    │ + IsSuccessful   │ │ + Success  │ │ + Timeout      │
    │ + ErrorMessage   │ │ + Exception│ │ + RetryPolicy  │
    └──────────────────┘ └──────────┘ └─────────────┘
```

### Core Components

- **TaskListProcessorEnhanced**: The main orchestrator with thread-safe execution
- **EnhancedTaskResult&lt;T&gt;**: Strongly-typed results with comprehensive error information  
- **TaskTelemetry**: Rich performance and diagnostic data
- **TaskListOptions**: Configuration for advanced scenarios

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

### Basic Usage

```csharp
using TaskListProcessing;
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

### Custom Configuration

```csharp
var options = new TaskListProcessorOptions
{
    MaxConcurrency = Environment.ProcessorCount * 2,
    DefaultTimeout = TimeSpan.FromSeconds(30),
    RetryPolicy = new ExponentialBackoffRetry(maxRetries: 3)
};

using var processor = new TaskListProcessorEnhanced("Advanced Tasks", logger, options);
```

### Error Handling & Resilience

```csharp
// Built-in timeout and cancellation support
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Resilient Task"] = async ct => 
    {
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        combinedCts.CancelAfter(TimeSpan.FromSeconds(10)); // Per-task timeout
        
        return await SomeResilientOperation(combinedCts.Token);
    }
};

await processor.ProcessTasksAsync(taskFactories, timeoutCts.Token);
```

### Custom Result Types

```csharp
public record WeatherData(string City, int Temperature, string Condition);
public record ActivityData(string City, List<string> Activities);

// Type-safe execution
var weatherResult = await processor.ExecuteTaskAsync<WeatherData>(
    "London Weather",
    GetWeatherDataAsync("London"),
    cancellationToken);

if (weatherResult.IsSuccessful && weatherResult.Data != null)
{
    var weather = weatherResult.Data;
    Console.WriteLine($"{weather.City}: {weather.Temperature}°F, {weather.Condition}");
}
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

### TaskListProcessorEnhanced

| Method | Description | Returns |
|--------|-------------|---------|
| `ProcessTasksAsync(taskFactories, ct)` | Execute multiple tasks concurrently | `Task` |
| `ExecuteTaskAsync<T>(name, task, ct)` | Execute single task with telemetry | `Task<EnhancedTaskResult<T>>` |
| `WhenAllWithLoggingAsync(tasks, logger, ct)` | Static utility for batch execution | `Task` |

### EnhancedTaskResult&lt;T&gt;

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Task identifier |
| `Data` | `T?` | Task result data |
| `IsSuccessful` | `bool` | Success indicator |
| `ErrorMessage` | `string?` | Error details |
| `Timestamp` | `DateTime` | Completion time |

### TaskTelemetry

| Property | Type | Description |
|----------|------|-------------|
| `TaskName` | `string` | Task identifier |
| `DurationMs` | `long` | Execution time |
| `IsSuccessful` | `bool` | Success status |
| `ExceptionType` | `string?` | Exception type name |
| `ErrorMessage` | `string?` | Error details |

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

## 🔄 Migration Guide

### From TaskListProcessorGeneric

**Old (TaskListProcessorGeneric):**

```csharp
var processor = new TaskListProcessorGeneric();
var tasks = new List<Task>();
tasks.Add(processor.GetTaskResultAsync("Task1", SomeAsync()));
await processor.WhenAllWithLoggingAsync(tasks, logger);
```

**New (TaskListProcessorEnhanced):**

```csharp
using var processor = new TaskListProcessorEnhanced("MyTasks", logger);
var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
{
    ["Task1"] = ct => SomeAsync()
};
await processor.ProcessTasksAsync(taskFactories, cancellationToken: ct);
```

### Breaking Changes

1. **Task Definition**: Factory pattern instead of pre-created tasks
2. **Resource Management**: Implements `IDisposable` - use `using` statements
3. **Result Access**: Use `TaskResults` property instead of separate collection
4. **Cancellation**: Built-in cancellation token support

### Benefits of Migration

- 🛡️ **Better Error Isolation**: Individual task failures don't affect others
- 📊 **Enhanced Telemetry**: Richer performance and diagnostic data
- 🧵 **Thread Safety**: Improved concurrent execution safety
- ⏱️ **Cancellation Support**: Proper timeout and cancellation handling
- 💾 **Resource Management**: Automatic cleanup and disposal

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

- [📖 Complete Technical Article](https://markhazleton.com/task-list-processor.html) - Deep dive into architecture and patterns
- [🔍 API Documentation](https://markhazleton.github.io/TaskListProcessor/) - Complete API reference
- [📝 Best Practices Guide](https://markhazleton.com/async-best-practices.html) - Async programming patterns

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

- 🌍 Multi-city travel data aggregation
- ⚡ Concurrent API calls with error handling
- 📊 Rich telemetry and performance metrics  
- 🎯 Type-safe result processing
- ⏱️ Timeout and cancellation scenarios

*Built with ❤️ by [Mark Hazleton](https://markhazleton.com) • Follow for more .NET content and best practices*

---

## 📈 Project Stats

![GitHub stars](https://img.shields.io/github/stars/markhazleton/TaskListProcessor?style=social)
![GitHub forks](https://img.shields.io/github/forks/markhazleton/TaskListProcessor?style=social)
![GitHub issues](https://img.shields.io/github/issues/markhazleton/TaskListProcessor)
![GitHub last commit](https://img.shields.io/github/last-commit/markhazleton/TaskListProcessor)

*⭐ If this project helped you, please consider giving it a star!*
