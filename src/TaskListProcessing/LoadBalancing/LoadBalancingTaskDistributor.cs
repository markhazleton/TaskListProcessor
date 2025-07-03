using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Telemetry;

namespace TaskListProcessing.LoadBalancing
{

    /// <summary>
    /// Load balancing task distributor for multiple processor instances.
    /// </summary>
    public class LoadBalancingTaskDistributor : IDisposable
    {
        private readonly List<TaskListProcessorEnhanced> _processors;
        private readonly LoadBalancingStrategy _strategy;
        private readonly object _lock = new();
        private int _currentIndex = 0;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the LoadBalancingTaskDistributor class.
        /// </summary>
        /// <param name="processors">Collection of processors to distribute tasks across.</param>
        /// <param name="strategy">Load balancing strategy to use.</param>
        public LoadBalancingTaskDistributor(
            IEnumerable<TaskListProcessorEnhanced> processors,
            LoadBalancingStrategy strategy = LoadBalancingStrategy.RoundRobin)
        {
            _processors = processors?.ToList() ?? throw new ArgumentNullException(nameof(processors));
            _strategy = strategy;

            if (!_processors.Any())
                throw new ArgumentException("At least one processor is required", nameof(processors));
        }

        /// <summary>
        /// Distributes tasks across processors using the configured strategy.
        /// </summary>
        /// <param name="taskDefinitions">Tasks to distribute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the completion of all distributed tasks.</returns>
        public async Task DistributeTasksAsync(
            IEnumerable<TaskDefinition> taskDefinitions,
            CancellationToken cancellationToken = default)
        {
            var tasks = taskDefinitions.ToList();
            var distributedTasks = new List<Task>();

            // Group tasks by processor
            var taskGroups = DistributeTasksToProcessors(tasks);

            // Execute tasks on each processor
            foreach (var (processor, processorTasks) in taskGroups)
            {
                if (processorTasks.Any())
                {
                    var processingTask = processor.ProcessTaskDefinitionsAsync(processorTasks, null, cancellationToken);
                    distributedTasks.Add(processingTask);
                }
            }

            await Task.WhenAll(distributedTasks);
        }

        /// <summary>
        /// Gets aggregated telemetry from all processors.
        /// </summary>
        /// <returns>Combined telemetry summary.</returns>
        public TelemetrySummary GetAggregatedTelemetry()
        {
            var allTelemetry = _processors.SelectMany(p => p.Telemetry).ToList();

            if (!allTelemetry.Any())
            {
                return new TelemetrySummary();
            }

            return new TelemetrySummary
            {
                TotalTasks = allTelemetry.Count,
                SuccessfulTasks = allTelemetry.Count(t => t.IsSuccessful),
                FailedTasks = allTelemetry.Count(t => !t.IsSuccessful),
                AverageExecutionTime = allTelemetry.Average(t => t.ElapsedMilliseconds),
                TotalExecutionTime = allTelemetry.Sum(t => t.ElapsedMilliseconds),
                MinExecutionTime = allTelemetry.Min(t => t.ElapsedMilliseconds),
                MaxExecutionTime = allTelemetry.Max(t => t.ElapsedMilliseconds)
            };
        }

        /// <summary>
        /// Gets processor utilization statistics.
        /// </summary>
        /// <returns>Processor utilization information.</returns>
        public ProcessorUtilization[] GetProcessorUtilization()
        {
            return _processors.Select((processor, index) =>
            {
                var telemetry = processor.Telemetry.ToList();
                var results = processor.TaskResults.ToList();

                return new ProcessorUtilization
                {
                    ProcessorIndex = index,
                    ProcessorName = processor.TaskListName,
                    TasksProcessed = telemetry.Count,
                    SuccessfulTasks = telemetry.Count(t => t.IsSuccessful),
                    FailedTasks = telemetry.Count(t => !t.IsSuccessful),
                    AverageExecutionTime = telemetry.Any() ? telemetry.Average(t => t.ElapsedMilliseconds) : 0,
                    TotalExecutionTime = telemetry.Sum(t => t.ElapsedMilliseconds)
                };
            }).ToArray();
        }

        private List<(TaskListProcessorEnhanced Processor, List<TaskDefinition> Tasks)> DistributeTasksToProcessors(
            List<TaskDefinition> tasks)
        {
            var distribution = _processors.Select(p => (Processor: p, Tasks: new List<TaskDefinition>())).ToList();

            switch (_strategy)
            {
                case LoadBalancingStrategy.RoundRobin:
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        var processorIndex = i % _processors.Count;
                        distribution[processorIndex].Tasks.Add(tasks[i]);
                    }
                    break;

                case LoadBalancingStrategy.LeastLoaded:
                    foreach (var task in tasks)
                    {
                        var leastLoadedIndex = GetLeastLoadedProcessorIndex();
                        distribution[leastLoadedIndex].Tasks.Add(task);
                    }
                    break;

                case LoadBalancingStrategy.WeightedRoundRobin:
                    // Implement weighted distribution based on processor capabilities
                    DistributeWeightedRoundRobin(tasks, distribution);
                    break;

                case LoadBalancingStrategy.TaskAffinity:
                    // Group similar tasks on the same processor for cache efficiency
                    DistributeByTaskAffinity(tasks, distribution);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return distribution;
        }

        private int GetLeastLoadedProcessorIndex()
        {
            lock (_lock)
            {
                var utilizations = GetProcessorUtilization();
                return utilizations
                    .Select((util, index) => new { Utilization = util, Index = index })
                    .OrderBy(x => x.Utilization.TasksProcessed)
                    .First().Index;
            }
        }

        private void DistributeWeightedRoundRobin(
            List<TaskDefinition> tasks,
            List<(TaskListProcessorEnhanced Processor, List<TaskDefinition> Tasks)> distribution)
        {
            // Simple weight-based distribution (can be enhanced with processor-specific weights)
            var weights = Enumerable.Repeat(1, _processors.Count).ToArray();
            var currentWeights = new int[_processors.Count];

            foreach (var task in tasks)
            {
                var maxWeightIndex = 0;
                for (int i = 1; i < _processors.Count; i++)
                {
                    if (currentWeights[i] < currentWeights[maxWeightIndex])
                    {
                        maxWeightIndex = i;
                    }
                }

                distribution[maxWeightIndex].Tasks.Add(task);
                currentWeights[maxWeightIndex] += weights[maxWeightIndex];
            }
        }

        private void DistributeByTaskAffinity(
            List<TaskDefinition> tasks,
            List<(TaskListProcessorEnhanced Processor, List<TaskDefinition> Tasks)> distribution)
        {
            // Group tasks by some affinity criteria (e.g., task name prefix)
            var taskGroups = tasks
                .GroupBy(t => GetTaskAffinityKey(t))
                .ToList();

            var processorIndex = 0;
            foreach (var group in taskGroups)
            {
                distribution[processorIndex % _processors.Count].Tasks.AddRange(group);
                processorIndex++;
            }
        }

        private string GetTaskAffinityKey(TaskDefinition task)
        {
            // Simple affinity based on task name prefix
            var parts = task.Name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : "default";
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var processor in _processors)
                {
                    processor?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
