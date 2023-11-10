using CityThingsToDo;
using CityWeatherService;
using Microsoft.Extensions.Logging;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

Console.WriteLine(TimeProvider.System.GetLocalNow());
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var thingsToDoService = new CityThingsToDoService();
var weatherService = new WeatherService();
var cityDashboards = new TaskListProcessing.TaskListProcessor();
var cities = new List<string> { "London", "Paris", "New York", "Tokyo", "Sydney", "Chicago", "Dallas", "Wichita" };
var tasks = new List<Task>();
foreach (var city in cities)
{
    tasks.Add(cityDashboards.GetTaskResultAsync($"{city} Weather", weatherService.GetWeather(city)));
    tasks.Add(cityDashboards.GetTaskResultAsync($"{city} Things To Do", thingsToDoService.GetThingsToDoAsync(city)));
}
await TaskListProcessing.TaskListProcessor.WhenAllWithLoggingAsync(tasks, logger);

Console.WriteLine("All tasks completed\n\n");
Console.WriteLine("Telemetry:");
foreach (var cityTelemetry in cityDashboards.Telemetry.OrderBy(o=>o))
{
    Console.WriteLine(cityTelemetry);
}

Console.WriteLine("\n\nResults:");
foreach (var city in cityDashboards.TaskResults.OrderBy(o=>o.Name))
{
    Console.WriteLine($"{city.Name}:");
    if (city.Data is not null)
    {
        if (city.Name.EndsWith("Weather"))
        {
            foreach (var forecast in city.Data as IEnumerable<WeatherForecast>)
            {
                Console.WriteLine(forecast);
            }
        }
        else if (city.Name.EndsWith("Things To Do"))
        { 
            foreach (var activity in city.Data as IEnumerable<Activity>)
            {
                Console.WriteLine(activity);
            }
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
