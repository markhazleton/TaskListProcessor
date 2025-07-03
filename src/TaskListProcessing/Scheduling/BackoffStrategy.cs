using System.Net.Sockets;
using TaskListProcessing.Models;

namespace TaskListProcessing.Scheduling;

/// <summary>
/// Backoff strategies for retry policies.
/// </summary>
public enum BackoffStrategy
{
    /// <summary>
    /// Fixed delay between retries.
    /// </summary>
    Fixed,

    /// <summary>
    /// Exponential backoff with optional jitter.
    /// </summary>
    Exponential,

    /// <summary>
    /// Linear increase in delay.
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential backoff with full jitter.
    /// </summary>
    ExponentialWithJitter
}

/// <summary>
/// Advanced retry policy configuration for failed tasks.
/// </summary>
public class RetryPolicy
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retries.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the backoff strategy.
    /// </summary>
    public BackoffStrategy BackoffStrategy { get; set; } = BackoffStrategy.ExponentialWithJitter;

    /// <summary>
    /// Gets or sets the maximum delay between retries.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the jitter factor (0.0 to 1.0) for adding randomness to delays.
    /// </summary>
    public double JitterFactor { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the function to determine if an exception should be retried.
    /// </summary>
    public Func<Exception, int, bool> ShouldRetry { get; set; } = DefaultShouldRetry;

    /// <summary>
    /// Gets or sets additional retry conditions based on task error category.
    /// </summary>
    public Func<TaskErrorCategory, int, bool> ShouldRetryCategory { get; set; } = DefaultShouldRetryCategory;

    /// <summary>
    /// Calculates the delay for a specific retry attempt.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (1-based).</param>
    /// <returns>The delay to wait before the next retry.</returns>
    public TimeSpan CalculateDelay(int attemptNumber)
    {
        if (attemptNumber <= 1)
            return TimeSpan.Zero;

        var delay = BackoffStrategy switch
        {
            BackoffStrategy.Fixed => BaseDelay,
            BackoffStrategy.Linear => TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * attemptNumber),
            BackoffStrategy.Exponential => TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attemptNumber - 1)),
            BackoffStrategy.ExponentialWithJitter => CalculateExponentialWithJitter(attemptNumber),
            _ => BaseDelay
        };

        return delay > MaxDelay ? MaxDelay : delay;
    }

    /// <summary>
    /// Default retry logic based on exception type.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="attemptNumber">The current attempt number.</param>
    /// <returns>True if the operation should be retried; otherwise, false.</returns>
    public static bool DefaultShouldRetry(Exception exception, int attemptNumber)
    {
        var category = EnhancedTaskResult<object>.ClassifyError(exception);
        return DefaultShouldRetryCategory(category, attemptNumber);
    }

    /// <summary>
    /// Default retry logic based on error category.
    /// </summary>
    /// <param name="category">The error category.</param>
    /// <param name="attemptNumber">The current attempt number.</param>
    /// <returns>True if the operation should be retried; otherwise, false.</returns>
    public static bool DefaultShouldRetryCategory(TaskErrorCategory category, int attemptNumber)
    {
        return category switch
        {
            TaskErrorCategory.NetworkError => true,
            TaskErrorCategory.Timeout => attemptNumber <= 2, // Only retry timeouts twice
            TaskErrorCategory.SystemError => true,
            TaskErrorCategory.Unknown => true,
            TaskErrorCategory.AuthenticationError => false,
            TaskErrorCategory.ValidationError => false,
            TaskErrorCategory.BusinessError => false,
            _ => false
        };
    }

    /// <summary>
    /// Validates the retry policy configuration.
    /// </summary>
    /// <returns>Validation result.</returns>
    public (bool IsValid, string[] Errors) Validate()
    {
        var errors = new List<string>();

        if (MaxRetryAttempts < 0)
            errors.Add("MaxRetryAttempts cannot be negative");

        if (BaseDelay < TimeSpan.Zero)
            errors.Add("BaseDelay cannot be negative");

        if (MaxDelay < BaseDelay)
            errors.Add("MaxDelay cannot be less than BaseDelay");

        if (BackoffMultiplier <= 0)
            errors.Add("BackoffMultiplier must be greater than 0");

        if (JitterFactor < 0 || JitterFactor > 1)
            errors.Add("JitterFactor must be between 0 and 1");

        return (errors.Count == 0, errors.ToArray());
    }

    private TimeSpan CalculateExponentialWithJitter(int attemptNumber)
    {
        var exponentialDelay = BaseDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attemptNumber - 1);

        // Add jitter: random value between 0 and JitterFactor * exponentialDelay
        var jitter = Random.Shared.NextDouble() * JitterFactor * exponentialDelay;

        return TimeSpan.FromMilliseconds(exponentialDelay + jitter);
    }
}

