# TaskListProcessor

[![License](https://img.shields.io/github/license/MarkHazleton/TaskListProcessor)](LICENSE)
[![Issues](https://img.shields.io/github/issues/MarkHazleton/TaskListProcessor)](../../issues)

> For more information, visit [MarkHazleton.com](https://MarkHazleton.com)

## Overview

The `TaskListProcessor` and `TaskListProcessorGeneric` classes are .NET utilities for efficient asynchronous task management. They enable concurrent execution, result tracking, and detailed telemetry, ideal for applications that need to handle multiple tasks simultaneously and aggregate results of different types.

## Features

- **Concurrent Task Execution**: Run multiple tasks in parallel.
- **Result Tracking**: Capture and store the results of tasks, including heterogeneous types.
- **Performance Telemetry**: Generate detailed telemetry for task performance analysis.
- **Error Logging**: Robust error handling and logging capabilities.

## Why I Wrote the Task List Processor

The inception of the Task List Processor was fueled by the desire to address a common yet under-discussed challenge in modern .NET development: efficiently managing multiple asynchronous operations that may have varying return types and distinct completion times. My journey in the .NET landscape has shown me that while asynchronous programming is powerful, it can also introduce complexity that necessitates a thoughtful approach to error handling, performance measurement, and resource management.

In working on projects that required fetching and processing data from various external services concurrently—such as building a travel site dashboard with real-time weather information for multiple cities—I realized the need for a tool that could handle these operations in a streamlined and error-resilient manner. This led to the creation of the Task List Processor, a class that embodies the principles of concurrent task management and robust logging.

Another motivation was to share knowledge and solutions with the community. The Task List Processor is not just a utility; it's an embodiment of best practices and design patterns in asynchronous .NET programming. It serves as an educational tool for other developers to learn from and build upon.

Lastly, I am a firm believer in lifelong learning and continuous improvement. Developing this project has been an avenue for me to explore new ideas, refine my skills, and contribute to the collective wisdom of the developer ecosystem. By open-sourcing the Task List Processor, I aim to invite collaboration, feedback, and innovation, keeping in stride with the ever-evolving field of technology.

## Real-World Example

Below is a real example from the repository, showing how to use `TaskListProcessorGeneric` to concurrently fetch weather and activity data for multiple cities, aggregate results, and display them by type:

```csharp
using CityThingsToDo;
using CityWeatherService;
using Microsoft.Extensions.Logging;
using TaskListProcessing;
using static CityWeatherService.WeatherService;

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
await TaskListProcessorGeneric.WhenAllWithLoggingAsync(tasks, logger);

// Display telemetry
foreach (var cityTelemetry in cityDashboards.Telemetry.OrderBy(o => o))
{
    Console.WriteLine(cityTelemetry);
}

// Display results by type
foreach (var city in cityDashboards.TaskResults.OrderBy(o => o.Name))
{
    Console.WriteLine($"{city.Name}:");
    if (city is TaskResult<IEnumerable<WeatherForecast>> cityWeather)
    {
        if (cityWeather.Data is null)
            Console.WriteLine("No weather data available");
        else
            foreach (var forecast in cityWeather.Data)
                Console.WriteLine(forecast.ToString());
    }
    else if (city is TaskResult<IEnumerable<Activity>> cityActivity)
    {
        if (cityActivity.Data is null)
            Console.WriteLine("No activity data available");
        else
            foreach (var activity in cityActivity.Data)
                Console.WriteLine(activity.ToString());
    }
}
```

This example demonstrates how you can:

- Launch multiple asynchronous operations of different types.
- Aggregate and process results in a type-safe way.
- Log telemetry and errors for all tasks.

## Getting Started

The Task List Processor project is designed to be simple to set up and run, even if you're new to .NET development. Below are the instructions to get you started with this project:

### Prerequisites

- [Git](https://git-scm.com/downloads)
- [.NET Latest SDK](https://dotnet.microsoft.com/download)

### Installation

1. **Clone the repository**

   ```sh
   git clone https://github.com/markhazleton/TaskListProcessor.git
   ```

2. **Navigate to the project directory**

   ```sh
   cd TaskListProcessor
   ```

3. **Build the project**

   ```sh
   dotnet build
   ```

   This will compile the project and prepare it for running.

### Running the Application

After building the project, you can run the console application using the .NET CLI:

```sh
dotnet run --project ./examples/TaskListProcessor.Console/TaskListProcessor.Console.csproj
```

### What to Expect

Once the application is running, it will:

- Simulate fetching weather data and activities for a predefined list of cities.
- Log the progress and results of each operation to the console.
- Demonstrate handling of asynchronous tasks, errors, and telemetry for multiple result types.

By following these instructions, you should have the Task List Processor up and running on your local machine, ready to be explored and expanded upon.

## Console Test Application Demonstration

The console test application (`examples/TaskListProcessor.Console/Program.cs`) provides a practical demonstration of the core features of the Task List Processor. When you run the console app, it will:

- **Fetch Weather and Activities**: For a predefined list of cities, the app concurrently fetches both weather forecasts and things-to-do activities using separate services.
- **Concurrent Task Execution**: All data retrieval operations are performed asynchronously and in parallel, showcasing efficient use of `TaskListProcessorGeneric` for managing multiple tasks.
- **Result Aggregation and Type Safety**: Results for each city are aggregated and handled in a type-safe manner, distinguishing between weather data and activity data.
- **Telemetry and Logging**: The application logs progress, errors, and detailed telemetry for each operation using the .NET logging framework. This includes start/end times, task completion status, and any exceptions encountered.
- **Error Handling**: Any errors during task execution are caught and logged, demonstrating robust error handling in asynchronous workflows.
- **Output of Results**: After all tasks complete, the app outputs telemetry data and displays the results for each city, grouped by data type (weather or activities).

This demonstration serves as a real-world example of how to use the Task List Processor for orchestrating and monitoring multiple asynchronous operations, making it easy to extend or adapt for your own applications.

## Contributing

Contributions are welcome! Please open an [issue](../../issues) or submit a pull request.

## Contact

For questions or support, please open an [issue](../../issues) in this repository.

## License

This project is licensed under the [MIT License](LICENSE).

---

> Powered by [MarkHazleton.com](https://MarkHazleton.com)
