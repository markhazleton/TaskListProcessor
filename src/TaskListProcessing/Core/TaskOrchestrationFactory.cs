using Microsoft.Extensions.Logging;
using TaskListProcessing.Context;
using TaskListProcessing.Interfaces;
using TaskListProcessing.LoadBalancing;
using TaskListProcessing.Scheduling;

namespace TaskListProcessing.Core
{

    /// <summary>
    /// Factory for creating dependency resolvers and schedulers.
    /// </summary>
    public static class TaskOrchestrationFactory
    {
        /// <summary>
        /// Creates a topological dependency resolver.
        /// </summary>
        /// <returns>Topological task dependency resolver.</returns>
        public static ITaskDependencyResolver CreateTopologicalResolver()
        {
            return new TopologicalTaskDependencyResolver();
        }

        /// <summary>
        /// Creates an advanced task scheduler.
        /// </summary>
        /// <param name="strategy">Scheduling strategy.</param>
        /// <param name="maxConcurrency">Maximum concurrency.</param>
        /// <returns>Advanced task scheduler.</returns>
        public static AdvancedTaskScheduler CreateAdvancedScheduler(
            TaskSchedulingStrategy strategy = TaskSchedulingStrategy.FirstInFirstOut,
            int maxConcurrency = 0)
        {
            var concurrency = maxConcurrency > 0 ? maxConcurrency : Environment.ProcessorCount * 2;
            return new AdvancedTaskScheduler(strategy, concurrency);
        }

        /// <summary>
        /// Creates a load balancing distributor.
        /// </summary>
        /// <param name="processorCount">Number of processors to create.</param>
        /// <param name="strategy">Load balancing strategy.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>Load balancing task distributor.</returns>
        public static LoadBalancingTaskDistributor CreateLoadBalancer(
            int processorCount = 0,
            LoadBalancingStrategy strategy = LoadBalancingStrategy.RoundRobin,
            ILogger? logger = null)
        {
            var count = processorCount > 0 ? processorCount : Environment.ProcessorCount;
            var processors = new List<TaskListProcessorEnhanced>();

            for (int i = 0; i < count; i++)
            {
                var processor = TaskListProcessorBuilder
                    .Create($"Processor_{i}")
                    .WithLogger(logger)
                    .WithPreset(TaskProcessorPreset.Balanced)
                    .Build();

                processors.Add(processor);
            }

            return new LoadBalancingTaskDistributor(processors, strategy);
        }

        /// <summary>
        /// Creates context propagation handlers.
        /// </summary>
        /// <param name="includeExecutionContext">Whether to include execution context.</param>
        /// <param name="includeSynchronizationContext">Whether to include synchronization context.</param>
        /// <returns>List of context propagation handlers.</returns>
        public static List<IContextPropagationHandler> CreateContextHandlers(
            bool includeExecutionContext = true,
            bool includeSynchronizationContext = false)
        {
            var handlers = new List<IContextPropagationHandler>();

            if (includeExecutionContext)
                handlers.Add(new ExecutionContextPropagationHandler());

            if (includeSynchronizationContext)
                handlers.Add(new SynchronizationContextPropagationHandler());

            return handlers;
        }
    }
}