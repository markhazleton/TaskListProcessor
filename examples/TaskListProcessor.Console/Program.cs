using CityThingsToDo;
using CityWeatherService;
using Microsoft.Extensions.Logging;
using TaskListProcessing;
using static CityThingsToDo.CityThingsToDoService;
using static CityWeatherService.WeatherService;

Console.WriteLine(TimeProvider.System.GetLocalNow());
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var thingsToDoService = new CityThingsToDoService();
var weatherService = new WeatherService();
var cityDashboards = new TaskListProcessorGeneric();
var cities = new List<string> { "London", "Paris", "New York", "Tokyo", "Sydney", "Chicago", "Dallas", "Wichita" };
var tasks = new List<Task>();
foreach (var city in cities)
{
    tasks.Add(cityDashboards.GetTaskResultAsync($"{city} Weather", weatherService.GetWeather(city)));
    tasks.Add(cityDashboards.GetTaskResultAsync($"{city} Things To Do", thingsToDoService.GetThingsToDoAsync(city)));
}
await cityDashboards.WhenAllWithLoggingAsync(tasks, logger);

Console.WriteLine("All tasks completed\n\n");
Console.WriteLine("Telemetry:");
foreach (var cityTelemetry in cityDashboards.Telemetry.OrderBy(o => o))
{
    Console.WriteLine(cityTelemetry);
}

Console.WriteLine("\n\nResults:");
foreach (var city in cityDashboards.TaskResults.OrderBy(o => o.Name))
{
    Console.WriteLine($"{city.Name}:");

    if (city is TaskResult<IEnumerable<WeatherForecast>> cityWeather)
    {
        if (cityWeather.Data is null)
        {
            Console.WriteLine("No weather data available");
            continue;
        }
        else
        {
            foreach (var forecast in cityWeather.Data as IEnumerable<WeatherForecast>)
            {
                Console.WriteLine(forecast.ToString());
            }
        }
    }
    else if (city is TaskResult<IEnumerable<Activity>> cityActivity)
    {
        if (cityActivity.Data is null)
        {
            Console.WriteLine("No activity data available");
        }
        else
        {
            foreach (var activity in cityActivity.Data as IEnumerable<Activity>)
            {
                Console.WriteLine(activity.ToString());
            }
        }
    }
}

Console.WriteLine(TimeProvider.System.GetLocalNow());
Console.WriteLine();
