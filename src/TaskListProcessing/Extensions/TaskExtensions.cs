using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TaskListProcessing.Extensions;

/// <summary>
/// Extension methods to enforce ConfigureAwait usage in library code.
/// These extensions ensure consistent async behavior and prevent unnecessary context switching.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Library-safe await that always uses ConfigureAwait(false).
    /// Use this for all internal library operations to avoid context switching overhead.
    /// </summary>
    /// <param name="task">The task to await.</param>
    /// <returns>A configured awaitable that will not capture the current context.</returns>
    public static ConfiguredTaskAwaitable SafeAwait(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    /// <summary>
    /// Library-safe await that always uses ConfigureAwait(false).
    /// Use this for all internal library operations to avoid context switching overhead.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <returns>A configured awaitable that will not capture the current context.</returns>
    public static ConfiguredTaskAwaitable<T> SafeAwait<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    /// <summary>
    /// Context-preserving await for operations that need to marshal back to the calling context.
    /// Only use when you need to preserve synchronization context (e.g., UI operations).
    /// </summary>
    /// <param name="task">The task to await.</param>
    /// <returns>A configured awaitable that will capture the current context.</returns>
    public static ConfiguredTaskAwaitable PreserveContextAwait(this Task task)
    {
        return task.ConfigureAwait(true);
    }

    /// <summary>
    /// Context-preserving await for operations that need to marshal back to the calling context.
    /// Only use when you need to preserve synchronization context (e.g., UI operations).
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <returns>A configured awaitable that will capture the current context.</returns>
    public static ConfiguredTaskAwaitable<T> PreserveContextAwait<T>(this Task<T> task)
    {
        return task.ConfigureAwait(true);
    }
}
