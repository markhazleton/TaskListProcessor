namespace TaskListProcessing.Scheduling
{

    /// <summary>
    /// Task scheduling strategies for controlling task execution order.
    /// </summary>
    public enum TaskSchedulingStrategy
    {
        /// <summary>
        /// Tasks are executed in the order they were added (FIFO).
        /// </summary>
        FirstInFirstOut,

        /// <summary>
        /// Tasks are executed in reverse order (LIFO).
        /// </summary>
        LastInFirstOut,

        /// <summary>
        /// Tasks are executed based on priority.
        /// </summary>
        Priority,

        /// <summary>
        /// Tasks are executed based on estimated execution time (shortest first).
        /// </summary>
        ShortestJobFirst,

        /// <summary>
        /// Tasks are distributed randomly to balance load.
        /// </summary>
        Random
    }
}
