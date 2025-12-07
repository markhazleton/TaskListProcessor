# Your First Real Processor

In this tutorial, you'll build a **production-ready travel dashboard** that aggregates data from multiple sources concurrently. By the end, you'll have a real application that demonstrates best practices and proper error handling.

**Time**: 30 minutes
**Difficulty**: Beginner
**Prerequisites**: Completed [5-Minute Quick Start](01-quick-start-5-minutes.md) and [Fundamentals](02-fundamentals.md)

---

## What We're Building

A **Travel Dashboard** that fetches data for multiple cities:
- Weather information
- Hotel listings
- Popular activities
- Restaurant recommendations

**The challenge**: Each data source is an independent API call. We want:
- ✅ All calls to run concurrently (fast)
- ✅ Graceful error handling (some APIs might fail)
- ✅ Progress reporting (show real-time status)
- ✅ Telemetry (track performance)
- ✅ Cancellation support (user can stop)

---

## Step 1: Create the Project

```bash
dotnet new console -n TravelDashboard
cd TravelDashboard
dotnet add reference path/to/TaskListProcessing/TaskListProcessing.csproj
```

---

## Step 2: Create Service Classes

We'll simulate API services with realistic delays and occasional errors.

### WeatherService.cs

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TravelDashboard.Services
{
    public class WeatherService
    {
        private readonly Random _random = new();

        public async Task<WeatherData> GetWeatherAsync(string city, CancellationToken ct)
        {
            // Simulate API call delay (200-800ms)
            await Task.Delay(_random.Next(200, 800), ct);

            // Simulate occasional API failures (10% chance)
            if (_random.Next(100) < 10)
            {
                throw new HttpRequestException($"Weather API unavailable for {city}");
            }

            // Return simulated weather data
            return new WeatherData
            {
                City = city,
                Temperature = _random.Next(50, 90),
                Condition = GetRandomCondition(),
                Humidity = _random.Next(30, 80)
            };
        }

        private string GetRandomCondition()
        {
            var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy", "Clear" };
            return conditions[_random.Next(conditions.Length)];
        }
    }

    public class WeatherData
    {
        public string City { get; set; } = "";
        public int Temperature { get; set; }
        public string Condition { get; set; } = "";
        public int Humidity { get; set; }

        public override string ToString() =>
            $"{Condition}, {Temperature}°F, Humidity: {Humidity}%";
    }
}
```

### HotelService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TravelDashboard.Services
{
    public class HotelService
    {
        private readonly Random _random = new();

        public async Task<List<Hotel>> GetHotelsAsync(string city, CancellationToken ct)
        {
            // Simulate API call delay
            await Task.Delay(_random.Next(300, 900), ct);

            // Simulate occasional failures
            if (_random.Next(100) < 10)
            {
                throw new TimeoutException($"Hotel API timed out for {city}");
            }

            // Generate random hotel data
            var count = _random.Next(3, 8);
            return Enumerable.Range(1, count)
                .Select(i => new Hotel
                {
                    Name = $"{city} Hotel {i}",
                    Rating = _random.Next(3, 6),
                    PricePerNight = _random.Next(100, 500)
                })
                .ToList();
        }
    }

    public class Hotel
    {
        public string Name { get; set; } = "";
        public int Rating { get; set; }
        public decimal PricePerNight { get; set; }

        public override string ToString() =>
            $"{Name} - {Rating}⭐ - ${PricePerNight}/night";
    }
}
```

