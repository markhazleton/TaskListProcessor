# Task List Processing

> For a deep-dive and the latest updates, see the full article by Mark Hazleton: [Task List Processor on MarkHazleton.com](https://markhazleton.com/task-list-processor.html)

---

TaskListProcessor is a .NET utility for orchestrating and monitoring multiple asynchronous operations, with a focus on type safety, error handling, and telemetry. This README provides a practical overview, usage examples, and essential technical details. For a more comprehensive discussion, advanced scenarios, and the story behind the project, please visit the [Task List Processor article](https://markhazleton.com/task-list-processor.html).

## Table of Contents

- [Task List Processing](#task-list-processing)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Key Features](#key-features)
  - [How It Works](#how-it-works)
  - [Example Usage](#example-usage)
  - [Technical Details](#technical-details)
  - [Version Compatibility](#version-compatibility)
  - [Contributing](#contributing)
  - [Links \& Further Reading](#links--further-reading)

---

## Overview

TaskListProcessor helps .NET developers efficiently manage concurrent asynchronous operations, aggregate results of different types, and log telemetry and errors. It is ideal for scenarios like dashboards, data aggregation, and any workflow requiring robust parallel task management.

## Key Features

- **Concurrent Task Execution**: Run multiple tasks in parallel, each with its own result type.
- **Result Aggregation**: Collect and process results in a type-safe, extensible way.
- **Telemetry & Logging**: Built-in support for logging task durations, errors, and completion status.
- **Robust Error Handling**: Prevents a single task failure from affecting the entire operation.
- **Extensible Design**: Easily integrate new data sources or result types.

## How It Works

TaskListProcessor uses generics and a flexible result object to manage tasks of different types. It provides methods like `WhenAllWithLoggingAsync` for error-aware parallel execution, and `GetTaskResultAsync` for telemetry-wrapped task execution.

> **For a detailed architectural discussion, see the [Technical Jargon Explained](https://markhazleton.com/task-list-processor.html#technical-jargon-explained) and [How It Works](https://markhazleton.com/task-list-processor.html#the-whenallwithloggingasync-method) sections in the article.**

## Example Usage

Below is a simplified example. For a full walkthrough, see the [article's code samples](https://markhazleton.com/task-list-processor.html#a-travel-dashboard-using-tasklistprocessor).

```csharp
var thingsToDoService = new CityThingsToDoService();
var weatherService = new WeatherService();
var cityDashboards = new TaskListProcessorGeneric();
var cities = new List<string> { "London", "Paris", "New York", "Tokyo", "Sydney", "Chicago", "Dallas", "Wichita" };
var tasks = new List<Task>();
foreach (var city in cities)
{
  tasks.Add(cityDashboards.GetTaskResultAsync($"{city} Weather", weatherService.GetWeather(city)));
  tasks.Add(cityDashboards.GetTaskResultAsync($"{city} Things To Do", thingsToDoService.GetThingsToDoAsync(city)));
}
await cityDashboards.WhenAllWithLoggingAsync(tasks, logger);
```

**Sample Output:**

```
All tasks completed

Telemetry:
Chicago: Task completed in 602 ms with ERROR Exception: Random failure occurred fetching weather data.
Paris: Task completed in 723 ms with ERROR Exception: Random failure occurred fetching weather data.
...etc...
```

## Technical Details

- **WhenAllWithLoggingAsync**: Waits for all tasks, logs exceptions, and prevents a single failure from halting the process.
- **GetTaskResultAsync**: Wraps each task with telemetry and error logging.
- **TaskResult<T>**: Encapsulates the result, name, and error (if any) for each task.

For full method signatures and advanced usage, see the [Technical Details](https://markhazleton.com/task-list-processor.html#the-gettaskresultasync-method) section in the article.

## Version Compatibility

- Developed for **.NET 8**. Regularly updated to support the latest .NET features.
- See [Version Compatibility and Updates](https://markhazleton.com/task-list-processor.html#version-compatibility-and-updates) for more info.

## Contributing

Contributions are welcome! Please open an [issue](../../issues) or submit a pull request. For questions or support, open an [issue](../../issues) in this repository.

## Links & Further Reading

- [Task List Processor Article (MarkHazleton.com)](https://markhazleton.com/task-list-processor.html)
- [GitHub Repository](https://github.com/markhazleton/TaskListProcessor)
- [LinkedIn: Mark Hazleton](https://www.linkedin.com/in/markhazleton)
- [YouTube: Mark Hazleton](https://www.youtube.com/@MarkHazleton)
- [Instagram: Mark Hazleton](https://www.instagram.com/markhazleton/)

---

*This README is designed to complement the [Task List Processor article](https://markhazleton.com/task-list-processor.html). For the full story, advanced scenarios, and the latest updates, please read the article.*
