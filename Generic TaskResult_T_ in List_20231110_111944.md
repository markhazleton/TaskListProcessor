# Generic TaskResult<T> in List

[![Visit MarkHazleton.com](https://img.shields.io/badge/Visit-MarkHazleton.com-blue)](https://MarkHazleton.com)

---

## Introduction

This document discusses how to update the `TaskListProcessor` class so that the `TaskResult` data is a generic type `T`, and how this impacts working with lists containing multiple types. The patterns here are part of the [TaskListProcessor](https://github.com/MarkHazleton/TaskListProcessor) project.

---

## Question

> Is there a way to update the TaskListProcessor class (below) so that the TaskResult Data is a generic T? How would that impact pulling data out of a list? I still want to be able to have multiple types in a single List.

---

## Solution: Using Generics and Interfaces

### 1. Make `TaskResult` a Generic Class

```csharp
public class TaskResult<T>
{
    public TaskResult()
    {
        Name = "UNKNOWN";
        Data = default;
    }

    public TaskResult(string name, T data)
    {
        Name = name;
        Data = data;
    }

    public T Data { get; set; }
    public string Name { get; set; }
}
```

### 2. Use an Interface for Heterogeneous Lists

```csharp
public interface ITaskResult
{
    string Name { get; set; }
}

public class TaskResult<T> : ITaskResult
{
    // ...implementation as above...
}

public List<ITaskResult> TaskResults { get; set; } = new List<ITaskResult>();
```

### 3. Update `GetTaskResultAsync` Method

```csharp
public async Task GetTaskResultAsync<T>(string taskName, Task<T> task) where T : class
{
    var taskResult = new TaskResult<T> { Name = taskName };
    try
    {
        taskResult.Data = await task;
        // ...telemetry and logging...
    }
    catch (Exception ex)
    {
        // ...error handling...
    }
    finally
    {
        TaskResults.Add(taskResult);
    }
}
```

### 4. Accessing Results from the List

You can cast each `ITaskResult` to the appropriate generic type:

```csharp
foreach (var result in TaskResults)
{
    if (result is TaskResult<MyType> typedResult)
    {
        // Use typedResult.Data
    }
}
```

---

## Impact

- You can store results of various types in a single list.
- Type checking and casting are required when accessing results.
- This approach maintains type safety and flexibility.

---

## Contact

For questions, feedback, or support, please open an [issue](../../issues) in this repository.

---

> Document by Mark Hazleton. Visit [MarkHazleton.com](https://MarkHazleton.com) for more projects and articles.