### ActivityService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TravelDashboard.Services
{
    public class ActivityService
    {
        private readonly Random _random = new();
        private readonly Dictionary<string, List<string>> _cityActivities = new()
        {
            ["London"] = new() { "British Museum", "Tower of London", "London Eye", "Westminster Abbey" },
            ["Paris"] = new() { "Eiffel Tower", "Louvre Museum", "Arc de Triomphe", "Notre-Dame" },
            ["Tokyo"] = new() { "Tokyo Skytree", "Senso-ji Temple", "Meiji Shrine", "Shibuya Crossing" },
            ["New York"] = new() { "Statue of Liberty", "Central Park", "Empire State Building", "Times Square" }
        };

        public async Task<List<string>> GetActivitiesAsync(string city, CancellationToken ct)
        {
            // Simulate API call delay
            await Task.Delay(_random.Next(250, 750), ct);

            // Simulate occasional failures
            if (_random.Next(100) < 10)
            {
                throw new InvalidOperationException($"Activity API error for {city}");
            }

            // Return city-specific activities or generic ones
            if (_cityActivities.TryGetValue(city, out var activities))
            {
                return activities.Take(_random.Next(2, activities.Count + 1)).ToList();
            }

            return new List<string> { "City Tour", "Local Museum", "Shopping District" };
        }
    }
}
```

### RestaurantService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TravelDashboard.Services
{
    public class RestaurantService
    {
        private readonly Random _random = new();
        private readonly string[] _cuisines = { "Italian", "French", "Japanese", "Mexican", "American", "Chinese" };

        public async Task<List<Restaurant>> GetRestaurantsAsync(string city, CancellationToken ct)
        {
            // Simulate API call delay
            await Task.Delay(_random.Next(350, 850), ct);

            // Generate random restaurants
            var count = _random.Next(4, 9);
            return Enumerable.Range(1, count)
                .Select(i => new Restaurant
                {
                    Name = $"{_cuisines[_random.Next(_cuisines.Length)]} Restaurant {i}",
                    Rating = Math.Round(_random.NextDouble() * 2 + 3, 1), // 3.0 - 5.0
                    PriceLevel = _random.Next(1, 5)
                })
                .ToList();
        }
    }

    public class Restaurant
    {
        public string Name { get; set; } = "";
        public double Rating { get; set; }
        public int PriceLevel { get; set; }

        public override string ToString() =>
            $"{Name} - {Rating}⭐ - {new string('$', PriceLevel)}";
    }
}
```

---

## Step 3: Build the Dashboard

### Program.cs

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TravelDashboard.Services;

