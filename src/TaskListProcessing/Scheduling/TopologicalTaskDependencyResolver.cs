using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TaskListProcessing.Interfaces;
using TaskListProcessing.Models;

namespace TaskListProcessing.Scheduling;

/// <summary>
/// Default implementation of task dependency resolver using topological sorting.
/// </summary>
public class TopologicalTaskDependencyResolver : ITaskDependencyResolver
{
    /// <summary>
    /// Resolves task dependencies and returns execution order using topological sort.
    /// </summary>
    /// <param name="tasks">The tasks to resolve dependencies for.</param>
    /// <returns>Tasks ordered by their dependencies.</returns>
    public IEnumerable<TaskDefinition> ResolveDependencies(IEnumerable<TaskDefinition> tasks)
    {
        var taskList = tasks.ToList();
        var taskDict = taskList.ToDictionary(t => t.Name, t => t);
        var result = new List<TaskDefinition>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var task in taskList)
        {
            if (!visited.Contains(task.Name))
            {
                VisitTask(task.Name, taskDict, visited, visiting, result);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that there are no circular dependencies.
    /// </summary>
    /// <param name="tasks">The tasks to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public bool ValidateDependencies(IEnumerable<TaskDefinition> tasks)
    {
        try
        {
            ResolveDependencies(tasks);
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circular dependency"))
        {
            return false;
        }
    }

    private void VisitTask(
        string taskName,
        Dictionary<string, TaskDefinition> taskDict,
        HashSet<string> visited,
        HashSet<string> visiting,
        List<TaskDefinition> result)
    {
        if (visiting.Contains(taskName))
        {
            throw new InvalidOperationException($"Circular dependency detected involving task '{taskName}'");
        }

        if (visited.Contains(taskName))
        {
            return;
        }

        if (!taskDict.TryGetValue(taskName, out var task))
        {
            throw new InvalidOperationException($"Task '{taskName}' not found in task definitions");
        }

        visiting.Add(taskName);

        // Visit all dependencies first
        foreach (var dependency in task.Dependencies)
        {
            VisitTask(dependency, taskDict, visited, visiting, result);
        }

        visiting.Remove(taskName);
        visited.Add(taskName);
        result.Add(task);
    }
}
