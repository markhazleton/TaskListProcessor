// Program.cs - Entry point for TaskListProcessor.Console
// This application fetches weather and things-to-do data for a list of cities,
// processes the results asynchronously, and outputs telemetry and results.

/// <summary>
/// Entry point for the TaskListProcessor.Console application. Demonstrates asynchronous task processing,
/// telemetry collection, and result aggregation for weather and things-to-do data across multiple cities.
/// </summary>

using CityThingsToDo;
using CityWeatherService;
using Microsoft.Extensions.Logging;
using TaskListProcessing;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

// Print the current local time
Console.WriteLine(TimeProvider.System.GetLocalNow());

// Set up logging to the console
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

// Initialize services for fetching things to do and weather data
var thingsToDoService = new CityThingsToDoService();
var weatherService = new WeatherService();

// Use the improved TaskListProcessor with proper resource management
using var taskProcessor = new TaskListProcessorImproved("City Data Processing", logger);

// Set up cancellation token for timeout handling
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

// List of cities to process (could be loaded from config in the future)
var cities = GetCitiesToProcess();
var taskFactories = CreateCityTaskFactories(cities, weatherService, thingsToDoService);

try
{
    // Process all tasks using the improved processor
    await taskProcessor.ProcessTasksAsync(taskFactories, cts.Token);
    logger.LogInformation("All tasks completed\n\n");
}
catch (OperationCanceledException)
{
    logger.LogWarning("Processing was cancelled due to timeout.");
    return;
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while processing tasks.");
    return;
}

// Output telemetry data collected during processing
PrintTelemetry(taskProcessor, logger);

// Output the results for each city, grouped by type
PrintResults(taskProcessor, logger);

// Demonstrate individual task execution with strongly typed results
await DemonstrateIndividualTaskExecution(logger);

// Demonstrate timeout and cancellation handling
await DemonstrateCancellationHandling(logger);

// Demonstrate concurrent processing with different configurations
await DemonstrateConcurrentProcessing(logger);

// Print the current local time at the end
Console.WriteLine(TimeProvider.System.GetLocalNow());
Console.WriteLine();

// Helper methods for organization and clarity

/// <summary>
/// Returns the list of cities to process. In a real app, this could be loaded from config or arguments.
/// </summary>
List<string> GetCitiesToProcess() =>
    new() { "London", "Paris", "New York", "Tokyo", "Sydney", "Chicago", "Dallas", "Wichita" };

/// <summary>
/// Creates the dictionary of task factories for weather and things-to-do for each city.
/// </summary>
Dictionary<string, Func<CancellationToken, Task<object?>>> CreateCityTaskFactories(
    List<string> cities,
    CityWeatherService.WeatherService weatherService,
    CityThingsToDoService thingsToDoService)
{
    var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

    foreach (var city in cities)
    {
        // Add weather data task factory
        taskFactories[$"{city} Weather"] = async ct =>
        {
            var weather = await weatherService.GetWeather(city);
            return weather;
        };

        // Add things to do task factory
        taskFactories[$"{city} Things To Do"] = async ct =>
        {
            var activities = await thingsToDoService.GetThingsToDoAsync(city);
            return activities;
        };
    }

    return taskFactories;
}

/// <summary>
/// Prints telemetry data using the logger with enhanced formatting.
/// </summary>
void PrintTelemetry(TaskListProcessorImproved taskProcessor, ILogger logger)
{
    var summary = taskProcessor.GetTelemetrySummary();

    logger.LogInformation("\n=== 📊 TELEMETRY SUMMARY ===");
    logger.LogInformation("📈 Total Tasks: {TotalTasks}", summary.TotalTasks);
    logger.LogInformation("✅ Successful: {SuccessfulTasks} ({SuccessRate:F1}%)",
        summary.SuccessfulTasks, summary.SuccessRate);
    logger.LogInformation("❌ Failed: {FailedTasks}", summary.FailedTasks);
    logger.LogInformation("⏱️ Average Time: {AvgTime:F0}ms", summary.AverageExecutionTime);
    logger.LogInformation("🏃 Fastest: {MinTime}ms | 🐌 Slowest: {MaxTime}ms",
        summary.MinExecutionTime, summary.MaxExecutionTime);
    logger.LogInformation("⏰ Total Execution Time: {TotalTime:N0}ms", summary.TotalExecutionTime);

    logger.LogInformation("\n=== 📋 DETAILED TELEMETRY ===");

    var successfulTasks = taskProcessor.Telemetry.Where(t => t.IsSuccessful).OrderBy(t => t.ElapsedMilliseconds).ToList();
    var failedTasks = taskProcessor.Telemetry.Where(t => !t.IsSuccessful).OrderBy(t => t.TaskName).ToList();

    if (successfulTasks.Any())
    {
        logger.LogInformation("✅ Successful Tasks (sorted by execution time):");
        foreach (var telemetry in successfulTasks)
        {
            var performance = telemetry.ElapsedMilliseconds switch
            {
                < 500 => "🚀",
                < 1000 => "⚡",
                < 2000 => "🏃",
                _ => "🐌"
            };
            logger.LogInformation("  {Performance} {TaskName}: {Time}ms",
                performance, telemetry.TaskName, telemetry.ElapsedMilliseconds);
        }
    }

    if (failedTasks.Any())
    {
        logger.LogInformation("\n❌ Failed Tasks:");
        foreach (var telemetry in failedTasks)
        {
            logger.LogWarning("  💥 {TaskName}: {ErrorType} after {Time}ms - {ErrorMessage}",
                telemetry.TaskName, telemetry.ErrorType, telemetry.ElapsedMilliseconds, telemetry.ErrorMessage);
        }
    }
}