namespace TravelDashboard
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger<Program>();

            Console.WriteLine("╔═══════════════════════════════════════════╗");
            Console.WriteLine("║     Travel Dashboard - TaskListProcessor  ║");
            Console.WriteLine("╚═══════════════════════════════════════════╝\n");

            // Get cities from user
            Console.WriteLine("Enter cities to search (comma-separated):");
            Console.Write("Example: London, Paris, Tokyo, New York\n> ");
            var input = Console.ReadLine();

            var cities = input?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToArray() ?? new[] { "London", "Paris", "Tokyo" };

            if (cities.Length == 0)
            {
                cities = new[] { "London", "Paris", "Tokyo" };
            }

            Console.WriteLine($"\n🌍 Searching {cities.Length} cities...\n");

            // Create services
            var weatherService = new WeatherService();
            var hotelService = new HotelService();
            var activityService = new ActivityService();
            var restaurantService = new RestaurantService();

            // Create processor with options
            var options = new TaskListProcessorOptions
            {
                MaxConcurrentTasks = 12, // Allow good concurrency
                EnableDetailedTelemetry = true
            };

            using var processor = new TaskListProcessorEnhanced("TravelDashboard", logger, options);

            // Build task dictionary
            var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>();

            foreach (var city in cities)
            {
                // Capture city in local variable for closure
                var currentCity = city;

                tasks[$"{currentCity} - Weather"] = async ct =>
                    await weatherService.GetWeatherAsync(currentCity, ct);

                tasks[$"{currentCity} - Hotels"] = async ct =>
                    await hotelService.GetHotelsAsync(currentCity, ct);

                tasks[$"{currentCity} - Activities"] = async ct =>
                    await activityService.GetActivitiesAsync(currentCity, ct);

                tasks[$"{currentCity} - Restaurants"] = async ct =>
                    await restaurantService.GetRestaurantsAsync(currentCity, ct);
            }

            // Setup progress reporting
            var progress = new Progress<TaskProgress>(p =>
            {
                var percentage = p.PercentComplete;
                var completed = p.CompletedTasks;
                var total = p.TotalTasks;

                Console.Write($"\r⏳ Progress: {completed}/{total} ({percentage:F0}%) - {p.CurrentTask}     ");
            });

            // Setup cancellation (30 second timeout)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                // Execute all tasks
                await processor.ProcessTasksAsync(tasks, progress, cts.Token);
                Console.WriteLine("\n");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n\n⚠️  Operation timed out after 30 seconds\n");
            }

            // Display results
            DisplayResults(processor.TaskResults, cities);

            // Display telemetry
            DisplayTelemetry(processor);

            Console.WriteLine("\n✨ Dashboard complete! Press any key to exit...");
            Console.ReadKey();
        }

        static void DisplayResults(
            IEnumerable<EnhancedTaskResult<object>> results,
            string[] cities)
        {
            Console.WriteLine("╔═══════════════════════════════════════════╗");
            Console.WriteLine("║              TRAVEL RESULTS               ║");
            Console.WriteLine("╚═══════════════════════════════════════════╝\n");

            foreach (var city in cities)
            {
                Console.WriteLine($"📍 {city.ToUpper()}");
                Console.WriteLine(new string('─', 50));

                var cityResults = results
                    .Where(r => r.Name.StartsWith(city))
                    .OrderBy(r => r.Name);

                foreach (var result in cityResults)
                {
                    var category = result.Name.Split(" - ")[1];
                    var icon = GetCategoryIcon(category);

                    if (result.IsSuccessful)
                    {
                        Console.WriteLine($"{icon} {category}:");
                        DisplayResultData(result.Data, category);
                    }
                    else
                    {
                        Console.WriteLine($"❌ {category}: {result.ErrorMessage}");
                    }
                }

                Console.WriteLine();
            }
        }

        static void DisplayResultData(object? data, string category)
        {
            switch (category)
            {
                case "Weather":
                    if (data is WeatherData weather)
                        Console.WriteLine($"   {weather}");
                    break;

                case "Hotels":
                    if (data is List<Hotel> hotels)
                    {
                        var topHotels = hotels.OrderByDescending(h => h.Rating).Take(3);
                        foreach (var hotel in topHotels)
                            Console.WriteLine($"   • {hotel}");
                    }
                    break;

                case "Activities":
                    if (data is List<string> activities)
                    {
                        foreach (var activity in activities)
                            Console.WriteLine($"   • {activity}");
                    }
                    break;

                case "Restaurants":
                    if (data is List<Restaurant> restaurants)
                    {
                        var topRestaurants = restaurants.OrderByDescending(r => r.Rating).Take(3);
                        foreach (var restaurant in topRestaurants)
                            Console.WriteLine($"   • {restaurant}");
                    }
                    break;
            }
        }

        static string GetCategoryIcon(string category) => category switch
        {
            "Weather" => "🌤️",
            "Hotels" => "🏨",
            "Activities" => "🎯",
            "Restaurants" => "🍽️",
            _ => "📋"
        };

        static void DisplayTelemetry(TaskListProcessorEnhanced processor)
        {
            Console.WriteLine("╔═══════════════════════════════════════════╗");
            Console.WriteLine("║            PERFORMANCE METRICS            ║");
            Console.WriteLine("╚═══════════════════════════════════════════╝\n");

            var summary = processor.GetTelemetrySummary();

            Console.WriteLine($"📊 Total Tasks: {summary.TotalTasks}");
            Console.WriteLine($"✅ Successful: {summary.SuccessfulTasks} ({summary.SuccessRate:F1}%)");
            Console.WriteLine($"❌ Failed: {summary.FailedTasks}");
            Console.WriteLine($"⏱️  Average Time: {summary.AverageExecutionTime:F0}ms");
            Console.WriteLine($"🏃 Fastest: {summary.FastestTask?.DurationMs:F0}ms");
            Console.WriteLine($"🐌 Slowest: {summary.SlowestTask?.DurationMs:F0}ms");
            Console.WriteLine($"⏰ Total Time: {processor.Telemetry.Max(t => t.EndTime) - processor.Telemetry.Min(t => t.StartTime):g}");

            // Show category breakdown
            Console.WriteLine("\n📈 Breakdown by Category:");
            var byCategory = processor.TaskResults
                .GroupBy(r => r.Name.Split(" - ")[1])
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Count(),
                    Successful = g.Count(r => r.IsSuccessful),
                    AvgTime = g.Average(r => r.ExecutionTime.TotalMilliseconds)
                })
                .OrderBy(x => x.Category);

            foreach (var cat in byCategory)
            {
                var icon = GetCategoryIcon(cat.Category);
                var successRate = (cat.Successful / (double)cat.Total) * 100;
                Console.WriteLine($"   {icon} {cat.Category,-12} {cat.Successful}/{cat.Total} ({successRate:F0}%) - Avg: {cat.AvgTime:F0}ms");
            }
        }
    }
}
```

---

## Step 4: Run the Application

```bash
dotnet run
```

**Sample Output:**

```
╔═══════════════════════════════════════════╗
║     Travel Dashboard - TaskListProcessor  ║
╚═══════════════════════════════════════════╝

