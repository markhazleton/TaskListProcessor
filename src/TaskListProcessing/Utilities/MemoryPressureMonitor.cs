using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace TaskListProcessing.Utilities;

/// <summary>
/// Monitors memory pressure and adjusts pool behavior accordingly.
/// This helps prevent memory issues by tracking system memory usage.
/// </summary>
public class MemoryPressureMonitor : IDisposable
{
    private readonly Timer _monitorTimer;
    private readonly ILogger? _logger;
    private volatile MemoryPressureLevel _currentLevel = MemoryPressureLevel.Normal;
    private bool _disposed;

    /// <summary>
    /// Gets the current memory pressure level.
    /// </summary>
    public MemoryPressureLevel CurrentLevel => _currentLevel;

    /// <summary>
    /// Event raised when memory pressure level changes.
    /// </summary>
    public event EventHandler<MemoryPressureChangedEventArgs>? PressureChanged;

    /// <summary>
    /// Initializes a new instance of the MemoryPressureMonitor.
    /// </summary>
    /// <param name="logger">Optional logger for monitoring events.</param>
    public MemoryPressureMonitor(ILogger? logger = null)
    {
        _logger = logger;
        _monitorTimer = new Timer(CheckMemoryPressure, null, 
            TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Checks current memory pressure and updates the level if needed.
    /// </summary>
    /// <param name="state">Timer state (unused).</param>
    private void CheckMemoryPressure(object? state)
    {
        if (_disposed) return;

        try
        {
            var totalMemory = GC.GetTotalMemory(false);
            var workingSet = Environment.WorkingSet;
            var memoryPressure = totalMemory / (double)workingSet;

            var newLevel = memoryPressure switch
            {
                > 0.85 => MemoryPressureLevel.Critical,
                > 0.70 => MemoryPressureLevel.High,
                > 0.50 => MemoryPressureLevel.Medium,
                _ => MemoryPressureLevel.Normal
            };

            if (newLevel != _currentLevel)
            {
                var previousLevel = _currentLevel;
                _currentLevel = newLevel;
                
                _logger?.LogInformation("Memory pressure changed from {Previous} to {Current} (Memory: {TotalMemory:N0} bytes, Working Set: {WorkingSet:N0} bytes)", 
                    previousLevel, newLevel, totalMemory, workingSet);
                    
                PressureChanged?.Invoke(this, new MemoryPressureChangedEventArgs(previousLevel, newLevel));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error occurred while checking memory pressure");
        }
    }

    /// <summary>
    /// Disposes the memory pressure monitor and stops monitoring.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _monitorTimer?.Dispose();
            _logger?.LogDebug("Memory pressure monitor disposed");
        }
    }
}

/// <summary>
/// Represents different levels of memory pressure.
/// </summary>
public enum MemoryPressureLevel
{
    /// <summary>
    /// Normal memory usage - no action required.
    /// </summary>
    Normal,
    
    /// <summary>
    /// Medium memory usage - consider reducing pool sizes.
    /// </summary>
    Medium,
    
    /// <summary>
    /// High memory usage - actively reduce pool sizes.
    /// </summary>
    High,
    
    /// <summary>
    /// Critical memory usage - emergency cleanup required.
    /// </summary>
    Critical
}

/// <summary>
/// Event arguments for memory pressure changes.
/// </summary>
public class MemoryPressureChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous memory pressure level.
    /// </summary>
    public MemoryPressureLevel PreviousLevel { get; }

    /// <summary>
    /// Gets the current memory pressure level.
    /// </summary>
    public MemoryPressureLevel CurrentLevel { get; }

    /// <summary>
    /// Initializes a new instance of the MemoryPressureChangedEventArgs.
    /// </summary>
    /// <param name="previous">The previous memory pressure level.</param>
    /// <param name="current">The current memory pressure level.</param>
    public MemoryPressureChangedEventArgs(MemoryPressureLevel previous, MemoryPressureLevel current)
    {
        PreviousLevel = previous;
        CurrentLevel = current;
    }
}