/// <summary>
/// Prints the results for each city with enhanced formatting and organization.
/// </summary>
void PrintResults(TaskListProcessorImproved taskProcessor, ILogger logger)
{
    logger.LogInformation("\n=== 🏙️ CITY DATA RESULTS ===");

    var cityGroups = taskProcessor.TaskResults
        .GroupBy(r => r.Name.Split(' ')[0]) // Group by city name
        .OrderBy(g => g.Key);

    foreach (var cityGroup in cityGroups)
    {
        var cityName = cityGroup.Key;
        logger.LogInformation("\n🌍 {CityName}:", cityName);

        foreach (var result in cityGroup.OrderBy(r => r.Name))
        {
            var taskType = result.Name.Contains("Weather") ? "🌤️ Weather" : "🎭 Activities";

            if (!result.IsSuccessful)
            {
                logger.LogWarning("  {TaskType}: ❌ Failed - {Error}", taskType, result.ErrorMessage);
                continue;
            }

            logger.LogInformation("  {TaskType}: ✅ Success", taskType);

            // Handle weather results with better formatting
            if (result.Name.Contains("Weather") && result is EnhancedTaskResult<object> weatherResult)
            {
                if (weatherResult.Data is IEnumerable<WeatherForecast> weatherData)
                {
                    var forecasts = weatherData.Take(3).ToList(); // Show first 3 days
                    foreach (var forecast in forecasts)
                    {
                        var tempIcon = forecast.TemperatureF switch
                        {
                            > 80 => "🔥",
                            > 60 => "☀️",
                            > 40 => "🌤️",
                            _ => "❄️"
                        };
                        logger.LogInformation("    {Icon} {Date:MMM dd}: {Temp}°F - {Summary}",
                            tempIcon, forecast.Date, forecast.TemperatureF, forecast.Summary);
                    }
                }
                else
                {
                    logger.LogWarning("    ⚠️ No weather data available");
                }
            }
            // Handle activity results with better formatting
            else if (result.Name.Contains("Things To Do") && result is EnhancedTaskResult<object> activityResult)
            {
                if (activityResult.Data is IEnumerable<Activity> activityData)
                {
                    var activities = activityData.Take(3).ToList(); // Show first 3 activities
                    foreach (var activity in activities)
                    {
                        var priceIcon = activity.PricePerPerson switch
                        {
                            0 => "🆓",
                            < 20 => "💰",
                            < 50 => "💸",
                            _ => "💎"
                        };
                        logger.LogInformation("    {Icon} {ActivityName} - ${Price:F2}",
                            priceIcon, activity.Name, activity.PricePerPerson);
                    }
                }
                else
                {
                    logger.LogWarning("    ⚠️ No activity data available");
                }
            }
        }
    }

    // Show summary statistics
    var totalResults = taskProcessor.TaskResults.Count();
    var successfulResults = taskProcessor.TaskResults.Count(r => r.IsSuccessful);
    var failedResults = totalResults - successfulResults;

    logger.LogInformation("\n=== 📊 RESULTS SUMMARY ===");
    logger.LogInformation("🎯 Total Tasks: {Total} | ✅ Successful: {Success} | ❌ Failed: {Failed}",
        totalResults, successfulResults, failedResults);
}