Enter cities to search (comma-separated):
Example: London, Paris, Tokyo, New York
> London, Paris, Tokyo

🌍 Searching 3 cities...

⏳ Progress: 12/12 (100%) - Tokyo - Restaurants

╔═══════════════════════════════════════════╗
║              TRAVEL RESULTS               ║
╚═══════════════════════════════════════════╝

📍 LONDON
──────────────────────────────────────────────────
🏨 Hotels:
   • London Hotel 3 - 5⭐ - $425/night
   • London Hotel 1 - 5⭐ - $380/night
   • London Hotel 2 - 4⭐ - $275/night
🌤️ Weather:
   Sunny, 75°F, Humidity: 45%
🎯 Activities:
   • British Museum
   • Tower of London
   • London Eye
🍽️ Restaurants:
   • Italian Restaurant 2 - 4.8⭐ - $$$
   • French Restaurant 1 - 4.6⭐ - $$$$
   • Japanese Restaurant 3 - 4.5⭐ - $$$

📍 PARIS
──────────────────────────────────────────────────
🏨 Hotels:
   • Paris Hotel 4 - 5⭐ - $450/night
   • Paris Hotel 2 - 4⭐ - $320/night
   • Paris Hotel 1 - 4⭐ - $285/night
❌ Weather: Weather API unavailable for Paris
🎯 Activities:
   • Eiffel Tower
   • Louvre Museum
   • Arc de Triomphe
🍽️ Restaurants:
   • French Restaurant 4 - 4.9⭐ - $$$$
   • Italian Restaurant 2 - 4.7⭐ - $$$
   • Mexican Restaurant 1 - 4.5⭐ - $$

📍 TOKYO
──────────────────────────────────────────────────
🏨 Hotels:
   • Tokyo Hotel 3 - 5⭐ - $395/night
   • Tokyo Hotel 5 - 5⭐ - $380/night
   • Tokyo Hotel 1 - 4⭐ - $240/night
🌤️ Weather:
   Clear, 68°F, Humidity: 55%
🎯 Activities:
   • Tokyo Skytree
   • Senso-ji Temple
   • Meiji Shrine
🍽️ Restaurants:
   • Japanese Restaurant 2 - 4.9⭐ - $$$$
   • Chinese Restaurant 4 - 4.7⭐ - $$$
   • Italian Restaurant 1 - 4.6⭐ - $$$

╔═══════════════════════════════════════════╗
║            PERFORMANCE METRICS            ║
╚═══════════════════════════════════════════╝

📊 Total Tasks: 12
✅ Successful: 11 (91.7%)
❌ Failed: 1
⏱️  Average Time: 528ms
🏃 Fastest: 245ms
🐌 Slowest: 848ms
⏰ Total Time: 00:00:00.9153421

📈 Breakdown by Category:
   🎯 Activities    3/3 (100%) - Avg: 485ms
   🏨 Hotels        3/3 (100%) - Avg: 612ms
   🍽️ Restaurants   3/3 (100%) - Avg: 574ms
   🌤️ Weather       2/3 (67%) - Avg: 441ms

