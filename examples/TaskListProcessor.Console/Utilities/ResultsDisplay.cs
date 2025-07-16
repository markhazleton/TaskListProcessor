// Program.cs - Enhanced TaskListProcessor Console Application
// Demonstrates advanced asynchronous task processing with impressive telemetry and clear output formatting

using TaskListProcessing.Core;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

// Enhanced results display
public static class ResultsDisplay
{
    public static void ShowEnhancedResults(TaskListProcessorEnhanced processor, AppConfiguration config)
    {
        OutputFormatter.PrintSubHeader("[CITY] CITY DATA RESULTS");

        var cityGroups = processor.TaskResults
            .GroupBy(r => r.Name.Split(' ')[0])
            .OrderBy(g => g.Key)
            .ToList();

        Console.WriteLine($"Retrieved data for {cityGroups.Count} cities with {processor.TaskResults.Count()} total tasks:");
        Console.WriteLine();

        foreach (var cityGroup in cityGroups)
        {
            ShowCityResults(cityGroup, config);
        }

        ShowResultsSummary(processor);
    }

    private static void ShowCityResults(IGrouping<string, dynamic> cityGroup, AppConfiguration config)
    {
        var cityName = cityGroup.Key;
        Console.WriteLine($"[CITY] {cityName}");
        Console.WriteLine("+-----------------------------------------------------------------------------");

        foreach (var result in cityGroup.OrderBy(r => r.Name))
        {
            if (result.Name.Contains("Weather"))
            {
                ShowWeatherResults(result, config);
            }
            else if (result.Name.Contains("Things To Do"))
            {
                ShowActivityResults(result, config);
            }
        }

        Console.WriteLine();
    }

    private static void ShowWeatherResults(dynamic result, AppConfiguration config)
    {
        Console.WriteLine("| [WEATHER] Weather Forecast:");

        if (!result.IsSuccessful)
        {
            OutputFormatter.PrintError($"|   Failed to retrieve weather data: {result.ErrorMessage}");
            return;
        }

        if (result.Data is IEnumerable<WeatherForecast> weatherData)
        {
            var forecasts = weatherData.Take(config.MaxForecastDays).ToList();
            Console.WriteLine($"|   [CALENDAR] {forecasts.Count} day forecast:");

            foreach (var forecast in forecasts)
            {
                var tempIcon = GetTemperatureIcon(forecast.TemperatureF);
                var tempColor = GetTemperatureColor(forecast.TemperatureF);

                Console.ForegroundColor = tempColor;
                Console.WriteLine($"|     {tempIcon} {forecast.Date:ddd MMM dd}: {forecast.TemperatureF,3}°F - {forecast.Summary}");
                Console.ResetColor();
            }
        }
        else
        {
            OutputFormatter.PrintWarning("|   No weather data available");
        }
    }

    private static void ShowActivityResults(dynamic result, AppConfiguration config)
    {
        Console.WriteLine("| [ACTIVITIES] Things To Do:");

        if (!result.IsSuccessful)
        {
            OutputFormatter.PrintError($"|   Failed to retrieve activities: {result.ErrorMessage}");
            return;
        }

        if (result.Data is IEnumerable<Activity> activityData)
        {
            var activities = activityData.Take(config.MaxResultsToShow).ToList();
            Console.WriteLine($"|   [TARGET] Top {activities.Count} activities:");

            foreach (var activity in activities)
            {
                var priceIcon = GetPriceIcon(activity.PricePerPerson);
                var priceColor = GetPriceColor(activity.PricePerPerson);

                Console.ForegroundColor = priceColor;
                Console.WriteLine($"|     {priceIcon} {activity.Name,-40} ${activity.PricePerPerson,6:F2}");
                Console.ResetColor();
            }
        }
        else
        {
            OutputFormatter.PrintWarning("|   No activities available");
        }
    }

    private static void ShowResultsSummary(TaskListProcessorEnhanced processor)
    {
        var total = processor.TaskResults.Count();
        var successful = processor.TaskResults.Count(r => r.IsSuccessful);
        var failed = total - successful;

        Console.WriteLine("[SUMMARY] RESULTS SUMMARY");
        Console.WriteLine("+------------------------------------------------------------------------------+");
        Console.WriteLine($"|  Total Tasks: {total,3} | Successful: {successful,3} ({(double)successful / total * 100:F1}%) | Failed: {failed,3} ({(double)failed / total * 100:F1}%) |");
        Console.WriteLine("+------------------------------------------------------------------------------+");
    }

    private static string GetTemperatureIcon(int temp) => temp switch
    {
        > 85 => "[HOT]",
        > 75 => "[WARM]",
        > 65 => "[NICE]",
        > 50 => "[COOL]",
        > 35 => "[COLD]",
        _ => "[FREEZING]"
    };

    private static ConsoleColor GetTemperatureColor(int temp) => temp switch
    {
        > 85 => ConsoleColor.Red,
        > 75 => ConsoleColor.Yellow,
        > 65 => ConsoleColor.Green,
        > 50 => ConsoleColor.Cyan,
        > 35 => ConsoleColor.Blue,
        _ => ConsoleColor.White
    };

    private static string GetPriceIcon(decimal price) => price switch
    {
        0 => "[FREE]",
        < 20 => "[CHEAP]",
        < 50 => "[MEDIUM]",
        < 100 => "[EXPENSIVE]",
        _ => "[PREMIUM]"
    };

    private static ConsoleColor GetPriceColor(decimal price) => price switch
    {
        0 => ConsoleColor.Green,
        < 20 => ConsoleColor.Yellow,
        < 50 => ConsoleColor.Cyan,
        < 100 => ConsoleColor.Magenta,
        _ => ConsoleColor.Red
    };
}
