using CityWeatherService;
using Microsoft.Extensions.Logging;
using static CityWeatherService.WeatherService;

Console.WriteLine(TimeProvider.System.GetLocalNow());
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var weatherService = new WeatherService();
var weatherCities = new TaskListProcessing.TaskListProcessor();
var cities = new List<string> { "London", "Paris", "New York", "Tokyo", "Sydney","Chicago","Dallas","Wichita" };
var tasks = new List<Task>();
foreach (var city in cities)
{
    tasks.Add(weatherCities.GetTaskResultAsync(city, weatherService.GetWeather(city)));
}
await TaskListProcessing.TaskListProcessor.WhenAllWithLoggingAsync(tasks, logger);

Console.WriteLine("All tasks completed\n\n");
Console.WriteLine("Telemetry:");
foreach (var cityTelemetry in weatherCities.Telemetry)
{
    Console.WriteLine(cityTelemetry);
}

Console.WriteLine("\n\nResults:");
foreach (var city in weatherCities.TaskResults)
{
    Console.WriteLine($"{city.Name}:");
    if (city.Data is not null)
    {
        foreach (var forecast in city.Data as IEnumerable<WeatherForecast>)
        {
            Console.WriteLine(forecast);
        }
    }
    else
    {
        Console.WriteLine("No data");
    }
    Console.WriteLine();
}

Console.WriteLine(TimeProvider.System.GetLocalNow());
Console.WriteLine();