/// <summary>
/// Demonstrates individual task execution with strongly typed results and detailed telemetry.
/// </summary>
async Task DemonstrateIndividualTaskExecution(ILogger logger)
{
    logger.LogInformation("\n=== Individual Task Execution Demo ===");

    using var processor = new TaskListProcessorImproved("Individual Task Demo", logger);
    var weatherService = new WeatherService();

    // Execute a single strongly-typed task
    var weatherTask = weatherService.GetWeather("Seattle");
    var result = await processor.ExecuteTaskAsync("Seattle Weather", weatherTask);

    if (result.IsSuccessful && result.Data != null)
    {
        logger.LogInformation("✅ Individual task completed successfully");
        logger.LogInformation("Task Name: {TaskName}", result.Name);
        logger.LogInformation("Execution Time: {ElapsedMs}ms",
            processor.Telemetry.First().ElapsedMilliseconds);

        if (result.Data is IEnumerable<WeatherForecast> forecasts)
        {
            foreach (var forecast in forecasts.Take(2)) // Show first 2 forecasts
            {
                logger.LogInformation("Weather: {Forecast}", forecast);
            }
        }
    }
    else
    {
        logger.LogWarning("❌ Individual task failed: {Error}", result.ErrorMessage);
    }

    // Show telemetry summary for this single task
    var summary = processor.GetTelemetrySummary();
    logger.LogInformation("Single Task Summary: {Summary}", summary);
}

/// <summary>
/// Demonstrates timeout and cancellation handling capabilities.
/// </summary>
async Task DemonstrateCancellationHandling(ILogger logger)
{
    logger.LogInformation("\n=== Cancellation & Timeout Demo ===");

    using var processor = new TaskListProcessorImproved("Cancellation Demo", logger);
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Very short timeout

    var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>
    {
        ["Quick Task"] = async ct =>
        {
            await Task.Delay(50, ct); // Should complete
            return "Quick result";
        },
        ["Slow Task"] = async ct =>
        {
            await Task.Delay(500, ct); // Should be cancelled
            return "Slow result";
        }
    };

    try
    {
        await processor.ProcessTasksAsync(taskFactories, cts.Token);
    }
    catch (OperationCanceledException)
    {
        logger.LogInformation("Tasks were cancelled as expected due to timeout");
    }

    // Show results and telemetry
    var summary = processor.GetTelemetrySummary();
    logger.LogInformation("Cancellation Demo Summary: {Summary}", summary);

    foreach (var result in processor.TaskResults)
    {
        var status = result.IsSuccessful ? "✅ Completed" : "❌ Failed/Cancelled";
        logger.LogInformation("{Status}: {TaskName} - {Error}",
            status, result.Name, result.ErrorMessage ?? "Success");
    }
}

/// <summary>
/// Demonstrates concurrent processing with performance comparison.
/// </summary>
async Task DemonstrateConcurrentProcessing(ILogger logger)
{
    logger.LogInformation("\n=== Concurrent Processing Demo ===");

    // Create multiple processors to show different scenarios
    var scenarios = new[]
    {
        new { Name = "Small Batch", Cities = new[] { "Boston", "Miami" } },
        new { Name = "Medium Batch", Cities = new[] { "Denver", "Phoenix", "Portland", "Nashville" } },
        new { Name = "Large Batch", Cities = new[] { "Atlanta", "Detroit", "Houston", "Las Vegas", "Memphis", "Milwaukee" } }
    };

    var weatherService = new WeatherService();
    var thingsToDoService = new CityThingsToDoService();

    foreach (var scenario in scenarios)
    {
        logger.LogInformation("\n--- {ScenarioName} ({CityCount} cities) ---",
            scenario.Name, scenario.Cities.Length);

        using var processor = new TaskListProcessorImproved($"{scenario.Name} Processor", logger);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var taskFactories = scenario.Cities.ToDictionary(
            city => $"{city} Weather",
            city => (Func<CancellationToken, Task<object?>>)(async ct => await weatherService.GetWeather(city))
        );

        try
        {
            await processor.ProcessTasksAsync(taskFactories);
            stopwatch.Stop();

            var summary = processor.GetTelemetrySummary();
            logger.LogInformation("Scenario Complete - Total Time: {TotalMs}ms", stopwatch.ElapsedMilliseconds);
            logger.LogInformation("Batch Summary: {Summary}", summary);
            logger.LogInformation("Throughput: {Throughput:F1} tasks/second",
                summary.TotalTasks / (stopwatch.ElapsedMilliseconds / 1000.0));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scenario {ScenarioName} failed", scenario.Name);
        }
    }
}
