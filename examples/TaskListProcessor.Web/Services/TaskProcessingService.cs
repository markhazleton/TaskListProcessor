using CityThingsToDo;
using CityWeatherService;
using TaskListProcessing.Core;
using TaskListProcessing.Telemetry;
using TaskListProcessor.Web.Models;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

namespace TaskListProcessor.Web.Services;

public class TaskProcessingService
{
    private readonly WeatherService _weatherService;
    private readonly CityThingsToDoService _thingsToDoService;
    private readonly ILogger<TaskProcessingService> _logger;

    public TaskProcessingService(
        WeatherService weatherService,
        CityThingsToDoService thingsToDoService,
        ILogger<TaskProcessingService> logger)
    {
        _weatherService = weatherService;
        _thingsToDoService = thingsToDoService;
        _logger = logger;
    }

    public async Task<TaskProcessingResultViewModel> ProcessCityDataAsync(
        ProcessingConfigurationViewModel config,
        CancellationToken cancellationToken = default)
    {
        var result = new TaskProcessingResultViewModel
        {
            ProcessorName = "City Data Collection",
            StartTime = DateTime.UtcNow
        };

        try
        {
            using var processor = new TaskListProcessorEnhanced("City Data Collection", _logger);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(config.TimeoutMinutes));

            var taskFactories = CreateCityTaskFactories(config.SelectedCities);

            await processor.ProcessTasksAsync(taskFactories, cancellationToken: cts.Token);

            result.EndTime = DateTime.UtcNow;
            result.TotalDurationMs = (long)(result.EndTime.Value - result.StartTime).TotalMilliseconds;
            result.IsCompleted = true;

            // Convert results to view models
            result.CityResults = ConvertToCityResults(processor.TaskResults, config.SelectedCities);
            result.TelemetrySummary = ConvertToTelemetrySummary(processor.GetTelemetrySummary());
            result.DetailedTelemetry = ConvertToTaskTelemetry(processor.Telemetry);

            result.HasErrors = result.DetailedTelemetry.Any(t => !t.IsSuccessful);
        }
        catch (OperationCanceledException)
        {
            result.EndTime = DateTime.UtcNow;
            result.IsCompleted = false;
            result.HasErrors = true;
            result.ErrorMessage = "Processing was cancelled due to timeout";
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.UtcNow;
            result.IsCompleted = false;
            result.HasErrors = true;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error occurred during task processing");
        }

