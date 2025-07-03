using TaskListProcessing.Models;

namespace TaskListProcessing.Interfaces
{

    /// <summary>
    /// Interface for task dependency resolution.
    /// </summary>
    public interface ITaskDependencyResolver
    {
        /// <summary>
        /// Resolves task dependencies and returns execution order.
        /// </summary>
        /// <param name="tasks">The tasks to resolve dependencies for.</param>
        /// <returns>Tasks ordered by their dependencies.</returns>
        IEnumerable<TaskDefinition> ResolveDependencies(IEnumerable<TaskDefinition> tasks);

        /// <summary>
        /// Validates that there are no circular dependencies.
        /// </summary>
        /// <param name="tasks">The tasks to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        bool ValidateDependencies(IEnumerable<TaskDefinition> tasks);
    }
}
