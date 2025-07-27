using TaskListProcessing.Telemetry;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

namespace TaskListProcessor.Web.Models;

public class TaskProcessingResultViewModel
{
    public string ProcessorName { get; set; } = string.Empty;
    public List<CityResultViewModel> CityResults { get; set; } = new();
    public TelemetrySummaryViewModel TelemetrySummary { get; set; } = new();
    public List<TaskTelemetryViewModel> DetailedTelemetry { get; set; } = new();
    public bool IsCompleted { get; set; }
    public bool HasErrors { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long TotalDurationMs { get; set; }
}

public class CityResultViewModel
{
    public string CityName { get; set; } = string.Empty;
    public WeatherResultViewModel? Weather { get; set; }
    public ActivitiesResultViewModel? Activities { get; set; }
    public bool HasWeatherData => Weather?.IsSuccessful ?? false;
    public bool HasActivitiesData => Activities?.IsSuccessful ?? false;
    public bool IsFullySuccessful => HasWeatherData && HasActivitiesData;
}

public class WeatherResultViewModel
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public List<WeatherForecast> Forecasts { get; set; } = new();
    public long DurationMs { get; set; }
}

public class ActivitiesResultViewModel
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Activity> Activities { get; set; } = new();
    public long DurationMs { get; set; }
}

public class TelemetrySummaryViewModel
{
    public int TotalTasks { get; set; }
    public int SuccessfulTasks { get; set; }
    public int FailedTasks { get; set; }
    public double SuccessRate { get; set; }
    public double AverageExecutionTime { get; set; }
    public long MinExecutionTime { get; set; }
    public long MaxExecutionTime { get; set; }
    public long TotalExecutionTime { get; set; }
    public double ThroughputPerSecond { get; set; }
}

public class TaskTelemetryViewModel
{
    public string TaskName { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty;
    public string PerformanceIcon { get; set; } = string.Empty;
    public string PerformanceColor { get; set; } = string.Empty;
}

public class ProcessingConfigurationViewModel
{
    public List<string> SelectedCities { get; set; } = new();
    public List<string> AvailableCities { get; set; } = new()
    {
        "London", "Paris", "New York", "Tokyo", "Sydney", "Chicago",
        "Dallas", "Wichita", "Rome", "Houston", "Boston", "Miami",
        "Denver", "Phoenix", "Portland", "Atlanta", "Detroit", "Las Vegas"
    };
    public int MaxConcurrentTasks { get; set; } = 10;
    public int TimeoutMinutes { get; set; } = 5;
    public bool EnableDetailedTelemetry { get; set; } = true;
    public bool ShowIndividualResults { get; set; } = true;
    public ProcessingScenario Scenario { get; set; } = ProcessingScenario.MainProcessing;
}

public enum ProcessingScenario
{
    MainProcessing,
    IndividualTask,
    CancellationDemo,
    ConcurrentProcessingDemo,
    StreamingDemo
}
