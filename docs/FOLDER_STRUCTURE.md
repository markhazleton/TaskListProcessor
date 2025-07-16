# TaskListProcessing - Folder Structure

This document describes the organized folder structure for the TaskListProcessing library.

## Folder Organization

### 📁 Core

Contains the main task processor classes and core functionality:

- `TaskListProcessorBuilder.cs` - Builder pattern for processor configuration
- `TaskListProcessorEnhanced.cs` - Enhanced processor with advanced features
- `TaskOrchestrationFactory.cs` - Factory for creating task orchestration instances

### 📁 Interfaces

Contains all interface definitions:

- `ITaskResult.cs` - Contract for task result objects
- `ITelemetryExporter.cs` - Interface for telemetry exporters
- `IContextPropagationHandler.cs` - Interface for context propagation handlers
- `ITaskDependencyResolver.cs` - Interface for task dependency resolution

### 📁 Models

Contains data models, DTOs, and result classes:

- `TaskResult<T>.cs` - Generic task result implementation
- `TaskDefinition.cs` - Task definition model
- `TaskProgress.cs` - Task progress tracking model
- `TaskErrorCategory.cs` - Error categorization enumeration
- `ScheduledTask.cs` - Scheduled task model
- `TaskExecutionSlot.cs` - Task execution slot model

### 📁 Scheduling

Contains task scheduling and orchestration functionality:

- `AdvancedTaskScheduler.cs` - Advanced task scheduling implementation
- `TaskSchedulingStrategy.cs` - Task scheduling strategy enumeration
- `TopologicalTaskDependencyResolver.cs` - Dependency resolution using topological sorting
- `BackoffStrategy.cs` - Backoff strategy enumeration for retry logic

### 📁 CircuitBreaker

Contains circuit breaker pattern implementation:

- `CircuitBreaker.cs` - Main circuit breaker implementation
- `CircuitBreakerOptions.cs` - Configuration options for circuit breaker
- `CircuitBreakerState.cs` - Circuit breaker state enumeration
- `CircuitBreakerStats.cs` - Circuit breaker statistics tracking

### 📁 LoadBalancing

Contains load balancing functionality:

- `LoadBalancingTaskDistributor.cs` - Task distributor with load balancing
- `LoadBalancingStrategy.cs` - Load balancing strategy enumeration

### 📁 Telemetry

Contains telemetry and monitoring functionality:

- `TaskTelemetry.cs` - Task telemetry data model
- `TelemetrySummary.cs` - Telemetry summary aggregation
- `ConsoleTelemetryExporter.cs` - Console-based telemetry exporter
- `SchedulerStats.cs` - Scheduler statistics tracking
- `ProcessorUtilization.cs` - Processor utilization monitoring

### 📁 Options

Contains configuration and options classes:

- `enhanced_options.cs` - Enhanced processor configuration options
- `HealthCheckOptions.cs` - Health check configuration options
- `TaskExecutionContextOptions.cs` - Task execution context configuration
- `MemoryPoolOptions.cs` - Memory pool configuration options

### 📁 Context

Contains context propagation and restoration functionality:

- `ContextRestorer.cs` - Context restoration implementation
- `ExecutionContextPropagationHandler.cs` - Execution context propagation handler
- `SynchronizationContextPropagationHandler.cs` - Synchronization context propagation handler

### 📁 Utilities

Contains helper classes and utilities:

- `UsageExample.cs` - Usage examples and demonstration code

### 📁 Testing

Contains testing utilities and helpers:

- `TaskListProcessorTestHelpers.cs` - Helper classes for testing

## Benefits of This Organization

1. **Logical Grouping**: Related functionality is grouped together, making it easier to find and maintain code.
2. **Separation of Concerns**: Each folder has a clear, single responsibility.
3. **Easier Navigation**: Developers can quickly locate specific functionality without scrolling through a long list of files.
4. **Better Maintainability**: Changes to specific features are contained within their respective folders.
5. **Improved Readability**: The project structure tells a story about the architecture and capabilities.

## Usage

The folder structure is transparent to consumers of the library. All public APIs remain accessible through their original namespaces. The SDK-style project format automatically includes all `.cs` files in subdirectories, so no changes to the project file are required.
