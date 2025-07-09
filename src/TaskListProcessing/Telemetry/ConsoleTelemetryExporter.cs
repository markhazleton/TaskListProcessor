using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using TaskListProcessing.Interfaces;

namespace TaskListProcessing.Telemetry;

/// <summary>
/// Console telemetry exporter for development and debugging.
/// </summary>
public class ConsoleTelemetryExporter : ITelemetryExporter
{
    private readonly ILogger? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the ConsoleTelemetryExporter class.
    /// </summary>
    /// <param name="logger">Optional logger for structured logging.</param>
    public ConsoleTelemetryExporter(ILogger? logger = null)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Gets the name of the exporter.
    /// </summary>
    public string Name => "Console";

    /// <summary>
    /// Gets whether the exporter is enabled.
    /// </summary>
    public bool IsEnabled => true;

    /// <summary>
    /// Exports telemetry data to the console.
    /// </summary>
    /// <param name="telemetry">The telemetry data to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the export operation.</returns>
    public Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default)
    {
        var telemetryList = telemetry.ToList();

        Console.WriteLine($"\n=== TELEMETRY EXPORT ({DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC) ===");
        Console.WriteLine($"Total Records: {telemetryList.Count}");

        foreach (var record in telemetryList)
        {
            var json = JsonSerializer.Serialize(record, _jsonOptions);
            Console.WriteLine(json);
        }

        Console.WriteLine("=== END TELEMETRY EXPORT ===\n");

        _logger?.LogInformation("Exported {RecordCount} telemetry records to console", telemetryList.Count);

        return Task.CompletedTask;
    }
}

/// <summary>
/// File-based telemetry exporter for persistent storage.
/// </summary>
public class FileTelemetryExporter : ITelemetryExporter, IDisposable
{
    private readonly string _filePath;
    private readonly ILogger? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the FileTelemetryExporter class.
    /// </summary>
    /// <param name="filePath">Path to the telemetry file.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    public FileTelemetryExporter(string filePath, ILogger? logger = null)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Ensure directory exists
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Gets the name of the exporter.
    /// </summary>
    public string Name => "File";

    /// <summary>
    /// Gets whether the exporter is enabled.
    /// </summary>
    public bool IsEnabled => true;

    /// <summary>
    /// Exports telemetry data to a file.
    /// </summary>
    /// <param name="telemetry">The telemetry data to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the export operation.</returns>
    public async Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            var telemetryList = telemetry.ToList();
            var exportData = new
            {
                ExportTimestamp = DateTime.UtcNow,
                RecordCount = telemetryList.Count,
                Records = telemetryList
            };

            var json = JsonSerializer.Serialize(exportData, _jsonOptions);
            await File.AppendAllTextAsync(_filePath, json + Environment.NewLine, cancellationToken);