✨ Dashboard complete! Press any key to exit...
```

---

## What We Accomplished

### ✅ Best Practices Demonstrated

1. **Concurrent Execution**
   - 12 tasks (4 per city × 3 cities)
   - All run concurrently (limited by MaxConcurrentTasks)
   - Sequential execution would take ~6 seconds
   - Concurrent execution completes in ~900ms

2. **Error Isolation**
   - Paris weather API failed
   - All other tasks completed successfully
   - Application remained functional with partial results

3. **Progress Reporting**
   - Real-time progress updates
   - User sees status as tasks complete

4. **Telemetry**
   - Detailed performance metrics
   - Success rate tracking
   - Category-level analysis

5. **Cancellation Support**
   - 30-second timeout configured
   - User can cancel anytime
   - Graceful shutdown

6. **Clean Code**
   - Service layer separation
   - Reusable components
   - Proper async/await
   - Resource disposal with `using`

---

## Key Learnings

### 1. Task Factory Pattern

```csharp
// ✅ Correct: Capture city in closure properly
var currentCity = city;
tasks[$"{currentCity} - Weather"] = async ct =>
    await weatherService.GetWeatherAsync(currentCity, ct);

// ❌ Wrong: Would use last city value for all
tasks[$"{city} - Weather"] = async ct =>
    await weatherService.GetWeatherAsync(city, ct);
```

### 2. Progress Reporting

```csharp
var progress = new Progress<TaskProgress>(p =>
{
    // Update UI on each task completion
    Console.Write($"\r⏳ Progress: {p.CompletedTasks}/{p.TotalTasks}");
});
```

### 3. Error Handling

```csharp
// No try/catch needed in task factories
tasks["Weather"] = async ct =>
    await weatherService.GetWeatherAsync(city, ct);

// TaskListProcessor handles exceptions automatically
// Check results after execution
if (!result.IsSuccessful)
{
    Console.WriteLine($"❌ {result.ErrorMessage}");
}
```

### 4. Telemetry Analysis

```csharp
// Access individual telemetry
foreach (var t in processor.Telemetry)
{
    Console.WriteLine($"{t.TaskName}: {t.DurationMs}ms");
}

// Or use summary
var summary = processor.GetTelemetrySummary();
Console.WriteLine($"Success: {summary.SuccessRate:F1}%");
```

---

## Exercises

Try these modifications to deepen your understanding:

### Exercise 1: Add Retry Logic

Modify services to retry failed operations:

```csharp
public async Task<WeatherData> GetWeatherAsync(string city, CancellationToken ct, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await FetchWeatherAsync(city, ct);
        }
        catch (HttpRequestException) when (i < maxRetries - 1)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), ct); // Exponential backoff
        }
    }
    throw new HttpRequestException($"Failed after {maxRetries} retries");
}
```

### Exercise 2: Add Task Dependencies

Make restaurants depend on successful hotel search:

```csharp
var definitions = new List<TaskDefinition>
{
    new() { Name = $"{city} - Hotels", Factory = hotelTask },
    new() {
        Name = $"{city} - Restaurants",
        Factory = restaurantTask,
        Dependencies = new[] { $"{city} - Hotels" }
    }
};

await processor.ProcessTaskDefinitionsAsync(definitions);
```

### Exercise 3: Add Caching

Cache weather results to reduce API calls:

```csharp
private readonly Dictionary<string, WeatherData> _cache = new();

public async Task<WeatherData> GetWeatherAsync(string city, CancellationToken ct)
{
    if (_cache.TryGetValue(city, out var cached))
        return cached;

    var data = await FetchWeatherAsync(city, ct);
    _cache[city] = data;
    return data;
}
```

---

## Production Enhancements

To make this production-ready, consider adding:

1. **Configuration**
   - API keys from configuration
   - Timeout values
   - Retry policies

2. **Real API Integration**
   - Replace mock services with actual HTTP clients
   - Handle rate limiting
   - Implement authentication

3. **Logging**
   - Structured logging with Serilog
   - Log levels for different scenarios
   - Correlation IDs for tracing

4. **Monitoring**
   - OpenTelemetry integration
   - Application Insights
   - Custom metrics

5. **Testing**
   - Unit tests for services
   - Integration tests for processor
   - Mock external dependencies

---

## Next Steps

You've built a real application! Now:

1. **Review**: Read [Common Pitfalls](04-common-pitfalls.md) to avoid mistakes
2. **Learn More**: Explore [Beginner Tutorials](../tutorials/beginner/)
3. **Go Deeper**: Try [Intermediate Tutorials](../tutorials/intermediate/)
4. **Build**: Create your own project!

---

**Congratulations!** 🎉 You've built your first production-ready TaskListProcessor application!

---

*Built with ❤️ by [Mark Hazleton](https://markhazleton.com)*
