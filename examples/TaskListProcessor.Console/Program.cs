// Program.cs - Enhanced TaskListProcessor Console Application
// Demonstrates advanced asynchronous task processing with impressive telemetry and clear output formatting

using CityThingsToDo;
using CityWeatherService;
using Microsoft.Extensions.Logging;
using TaskListProcessing;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

// Main Program
public class Program
{
    private static readonly AppConfiguration Config = new();

    public static async Task Main(string[] args)
    {
        // Initialize application
        OutputFormatter.PrintHeader("[ROCKET] TASKLISTPROCESSOR DEMONSTRATION",
            "Advanced Asynchronous Task Processing with Comprehensive Telemetry");

        OutputFormatter.PrintInfo($"Application started at: {TimeProvider.System.GetLocalNow():yyyy-MM-dd HH:mm:ss}");
        OutputFormatter.PrintInfo($"Processing {Config.Cities.Count} cities with {Config.MaxConcurrentTasks} max concurrent tasks");

        // Set up logging
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<Program>();

        // Initialize services
        var thingsToDoService = new CityThingsToDoService();
        var weatherService = new WeatherService();

        // Run main processing demonstration
        await RunMainProcessingDemo(weatherService, thingsToDoService, logger);

        if (Config.RunDemoScenarios)
        {
            // Run additional demonstration scenarios
            await RunIndividualTaskDemo(weatherService, logger);
            await RunCancellationDemo(logger);
            await RunConcurrentProcessingDemo(weatherService, thingsToDoService, logger);
        }

        OutputFormatter.PrintHeader("[TARGET] DEMONSTRATION COMPLETE",
            $"Application finished at: {TimeProvider.System.GetLocalNow():yyyy-MM-dd HH:mm:ss}");
    }

    private static async Task RunMainProcessingDemo(WeatherService weatherService,
        CityThingsToDoService thingsToDoService, ILogger logger)
    {
        OutputFormatter.PrintSubHeader("[CITY] MAIN PROCESSING - CITY DATA COLLECTION");

        OutputFormatter.PrintInfo("Initializing task processor and creating task factories...");

        using var taskProcessor = new TaskListProcessorImproved("City Data Collection", logger);
        using var cts = new CancellationTokenSource(Config.DefaultTimeout);

        var taskFactories = CreateCityTaskFactories(Config.Cities, weatherService, thingsToDoService);

        OutputFormatter.PrintInfo($"Created {taskFactories.Count} task factories for {Config.Cities.Count} cities");
        OutputFormatter.PrintInfo("Starting asynchronous task processing...");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await taskProcessor.ProcessTasksAsync(taskFactories, cts.Token);
            stopwatch.Stop();

            OutputFormatter.PrintSuccess($"All tasks completed successfully in {stopwatch.ElapsedMilliseconds:N0}ms");

            // Show impressive telemetry
            TelemetryDisplay.ShowImpressiveTelemetry(taskProcessor, "Main Processing");

            // Show enhanced results
            if (Config.ShowIndividualResults)
            {
                ResultsDisplay.ShowEnhancedResults(taskProcessor, Config);
            }
        }
        catch (OperationCanceledException)
        {
            OutputFormatter.PrintError("Processing was cancelled due to timeout");
        }
        catch (Exception ex)
        {
            OutputFormatter.PrintError($"An error occurred: {ex.Message}");
        }
    }

    private static async Task RunIndividualTaskDemo(WeatherService weatherService, ILogger logger)
    {
        OutputFormatter.PrintSubHeader("[TARGET] INDIVIDUAL TASK EXECUTION DEMO");

        OutputFormatter.PrintInfo("Demonstrating individual task execution with detailed telemetry...");

        using var processor = new TaskListProcessorImproved("Individual Task Demo", logger);

        var weatherTask = weatherService.GetWeather("Seattle");
        var result = await processor.ExecuteTaskAsync("Seattle Weather", weatherTask);

        if (result.IsSuccessful)
        {
            OutputFormatter.PrintSuccess("Individual task completed successfully");

            if (result.Data is IEnumerable<WeatherForecast> forecasts)
            {
                Console.WriteLine("\n[WEATHER] Seattle Weather Preview:");
                foreach (var forecast in forecasts.Take(2))
                {
                    Console.WriteLine($"   {forecast.Date:MMM dd}: {forecast.TemperatureF}°F - {forecast.Summary}");
                }
            }
        }
        else
        {
            OutputFormatter.PrintError($"Individual task failed: {result.ErrorMessage}");
        }

        TelemetryDisplay.ShowImpressiveTelemetry(processor, "Individual Task");
    }

    private static async Task RunCancellationDemo(ILogger logger)
    {
        OutputFormatter.PrintSubHeader("[TIME] CANCELLATION & TIMEOUT DEMO");

        OutputFormatter.PrintInfo("Demonstrating cancellation handling with short timeout...");

        using var processor = new TaskListProcessorImproved("Cancellation Demo", logger);
        using var cts = new CancellationTokenSource(Config.ShortTimeout);

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

        try
        {
            await processor.ProcessTasksAsync(taskFactories, cts.Token);
            OutputFormatter.PrintSuccess("All tasks completed within timeout");
        }
        catch (OperationCanceledException)
        {
            OutputFormatter.PrintWarning("Some tasks were cancelled due to timeout (as expected)");
        }

        TelemetryDisplay.ShowImpressiveTelemetry(processor, "Cancellation Demo");
    }

    private static async Task RunConcurrentProcessingDemo(WeatherService weatherService,
        CityThingsToDoService thingsToDoService, ILogger logger)
    {
        OutputFormatter.PrintSubHeader("[ROCKET] CONCURRENT PROCESSING PERFORMANCE DEMO");

        var scenarios = new[]
        {
            new { Name = "Small Batch", Cities = new[] { "Boston", "Miami" } },
            new { Name = "Medium Batch", Cities = new[] { "Denver", "Phoenix", "Portland" } },
            new { Name = "Large Batch", Cities = new[] { "Atlanta", "Detroit", "Houston", "Las Vegas" } }
        };

        OutputFormatter.PrintInfo("Comparing performance across different batch sizes...");

        foreach (var scenario in scenarios)
        {
            Console.WriteLine($"\n[PROC] Processing {scenario.Name} ({scenario.Cities.Length} cities)...");

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
                var throughput = summary.TotalTasks / (stopwatch.ElapsedMilliseconds / 1000.0);

                OutputFormatter.PrintSuccess($"{scenario.Name} completed - {stopwatch.ElapsedMilliseconds:N0}ms - {throughput:F1} tasks/sec");
            }
            catch (Exception ex)
            {
                OutputFormatter.PrintError($"{scenario.Name} failed: {ex.Message}");
            }
        }
    }

    private static Dictionary<string, Func<CancellationToken, Task<object?>>> CreateCityTaskFactories(
        List<string> cities, WeatherService weatherService, CityThingsToDoService thingsToDoService)
    {
        var taskFactories = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

        foreach (var city in cities)
        {
            taskFactories[$"{city} Weather"] = async ct => await weatherService.GetWeather(city);
            taskFactories[$"{city} Things To Do"] = async ct => await thingsToDoService.GetThingsToDoAsync(city);
        }

        return taskFactories;
    }
}