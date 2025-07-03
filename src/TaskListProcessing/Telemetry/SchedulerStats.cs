using TaskListProcessing.Scheduling;

namespace TaskListProcessing.Telemetry
{

    /// <summary>
    /// Scheduler statistics and metrics.
    /// </summary>
    public record SchedulerStats
    {
        /// <summary>
        /// Gets or sets the number of queued tasks.
        /// </summary>
        public int QueuedTasks { get; init; }

        /// <summary>
        /// Gets or sets the number of currently running tasks.
        /// </summary>
        public int RunningTasks { get; init; }

        /// <summary>
        /// Gets or sets the number of available execution slots.
        /// </summary>
        public int AvailableSlots { get; init; }

        /// <summary>
        /// Gets or sets the scheduling strategy in use.
        /// </summary>
        public TaskSchedulingStrategy Strategy { get; init; }

        /// <summary>
        /// Gets the scheduler utilization percentage.
        /// </summary>
        public double Utilization =>
            QueuedTasks + RunningTasks > 0
                ? (double)RunningTasks / (RunningTasks + AvailableSlots) * 100
                : 0;
    }
}
