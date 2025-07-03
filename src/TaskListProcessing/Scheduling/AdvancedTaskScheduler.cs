using System.Collections.Concurrent;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Scheduling
{

    /// <summary>
    /// Advanced task scheduler with priority and load balancing support.
    /// </summary>
    public class AdvancedTaskScheduler
    {
        private readonly TaskSchedulingStrategy _strategy;
        private readonly int _maxConcurrency;
        private readonly SemaphoreSlim _concurrencyLimiter;
        private readonly ConcurrentQueue<ScheduledTask> _taskQueue = new();
        private readonly ConcurrentDictionary<string, TaskExecutionSlot> _executionSlots = new();
        private readonly Timer _schedulerTimer;
        private volatile bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the AdvancedTaskScheduler class.
        /// </summary>
        /// <param name="strategy">Task scheduling strategy.</param>
        /// <param name="maxConcurrency">Maximum concurrent tasks.</param>
        public AdvancedTaskScheduler(TaskSchedulingStrategy strategy, int maxConcurrency)
        {
            _strategy = strategy;
            _maxConcurrency = maxConcurrency;
            _concurrencyLimiter = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            _schedulerTimer = new Timer(ProcessScheduledTasks, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
        }

        /// <summary>
        /// Schedules a task for execution.
        /// </summary>
        /// <param name="taskDefinition">Task to schedule.</param>
        /// <param name="priority">Optional priority override.</param>
        /// <returns>Scheduled task wrapper.</returns>
        public ScheduledTask ScheduleTask(TaskDefinition taskDefinition, int? priority = null)
        {
            var scheduledTask = new ScheduledTask
            {
                Definition = taskDefinition,
                Priority = priority ?? taskDefinition.Priority,
                ScheduledAt = DateTimeOffset.UtcNow,
                EstimatedDuration = taskDefinition.EstimatedExecutionTime ?? TimeSpan.FromSeconds(1)
            };

            _taskQueue.Enqueue(scheduledTask);
            return scheduledTask;
        }

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        public void Start()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Gets current scheduler statistics.
        /// </summary>
        /// <returns>Scheduler statistics.</returns>
        public SchedulerStats GetStats()
        {
            return new SchedulerStats
            {
                QueuedTasks = _taskQueue.Count,
                RunningTasks = _executionSlots.Count,
                AvailableSlots = _maxConcurrency - _executionSlots.Count,
                Strategy = _strategy
            };
        }

        private void ProcessScheduledTasks(object? state)
        {
            if (!_isRunning) return;

            var availableTasks = new List<ScheduledTask>();

            // Collect available tasks
            while (_taskQueue.TryDequeue(out var task) && availableTasks.Count < _maxConcurrency)
            {
                availableTasks.Add(task);
            }

            if (!availableTasks.Any()) return;

            // Apply scheduling strategy
            var orderedTasks = ApplySchedulingStrategy(availableTasks);

            // Execute tasks based on available slots
            foreach (var task in orderedTasks.Take(_maxConcurrency - _executionSlots.Count))
            {
                _ = ExecuteTaskAsync(task);
            }
        }

        private List<ScheduledTask> ApplySchedulingStrategy(List<ScheduledTask> tasks)
        {
            return _strategy switch
            {
                TaskSchedulingStrategy.Priority => tasks.OrderByDescending(t => t.Priority).ToList(),
                TaskSchedulingStrategy.ShortestJobFirst => tasks.OrderBy(t => t.EstimatedDuration).ToList(),
                TaskSchedulingStrategy.LastInFirstOut => tasks.AsEnumerable().Reverse().ToList(),
                TaskSchedulingStrategy.Random => tasks.OrderBy(_ => Random.Shared.Next()).ToList(),
                _ => tasks // FirstInFirstOut
            };
        }

        private async Task ExecuteTaskAsync(ScheduledTask scheduledTask)
        {
            await _concurrencyLimiter.WaitAsync();

            var slot = new TaskExecutionSlot
            {
                TaskName = scheduledTask.Definition.Name,
                StartTime = DateTimeOffset.UtcNow,
                EstimatedCompletion = DateTimeOffset.UtcNow + scheduledTask.EstimatedDuration
            };

            _executionSlots.TryAdd(scheduledTask.Definition.Name, slot);

            try
            {
                scheduledTask.StartTime = DateTimeOffset.UtcNow;
                scheduledTask.Status = TaskStatus.Running;

                var result = await scheduledTask.Definition.Factory(CancellationToken.None);

                scheduledTask.CompletionTime = DateTimeOffset.UtcNow;
                scheduledTask.Status = TaskStatus.RanToCompletion;
                scheduledTask.Result = result;
            }
            catch (Exception ex)
            {
                scheduledTask.CompletionTime = DateTimeOffset.UtcNow;
                scheduledTask.Status = TaskStatus.Faulted;
                scheduledTask.Exception = ex;
            }
            finally
            {
                _executionSlots.TryRemove(scheduledTask.Definition.Name, out _);
                _concurrencyLimiter.Release();
            }
        }
    }
}
