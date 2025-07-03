// Example usage demonstrating the improved TaskListProcessor

using Microsoft.Extensions.Logging;

namespace TaskListProcessing;

/// <summary>
/// Provides usage examples for the improved TaskListProcessor.
/// </summary>
public static class UsageExamples
{
    /// <summary>
    /// Demonstrates basic usage with improved error handling.
    /// </summary>
    public static async Task BasicUsageExampleAsync()
    {
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("UsageExamples");

        using var processor = new TaskListProcessorImproved("City Data Processing", logger);

        // Configure tasks with proper error handling and cancellation
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
        {
            ["London Weather"] = async ct => await GetWeatherAsync("London", ct),
            ["Paris Weather"] = async ct => await GetWeatherAsync("Paris", ct),
            ["London Activities"] = async ct => await GetActivitiesAsync("London", ct),
            ["Paris Activities"] = async ct => await GetActivitiesAsync("Paris", ct)
        }; try
        {
            await processor.ProcessTasksAsync(tasks, cts.Token);

            // Get telemetry summary
            var summary = processor.GetTelemetrySummary();
            logger.LogInformation("Processing Summary: {Summary}", summary);

            // Process results
            foreach (var result in processor.TaskResults)
            {
                if (result.IsSuccessful)
                {
                    logger.LogInformation("✅ {TaskName} completed successfully", result.Name);
                }
                else
                {
                    logger.LogWarning("❌ {TaskName} failed: {Error}", result.Name, result.ErrorMessage);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Processing was cancelled due to timeout");
        }
    }

    /// <summary>
    /// Demonstrates individual task execution with strongly typed results.
    /// </summary>
    public static async Task StronglyTypedExampleAsync()
    {
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("UsageExamples");
        using var processor = new TaskListProcessorImproved("Individual Task Processing", logger);
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        // Using individual task execution with strongly typed results
        var weatherTask = GetWeatherDataAsync("Tokyo", cts.Token);
        var weatherResult = await processor.ExecuteTaskAsync("Tokyo Weather", weatherTask, cts.Token);

        if (weatherResult.IsSuccessful && weatherResult.Data != null)
        {
            logger.LogInformation("Weather data received: {Data}", weatherResult.Data);
        }
    }

    // Mock methods for demonstration
    private static async Task<object?> GetWeatherAsync(string city, CancellationToken ct)
    {
        await Task.Delay(Random.Shared.Next(100, 1000), ct);
        if (Random.Shared.NextDouble() < 0.1) // 10% failure rate
            throw new InvalidOperationException($"Weather service unavailable for {city}");
        return $"Weather data for {city}";
    }

    private static async Task<object?> GetActivitiesAsync(string city, CancellationToken ct)
    {
        await Task.Delay(Random.Shared.Next(200, 800), ct);
        if (Random.Shared.NextDouble() < 0.05) // 5% failure rate
            throw new InvalidOperationException($"Activities service unavailable for {city}");
        return $"Activities data for {city}";
    }

    private static async Task<WeatherData> GetWeatherDataAsync(string city, CancellationToken ct)
    {
        await Task.Delay(Random.Shared.Next(100, 1000), ct);
        if (Random.Shared.NextDouble() < 0.1) // 10% failure rate
            throw new InvalidOperationException($"Weather service unavailable for {city}");
        return new WeatherData { City = city, Temperature = Random.Shared.Next(-10, 35), Description = "Sunny" };
    }

    /// <summary>
    /// Sample weather data class for strongly typed examples.
    /// </summary>
    public class WeatherData
    {
        public string City { get; set; } = string.Empty;
        public int Temperature { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