            _logger?.LogInformation("Exported {RecordCount} telemetry records to file '{FilePath}'",
                telemetryList.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to export telemetry to file '{FilePath}'", _filePath);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Disposes the exporter and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _fileLock?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// OpenTelemetry exporter for integration with observability platforms.
/// </summary>
public class OpenTelemetryExporter : ITelemetryExporter
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger? _logger;
    private readonly string _serviceName;

    /// <summary>
    /// Initializes a new instance of the OpenTelemetryExporter class.
    /// </summary>
    /// <param name="serviceName">Name of the service for tracing.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    public OpenTelemetryExporter(string serviceName = "TaskListProcessor", ILogger? logger = null)
    {
        _serviceName = serviceName;
        _activitySource = new ActivitySource(_serviceName);
        _logger = logger;
    }

    /// <summary>
    /// Gets the name of the exporter.
    /// </summary>
    public string Name => "OpenTelemetry";

    /// <summary>
    /// Gets whether the exporter is enabled.
    /// </summary>
    public bool IsEnabled => true;

    /// <summary>
    /// Exports telemetry data to OpenTelemetry.
    /// </summary>
    /// <param name="telemetry">The telemetry data to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the export operation.</returns>
    public Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("TaskListProcessor.ExportTelemetry");

        var telemetryList = telemetry.ToList();

        activity?.SetTag("service.name", _serviceName);
        activity?.SetTag("telemetry.record_count", telemetryList.Count);
        activity?.SetTag("telemetry.successful_tasks", telemetryList.Count(t => t.IsSuccessful));
        activity?.SetTag("telemetry.failed_tasks", telemetryList.Count(t => !t.IsSuccessful));

        // Create individual spans for each task
        foreach (var record in telemetryList)
        {
            using var taskActivity = _activitySource.StartActivity($"Task.{record.TaskName}");

            taskActivity?.SetTag("task.name", record.TaskName);
            taskActivity?.SetTag("task.duration_ms", record.ElapsedMilliseconds);
            taskActivity?.SetTag("task.successful", record.IsSuccessful);

            if (!record.IsSuccessful)
            {
                taskActivity?.SetTag("task.error_type", record.ErrorType);
                taskActivity?.SetTag("task.error_message", record.ErrorMessage);
                taskActivity?.SetStatus(ActivityStatusCode.Error, record.ErrorMessage);
            }
        }

        _logger?.LogInformation("Exported {RecordCount} telemetry records to OpenTelemetry", telemetryList.Count);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Composite telemetry exporter that sends data to multiple exporters.
/// </summary>
public class CompositeTelemetryExporter : ITelemetryExporter, IDisposable
{
    private readonly List<ITelemetryExporter> _exporters;
    private readonly ILogger? _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the CompositeTelemetryExporter class.
    /// </summary>
    /// <param name="exporters">The collection of exporters to use.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    public CompositeTelemetryExporter(IEnumerable<ITelemetryExporter> exporters, ILogger? logger = null)
    {
        _exporters = exporters?.ToList() ?? throw new ArgumentNullException(nameof(exporters));
        _logger = logger;
    }

    /// <summary>
    /// Gets the name of the exporter.
    /// </summary>
    public string Name => $"Composite({string.Join(", ", _exporters.Select(e => e.Name))})";

    /// <summary>
    /// Gets whether the exporter is enabled.
    /// </summary>
    public bool IsEnabled => _exporters.Any(e => e.IsEnabled);

    /// <summary>
    /// Exports telemetry data to all configured exporters.
    /// </summary>
    /// <param name="telemetry">The telemetry data to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the export operation.</returns>
    public async Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default)
    {
        var enabledExporters = _exporters.Where(e => e.IsEnabled).ToList();
        var exportTasks = enabledExporters.Select(async exporter =>
        {
            try
            {
                await exporter.ExportAsync(telemetry, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to export telemetry using exporter '{ExporterName}'", exporter.Name);
            }
        });

        await Task.WhenAll(exportTasks);

        _logger?.LogInformation("Exported telemetry to {ExporterCount} exporters", enabledExporters.Count);
    }

    /// <summary>
    /// Disposes the exporter and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var exporter in _exporters.OfType<IDisposable>())
            {
                exporter.Dispose();
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// HTTP-based telemetry exporter for sending data to remote endpoints.
/// </summary>
public class HttpTelemetryExporter : ITelemetryExporter, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly ILogger? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Dictionary<string, string> _headers;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the HttpTelemetryExporter class.
    /// </summary>
    /// <param name="endpoint">The HTTP endpoint to send telemetry to.</param>
    /// <param name="httpClient">Optional HTTP client to use.</param>
    /// <param name="headers">Optional HTTP headers to include.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    public HttpTelemetryExporter(
        string endpoint,
        HttpClient? httpClient = null,
        Dictionary<string, string>? headers = null,
        ILogger? logger = null)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _httpClient = httpClient ?? new HttpClient();
        _headers = headers ?? new Dictionary<string, string>();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Gets the name of the exporter.
    /// </summary>
    public string Name => "HTTP";

    /// <summary>
    /// Gets whether the exporter is enabled.
    /// </summary>
    public bool IsEnabled => true;

    /// <summary>
    /// Exports telemetry data via HTTP.
    /// </summary>
    /// <param name="telemetry">The telemetry data to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the export operation.</returns>
    public async Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default)
    {
        var telemetryList = telemetry.ToList();
        var exportData = new
        {
            Timestamp = DateTime.UtcNow,
            RecordCount = telemetryList.Count,
            Records = telemetryList
        };

        var json = JsonSerializer.Serialize(exportData, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Add custom headers
        foreach (var header in _headers)
        {
            content.Headers.Add(header.Key, header.Value);
        }

        try
        {
            var response = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger?.LogInformation("Exported {RecordCount} telemetry records to HTTP endpoint '{Endpoint}'",
                telemetryList.Count, _endpoint);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to export telemetry to HTTP endpoint '{Endpoint}'", _endpoint);
            throw;
        }
    }

    /// <summary>
    /// Disposes the exporter and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// In-memory telemetry exporter for testing and development.
/// </summary>
public class MemoryTelemetryExporter : ITelemetryExporter
{
    private readonly List<TelemetryExportBatch> _exportHistory = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets the name of the exporter.
    /// </summary>
    public string Name => "Memory";

    /// <summary>
    /// Gets whether the exporter is enabled.
    /// </summary>
    public bool IsEnabled => true;

    /// <summary>
    /// Gets the export history.
    /// </summary>
    public IReadOnlyList<TelemetryExportBatch> ExportHistory
    {
        get
        {
            lock (_lock)
            {
                return _exportHistory.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Exports telemetry data to memory.
    /// </summary>
    /// <param name="telemetry">The telemetry data to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the export operation.</returns>
    public Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default)
    {
        var batch = new TelemetryExportBatch
        {
            ExportTimestamp = DateTime.UtcNow,
            Records = telemetry.ToList()
        };

        lock (_lock)
        {
            _exportHistory.Add(batch);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears the export history.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _exportHistory.Clear();
        }
    }

    /// <summary>
    /// Gets the total number of exported records.
    /// </summary>
    /// <returns>Total record count.</returns>
    public int GetTotalRecordCount()
    {
        lock (_lock)
        {
            return _exportHistory.Sum(batch => batch.Records.Count);
        }
    }
}

/// <summary>
/// Represents a batch of exported telemetry data.
/// </summary>
public class TelemetryExportBatch
{
    /// <summary>
    /// Gets or sets the export timestamp.
    /// </summary>
    public DateTime ExportTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the telemetry records in this batch.
    /// </summary>
    public List<TaskTelemetry> Records { get; set; } = new();
}

/// <summary>
/// Factory for creating telemetry exporters.
/// </summary>
public static class TelemetryExporterFactory
{
    /// <summary>
    /// Creates a console exporter.
    /// </summary>
    /// <param name="logger">Optional logger.</param>
    /// <returns>Console telemetry exporter.</returns>
    public static ITelemetryExporter CreateConsole(ILogger? logger = null)
    {
        return new ConsoleTelemetryExporter(logger);
    }

    /// <summary>
    /// Creates a file exporter.
    /// </summary>
    /// <param name="filePath">Path to the telemetry file.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>File telemetry exporter.</returns>
    public static ITelemetryExporter CreateFile(string filePath, ILogger? logger = null)
    {
        return new FileTelemetryExporter(filePath, logger);
    }

    /// <summary>
    /// Creates an HTTP exporter.
    /// </summary>
    /// <param name="endpoint">HTTP endpoint URL.</param>
    /// <param name="headers">Optional HTTP headers.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>HTTP telemetry exporter.</returns>
    public static ITelemetryExporter CreateHttp(
        string endpoint,
        Dictionary<string, string>? headers = null,
        ILogger? logger = null)
    {
        return new HttpTelemetryExporter(endpoint, null, headers, logger);
    }

    /// <summary>
    /// Creates an OpenTelemetry exporter.
    /// </summary>
    /// <param name="serviceName">Service name for tracing.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>OpenTelemetry exporter.</returns>
    public static ITelemetryExporter CreateOpenTelemetry(string serviceName = "TaskListProcessor", ILogger? logger = null)
    {
        return new OpenTelemetryExporter(serviceName, logger);
    }

    /// <summary>
    /// Creates a memory exporter for testing.
    /// </summary>
    /// <returns>Memory telemetry exporter.</returns>
    public static MemoryTelemetryExporter CreateMemory()
    {
        return new MemoryTelemetryExporter();
    }

    /// <summary>
    /// Creates a composite exporter with multiple targets.
    /// </summary>
    /// <param name="exporters">Collection of exporters to combine.</param>
    /// <param name="logger">Optional logger.</param>
    /// <returns>Composite telemetry exporter.</returns>
    public static ITelemetryExporter CreateComposite(IEnumerable<ITelemetryExporter> exporters, ILogger? logger = null)
    {
        return new CompositeTelemetryExporter(exporters, logger);
    }
}