        return result;
    }

    public async Task<TaskProcessingResultViewModel> ProcessIndividualTaskAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        var result = new TaskProcessingResultViewModel
        {
            ProcessorName = "Individual Task Demo",
            StartTime = DateTime.UtcNow
        };

        try
        {
            using var processor = new TaskListProcessorEnhanced("Individual Task Demo", _logger);

            var weatherTask = _weatherService.GetWeather(city);
            var taskResult = await processor.ExecuteTaskAsync($"{city} Weather", weatherTask, cancellationToken);

            result.EndTime = DateTime.UtcNow;
            result.TotalDurationMs = (long)(result.EndTime.Value - result.StartTime).TotalMilliseconds;
            result.IsCompleted = true;

            // Convert individual result
            var cityResult = new CityResultViewModel { CityName = city };

            if (taskResult.IsSuccessful && taskResult.Data is IEnumerable<WeatherForecast> forecasts)
            {
                cityResult.Weather = new WeatherResultViewModel
                {
                    IsSuccessful = true,
                    Forecasts = forecasts.ToList(),
                    DurationMs = processor.Telemetry.FirstOrDefault()?.ElapsedMilliseconds ?? 0
                };
            }
            else
            {
                cityResult.Weather = new WeatherResultViewModel
                {
                    IsSuccessful = false,
                    ErrorMessage = taskResult.ErrorMessage
                };
            }

            result.CityResults = new List<CityResultViewModel> { cityResult };
            result.TelemetrySummary = ConvertToTelemetrySummary(processor.GetTelemetrySummary());
            result.DetailedTelemetry = ConvertToTaskTelemetry(processor.Telemetry);
            result.HasErrors = !taskResult.IsSuccessful;
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.UtcNow;
            result.IsCompleted = false;
            result.HasErrors = true;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error occurred during individual task processing");
        }

        return result;
    }

    public async Task<TaskProcessingResultViewModel> ProcessCancellationDemoAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new TaskProcessingResultViewModel
        {
            ProcessorName = "Cancellation Demo",
            StartTime = DateTime.UtcNow
        };

        try
        {
            using var processor = new TaskListProcessorEnhanced("Cancellation Demo", _logger);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMilliseconds(200)); // Short timeout for demo

            var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
            {
                ["Quick Task"] = async ct =>
                {
                    await Task.Delay(50, ct);
                    return "Quick result";
                },
                ["Medium Task"] = async ct =>
                {
                    await Task.Delay(150, ct);
                    return "Medium result";
                },
                ["Slow Task"] = async ct =>
                {
                    await Task.Delay(500, ct);
                    return "Slow result";
                }
            };

            await processor.ProcessTasksAsync(taskFactories, cancellationToken: cts.Token);

            result.EndTime = DateTime.UtcNow;
            result.IsCompleted = true;
        }
        catch (OperationCanceledException)
        {
            result.EndTime = DateTime.UtcNow;
            result.IsCompleted = false;
            result.HasErrors = true;
            result.ErrorMessage = "Some tasks were cancelled due to timeout (as expected for demo)";
        }

        result.TotalDurationMs = (long)(result.EndTime!.Value - result.StartTime).TotalMilliseconds;
        return result;
    }

    public async IAsyncEnumerable<TaskProcessingResultViewModel> ProcessStreamingDemoAsync(
        List<string> cities,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ProcessStreamingDemoAsync started for cities: {Cities}", string.Join(", ", cities));

        using var processor = new TaskListProcessorEnhanced("Streaming Demo", _logger);
        var taskFactories = CreateCityTaskFactories(cities);

        _logger.LogInformation("Created {Count} task factories", taskFactories.Count);

        var startTime = DateTime.UtcNow;
        var completedTasks = new List<TaskTelemetryViewModel>();

        await foreach (var taskResult in processor.ProcessTasksStreamAsync(taskFactories, cancellationToken))
        {
            _logger.LogInformation("Task completed: {TaskName}, Success: {IsSuccessful}",
                taskResult.Name, taskResult.IsSuccessful);

            var telemetryItem = ConvertSingleTaskTelemetry(taskResult);
            completedTasks.Add(telemetryItem);

            var result = new TaskProcessingResultViewModel
            {
                ProcessorName = "Streaming Demo",
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                IsCompleted = false,
                DetailedTelemetry = completedTasks.ToList(),
                TelemetrySummary = CalculatePartialTelemetrySummary(completedTasks)
            };

            _logger.LogInformation("Yielding intermediate result with {Count} tasks", completedTasks.Count);
            yield return result;
        }

        // Final result
        var finalResult = new TaskProcessingResultViewModel
        {
            ProcessorName = "Streaming Demo",
            StartTime = startTime,
            EndTime = DateTime.UtcNow,
            IsCompleted = true,
            DetailedTelemetry = completedTasks,
            TelemetrySummary = CalculatePartialTelemetrySummary(completedTasks),
            CityResults = ConvertToCityResultsFromTelemetry(completedTasks, cities)
        };

        _logger.LogInformation("Yielding final result with {Count} tasks", completedTasks.Count);
        yield return finalResult;
    }

    private Dictionary<string, Func<CancellationToken, Task<object?>>> CreateCityTaskFactories(List<string> cities)
    {
        var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        foreach (var city in cities)
        {
            taskFactories[$"{city} Weather"] = async ct => await _weatherService.GetWeather(city);
            taskFactories[$"{city} Things To Do"] = async ct => await _thingsToDoService.GetThingsToDoAsync(city);
        }

        return taskFactories;
    }

    private List<CityResultViewModel> ConvertToCityResults(
        IEnumerable<dynamic> taskResults,
        List<string> cities)
    {
        var cityResults = new List<CityResultViewModel>();

        foreach (var city in cities)
        {
            var cityResult = new CityResultViewModel { CityName = city };

            // Find weather result
            var weatherResult = taskResults.FirstOrDefault(r => r.Name == $"{city} Weather");
            if (weatherResult != null)
            {
                cityResult.Weather = new WeatherResultViewModel
                {
                    IsSuccessful = weatherResult.IsSuccessful,
                    ErrorMessage = weatherResult.IsSuccessful ? null : weatherResult.ErrorMessage?.ToString(),
                    Forecasts = weatherResult.IsSuccessful && weatherResult.Data is IEnumerable<WeatherForecast>
                        ? ((IEnumerable<WeatherForecast>)weatherResult.Data).ToList()
                        : new List<WeatherForecast>(),
                    DurationMs = GetTaskDurationMs(weatherResult)
                };
            }

            // Find activities result
            var activitiesResult = taskResults.FirstOrDefault(r => r.Name == $"{city} Things To Do");
            if (activitiesResult != null)
            {
                cityResult.Activities = new ActivitiesResultViewModel
                {
                    IsSuccessful = activitiesResult.IsSuccessful,
                    ErrorMessage = activitiesResult.IsSuccessful ? null : activitiesResult.ErrorMessage?.ToString(),
                    Activities = activitiesResult.IsSuccessful && activitiesResult.Data is IEnumerable<Activity>
                        ? ((IEnumerable<Activity>)activitiesResult.Data).ToList()
                        : new List<Activity>(),
                    DurationMs = GetTaskDurationMs(activitiesResult)
                };
            }

            cityResults.Add(cityResult);
        }

        return cityResults;
    }

    private long GetTaskDurationMs(dynamic taskResult)
    {
        try
        {
            // Try to get duration from the task result if available
            if (taskResult.GetType().GetProperty("DurationMs") != null)
            {
                return (long)taskResult.DurationMs;
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private TelemetrySummaryViewModel ConvertToTelemetrySummary(TelemetrySummary summary)
    {
        // Calculate throughput, avoiding division by zero
        var throughputPerSecond = summary.TotalExecutionTime > 0
            ? summary.TotalTasks / (summary.TotalExecutionTime / 1000.0)
            : 0.0;

        return new TelemetrySummaryViewModel
        {
            TotalTasks = summary.TotalTasks,
            SuccessfulTasks = summary.SuccessfulTasks,
            FailedTasks = summary.FailedTasks,
            SuccessRate = summary.SuccessRate,
            AverageExecutionTime = summary.AverageExecutionTime,
            MinExecutionTime = summary.MinExecutionTime,
            MaxExecutionTime = summary.MaxExecutionTime,
            TotalExecutionTime = summary.TotalExecutionTime,
            ThroughputPerSecond = throughputPerSecond
        };
    }

    private List<TaskTelemetryViewModel> ConvertToTaskTelemetry(IEnumerable<TaskTelemetry> telemetry)
    {
        return telemetry.Select(ConvertSingleTaskTelemetry).ToList();
    }

    private TaskTelemetryViewModel ConvertSingleTaskTelemetry(dynamic taskResult)
    {
        var elapsed = taskResult.GetType().GetProperty("ElapsedMilliseconds")?.GetValue(taskResult) ?? 0L;
        var elapsedMs = Convert.ToInt64(elapsed);

        return new TaskTelemetryViewModel
        {
            TaskName = taskResult.Name?.ToString() ?? "Unknown",
            ElapsedMilliseconds = elapsedMs,
            IsSuccessful = taskResult.IsSuccessful,
            ErrorMessage = taskResult.IsSuccessful ? null : taskResult.ErrorMessage?.ToString(),
            ErrorType = taskResult.IsSuccessful ? null : taskResult.ErrorCategory?.ToString(),
            PerformanceLevel = GetPerformanceLevel(elapsedMs),
            PerformanceIcon = GetPerformanceIcon(elapsedMs),
            PerformanceColor = GetPerformanceColor(elapsedMs)
        };
    }

    private TaskTelemetryViewModel ConvertSingleTaskTelemetry(TaskTelemetry telemetry)
    {
        return new TaskTelemetryViewModel
        {
            TaskName = telemetry.TaskName,
            ElapsedMilliseconds = telemetry.ElapsedMilliseconds,
            IsSuccessful = telemetry.IsSuccessful,
            ErrorMessage = telemetry.ErrorMessage,
            ErrorType = telemetry.ErrorType,
            PerformanceLevel = GetPerformanceLevel(telemetry.ElapsedMilliseconds),
            PerformanceIcon = GetPerformanceIcon(telemetry.ElapsedMilliseconds),
            PerformanceColor = GetPerformanceColor(telemetry.ElapsedMilliseconds)
        };
    }

    private string GetPerformanceLevel(long milliseconds) => milliseconds switch
    {
        < 200 => "Ultra Fast",
        < 500 => "Fast",
        < 1000 => "Normal",
        < 2000 => "Slow",
        _ => "Critical"
    };

    private string GetPerformanceIcon(long milliseconds) => milliseconds switch
    {
        < 200 => "rocket-takeoff",
        < 500 => "lightning",
        < 1000 => "check-circle",
        < 2000 => "hourglass",
        _ => "exclamation-triangle"
    };

    private string GetPerformanceColor(long milliseconds) => milliseconds switch
    {
        < 200 => "success",
        < 500 => "info",
        < 1000 => "primary",
        < 2000 => "warning",
        _ => "danger"
    };

    private TelemetrySummaryViewModel CalculatePartialTelemetrySummary(List<TaskTelemetryViewModel> telemetry)
    {
        if (!telemetry.Any())
        {
            return new TelemetrySummaryViewModel();
        }

        var successful = telemetry.Count(t => t.IsSuccessful);
        var total = telemetry.Count;
        var totalTime = telemetry.Sum(t => t.ElapsedMilliseconds);

        // Calculate throughput, avoiding division by zero
        var throughputPerSecond = totalTime > 0 ? total / (totalTime / 1000.0) : 0.0;

        return new TelemetrySummaryViewModel
        {
            TotalTasks = total,
            SuccessfulTasks = successful,
            FailedTasks = total - successful,
            SuccessRate = (double)successful / total * 100,
            AverageExecutionTime = telemetry.Average(t => t.ElapsedMilliseconds),
            MinExecutionTime = telemetry.Min(t => t.ElapsedMilliseconds),
            MaxExecutionTime = telemetry.Max(t => t.ElapsedMilliseconds),
            TotalExecutionTime = totalTime,
            ThroughputPerSecond = throughputPerSecond
        };
    }

    private List<CityResultViewModel> ConvertToCityResultsFromTelemetry(
        List<TaskTelemetryViewModel> telemetry,
        List<string> cities)
    {
        var cityResults = new List<CityResultViewModel>();

        foreach (var city in cities)
        {
            var cityResult = new CityResultViewModel { CityName = city };

            var weatherTelemetry = telemetry.FirstOrDefault(t => t.TaskName == $"{city} Weather");
            if (weatherTelemetry != null)
            {
                cityResult.Weather = new WeatherResultViewModel
                {
                    IsSuccessful = weatherTelemetry.IsSuccessful,
                    ErrorMessage = weatherTelemetry.ErrorMessage,
                    DurationMs = weatherTelemetry.ElapsedMilliseconds,
                    Forecasts = new List<WeatherForecast>() // Would need actual data from processor
                };
            }

            var activitiesTelemetry = telemetry.FirstOrDefault(t => t.TaskName == $"{city} Things To Do");
            if (activitiesTelemetry != null)
            {
                cityResult.Activities = new ActivitiesResultViewModel
                {
                    IsSuccessful = activitiesTelemetry.IsSuccessful,
                    ErrorMessage = activitiesTelemetry.ErrorMessage,
                    DurationMs = activitiesTelemetry.ElapsedMilliseconds,
                    Activities = new List<Activity>() // Would need actual data from processor
                };
            }

            cityResults.Add(cityResult);
        }

        return cityResults;
    }
}
