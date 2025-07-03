using System.Collections.Concurrent;

namespace TaskListProcessing
{

    /// <summary>
    /// Circuit breaker pattern implementation for handling cascading failures.
    /// </summary>
    public class CircuitBreaker
    {
        private readonly CircuitBreakerOptions _options;
        private readonly object _lock = new();
        private readonly ConcurrentQueue<DateTimeOffset> _failureTimestamps = new();

        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private DateTimeOffset _openedAt;
        private int _halfOpenAttempts;

        /// <summary>
        /// Initializes a new instance of the CircuitBreaker class.
        /// </summary>
        /// <param name="options">Circuit breaker configuration options.</param>
        public CircuitBreaker(CircuitBreakerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the current state of the circuit breaker.
        /// </summary>
        public CircuitBreakerState State
        {
            get
            {
                lock (_lock)
                {
                    return _state;
                }
            }
        }

        /// <summary>
        /// Gets the current failure count within the time window.
        /// </summary>
        public int FailureCount
        {
            get
            {
                CleanupOldFailures();
                return _failureTimestamps.Count;
            }
        }

        /// <summary>
        /// Determines whether the circuit breaker should reject the request.
        /// </summary>
        /// <returns>True if the request should be rejected; otherwise, false.</returns>
        public bool ShouldReject()
        {
            lock (_lock)
            {
                switch (_state)
                {
                    case CircuitBreakerState.Open:
                        if (DateTimeOffset.UtcNow - _openedAt >= _options.OpenDuration)
                        {
                            _state = CircuitBreakerState.HalfOpen;
                            _halfOpenAttempts = 0;
                            return false;
                        }
                        return true;

                    case CircuitBreakerState.HalfOpen:
                        return _halfOpenAttempts >= _options.HalfOpenAttempts;

                    case CircuitBreakerState.Closed:
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Records a successful operation.
        /// </summary>
        public void RecordSuccess()
        {
            lock (_lock)
            {
                switch (_state)
                {
                    case CircuitBreakerState.HalfOpen:
                        _halfOpenAttempts++;
                        if (_halfOpenAttempts >= _options.SuccessThreshold)
                        {
                            _state = CircuitBreakerState.Closed;
                            _failureTimestamps.Clear();
                        }
                        break;

                    case CircuitBreakerState.Closed:
                        // In closed state, we don't need to track individual successes
                        break;
                }
            }
        }

        /// <summary>
        /// Records a failed operation.
        /// </summary>
        public void RecordFailure()
        {
            lock (_lock)
            {
                _failureTimestamps.Enqueue(DateTimeOffset.UtcNow);
                CleanupOldFailures();

                switch (_state)
                {
                    case CircuitBreakerState.Closed:
                        if (_failureTimestamps.Count >= _options.FailureThreshold)
                        {
                            _state = CircuitBreakerState.Open;
                            _openedAt = DateTimeOffset.UtcNow;
                        }
                        break;

                    case CircuitBreakerState.HalfOpen:
                        _state = CircuitBreakerState.Open;
                        _openedAt = DateTimeOffset.UtcNow;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets circuit breaker statistics.
        /// </summary>
        /// <returns>Circuit breaker statistics.</returns>
        public CircuitBreakerStats GetStats()
        {
            lock (_lock)
            {
                CleanupOldFailures();

                return new CircuitBreakerStats
                {
                    State = _state,
                    FailureCount = _failureTimestamps.Count,
                    FailureThreshold = _options.FailureThreshold,
                    TimeWindow = _options.TimeWindow,
                    OpenDuration = _options.OpenDuration,
                    OpenedAt = _state == CircuitBreakerState.Open ? _openedAt : null,
                    TimeUntilRetry = _state == CircuitBreakerState.Open
                        ? _options.OpenDuration - (DateTimeOffset.UtcNow - _openedAt)
                        : null
                };
            }
        }

        /// <summary>
        /// Resets the circuit breaker to closed state.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _state = CircuitBreakerState.Closed;
                _failureTimestamps.Clear();
                _halfOpenAttempts = 0;
            }
        }

        private void CleanupOldFailures()
        {
            var cutoff = DateTimeOffset.UtcNow - _options.TimeWindow;

            while (_failureTimestamps.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _failureTimestamps.TryDequeue(out _);
            }
        }
    }
}