/// <summary>
/// Handles retry logic execution for tasks.
/// </summary>
public class RetryHandler
{
    private readonly RetryPolicy _policy;

    /// <summary>
    /// Initializes a new instance of the RetryHandler class.
    /// </summary>
    /// <param name="policy">The retry policy to use.</param>
    public RetryHandler(RetryPolicy policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    /// <summary>
    /// Executes a task with retry logic.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="taskFactory">Factory function to create the task.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result with retry information.</returns>
    public async Task<EnhancedTaskResult<T>> ExecuteWithRetryAsync<T>(
        string taskName,
        Func<CancellationToken, Task<T>> taskFactory,
        CancellationToken cancellationToken = default)
    {
        var result = new EnhancedTaskResult<T>(taskName, default, false);
        Exception? lastException = null;

        for (int attempt = 1; attempt <= _policy.MaxRetryAttempts + 1; attempt++)
        {
            result.AttemptNumber = attempt;
            result.StartTime = DateTimeOffset.UtcNow;

            try
            {
                var data = await taskFactory(cancellationToken);
                result.Data = data;
                result.IsSuccessful = true;
                result.ExecutionTime = DateTimeOffset.UtcNow - result.StartTime;
                return result;
            }
            catch (Exception ex) when (attempt <= _policy.MaxRetryAttempts)
            {
                lastException = ex;
                result.ExecutionTime = DateTimeOffset.UtcNow - result.StartTime;

                var errorCategory = EnhancedTaskResult<T>.ClassifyError(ex);

                // Check if we should retry based on exception or category
                var shouldRetryException = _policy.ShouldRetry(ex, attempt);
                var shouldRetryCategory = _policy.ShouldRetryCategory(errorCategory, attempt);

                if (!shouldRetryException && !shouldRetryCategory)
                {
                    break; // Don't retry this type of error
                }

                // Calculate delay for next attempt
                if (attempt <= _policy.MaxRetryAttempts)
                {
                    var delay = _policy.CalculateDelay(attempt + 1);
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                result.ExecutionTime = DateTimeOffset.UtcNow - result.StartTime;
                break; // Final attempt failed
            }
        }

        // All retries exhausted or non-retryable error
        if (lastException != null)
        {
            result.SetError(lastException);
        }

        return result;
    }

    /// <summary>
    /// Executes an action with retry logic.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result with retry information.</returns>
    public async Task<EnhancedTaskResult<object>> ExecuteWithRetryAsync(
        string taskName,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<object>(
            taskName,
            async ct =>
            {
                await action(ct);
                return new object(); // Dummy return value for void actions
            },
            cancellationToken);
    }
}

/// <summary>
/// Provides pre-configured retry policies for common scenarios.
/// </summary>
public static class RetryPolicies
{
    /// <summary>
    /// No retry policy - fail immediately on first error.
    /// </summary>
    public static RetryPolicy None => new()
    {
        MaxRetryAttempts = 0
    };

    /// <summary>
    /// Simple retry policy with fixed delays.
    /// </summary>
    public static RetryPolicy Simple => new()
    {
        MaxRetryAttempts = 3,
        BaseDelay = TimeSpan.FromSeconds(1),
        BackoffStrategy = BackoffStrategy.Fixed
    };

    /// <summary>
    /// Aggressive retry policy with exponential backoff for network operations.
    /// </summary>
    public static RetryPolicy Network => new()
    {
        MaxRetryAttempts = 5,
        BaseDelay = TimeSpan.FromMilliseconds(200),
        BackoffStrategy = BackoffStrategy.ExponentialWithJitter,
        MaxDelay = TimeSpan.FromSeconds(10),
        JitterFactor = 0.2
    };

    /// <summary>
    /// Conservative retry policy for critical operations.
    /// </summary>
    public static RetryPolicy Critical => new()
    {
        MaxRetryAttempts = 2,
        BaseDelay = TimeSpan.FromSeconds(2),
        BackoffStrategy = BackoffStrategy.Linear,
        MaxDelay = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    /// Database retry policy optimized for transient database errors.
    /// </summary>
    public static RetryPolicy Database => new()
    {
        MaxRetryAttempts = 4,
        BaseDelay = TimeSpan.FromMilliseconds(100),
        BackoffStrategy = BackoffStrategy.ExponentialWithJitter,
        MaxDelay = TimeSpan.FromSeconds(8),
        JitterFactor = 0.15,
        ShouldRetry = (ex, attempt) =>
        {
            // Retry on typical database transient errors
            return ex is TimeoutException
                || (ex is InvalidOperationException && ex.Message.Contains("timeout"))
                || ex is SocketException;
        }
    };
}