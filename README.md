# TaskListProcessor

## Overview
The `TaskListProcessor` class is a .NET utility for efficient asynchronous task management. It enables concurrent execution, result tracking, and detailed telemetry, ideal for applications that need to handle multiple tasks simultaneously.

## Features
- **Concurrent Task Execution**: Run multiple tasks in parallel.
- **Result Tracking**: Capture and store the results of tasks.
- **Performance Telemetry**: Generate detailed telemetry for task performance analysis.
- **Error Logging**: Robust error handling and logging capabilities.

## Usage
```csharp
var taskListProcessor = new TaskListProcessor();
var tasks = new List<Task>
{
    // Add your tasks here
};
await TaskListProcessor.WhenAllWithLoggingAsync(tasks, logger);
```
