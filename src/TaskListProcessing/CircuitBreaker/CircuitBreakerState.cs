using System.Collections.Concurrent;

namespace TaskListProcessing;

/// <summary>
/// Represents the state of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuit is closed and requests are flowing normally.
    /// </summary>
    Closed,
    
    /// <summary>
    /// Circuit is open and all requests are being rejected.
    /// </summary>
    Open,
    
    /// <summary>
    /// Circuit is half-open and testing if the service has recovered.
    /// </summary>
    HalfOpen
}
