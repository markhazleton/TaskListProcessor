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
var taskProcessor = new TaskListProcessorGeneric();

// List of cities to process (could be loaded from config in the future)
var cities = GetCitiesToProcess();
var tasks = CreateCityTasks(cities, taskProcessor, weatherService, thingsToDoService);

try
{
    // Await all tasks and log their completion
    await TaskListProcessorGeneric.WhenAllWithLoggingAsync(tasks, logger);
    logger.LogInformation("All tasks completed\n\n");
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
/// Creates the list of tasks for weather and things-to-do for each city.
/// </summary>
List<Task> CreateCityTasks(
    List<string> cities,
    TaskListProcessorGeneric taskProcessor,
    CityWeatherService.WeatherService weatherService,
    CityThingsToDoService thingsToDoService)
{
    var tasks = new List<Task>();
    foreach (var city in cities)
    {
        // Add weather data task
        tasks.Add(taskProcessor.GetTaskResultAsync($"{city} Weather", weatherService.GetWeather(city)));
        // Add things to do task
        tasks.Add(taskProcessor.GetTaskResultAsync($"{city} Things To Do", thingsToDoService.GetThingsToDoAsync(city)));
    }
    return tasks;
}

/// <summary>
/// Prints telemetry data using the logger.
/// </summary>
void PrintTelemetry(TaskListProcessorGeneric taskProcessor, ILogger logger)
{
    logger.LogInformation("Telemetry:");
    foreach (var cityTelemetry in taskProcessor.Telemetry.OrderBy(o => o))
    {
        logger.LogInformation(cityTelemetry);
    }
}

/// <summary>
/// Prints the results for each city, grouped by type, using the logger.
/// </summary>
void PrintResults(TaskListProcessorGeneric taskProcessor, ILogger logger)
{
    logger.LogInformation("\n\nResults:");
    foreach (var city in taskProcessor.TaskResults.OrderBy(o => o.Name))
    {
        logger.LogInformation($"{city.Name}:");

        // Handle weather results
        if (city is TaskResult<IEnumerable<WeatherForecast>> cityWeather)
        {
            if (cityWeather.Data is null)
            {
                logger.LogWarning("No weather data available");
                continue;
            }
            else
            {
                foreach (var forecast in cityWeather.Data)
                {
                    logger.LogInformation(forecast.ToString());
                }
            }
        }
        // Handle things to do results
        else if (city is TaskResult<IEnumerable<Activity>> cityActivity)
        {
            if (cityActivity.Data is null)
            {
                logger.LogWarning("No activity data available");
            }
            else
            {
                foreach (var activity in cityActivity.Data)
                {
                    logger.LogInformation(activity.ToString());
                }
            }
        }
    }
}
