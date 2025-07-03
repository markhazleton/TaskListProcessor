namespace TaskListProcessing.LoadBalancing
{

    /// <summary>
    /// Load balancing strategies for task distribution.
    /// </summary>
    public enum LoadBalancingStrategy
    {
        /// <summary>
        /// Distribute tasks in round-robin fashion.
        /// </summary>
        RoundRobin,

        /// <summary>
        /// Distribute tasks to the least loaded processor.
        /// </summary>
        LeastLoaded,

        /// <summary>
        /// Weighted round-robin based on processor capabilities.
        /// </summary>
        WeightedRoundRobin,

        /// <summary>
        /// Group similar tasks on the same processor for affinity.
        /// </summary>
        TaskAffinity
    }
}
