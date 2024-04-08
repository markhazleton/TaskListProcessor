namespace CityWeatherService;

/// <summary>
/// WeatherService
/// </summary>
public class WeatherService
{
    private readonly (string Summary, int MaxCTemp)[] Summaries = [
        ("Freezing", -20),
        ("Bracing", -10),
        ("Chilly", 0),
        ("Cool", 10),
        ("Mild", 20),
        ("Warm", 30),
        ("Balmy", 35),
        ("Hot", 40),
        ("Sweltering", 50),
        ("Scorching", 55)
    ];

    /// <summary>
    /// Asynchronously gets the weather forecast for a given city.
    /// </summary>
    /// <param name="city">The name of the city for which the weather forecast is required.</param>
    /// <returns>A task that represents the asynchronous operation and contains the weather forecast for the city.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the city argument is null.</exception>
    /// <exception cref="Exception">Thrown when there is a random failure in fetching the weather data.</exception>
    public async Task<IEnumerable<WeatherForecast>> GetWeather(string? city, bool AddRandomError = true)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentNullException(nameof(city), "City name cannot be null or empty.");
        }

        await Task.Delay(Random.Shared.Next(500, 3000)); // Simulate external service call latency

        if (AddRandomError)
        {
            // Introduce randomness for failure to simulate real-world scenarios
            if (Random.Shared.NextDouble() < 0.4) // 40% Chance of failure
            {
                throw new Exception("Random failure occurred fetching weather data.");
            }
        }

        var weather = Enumerable.Range(1, 5).Select(index =>
        {
            var temperatureC = Random.Shared.Next(-20, 55);
            return new WeatherForecast
            {
                City = city,
                Date = DateTime.Now.AddDays(index),
                TemperatureC = temperatureC,
                Summary = Summaries.FirstOrDefault(s => s.MaxCTemp >= temperatureC).Summary
            };
        })
        .ToArray();
        return weather;
    }

    /// <summary>
    /// Represents a weather forecast for a specific date and city.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// The date for which the forecast is valid.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The name of the city for the forecast.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// The forecasted temperature in degrees Celsius.
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// The forecasted temperature in degrees Fahrenheit.
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// A summary of the weather forecast.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current weather forecast.</returns>
        public override string ToString()
        {
            return $"City: {City}, Date: {Date:yyyy-MM-dd}, Temp (F): {TemperatureF}, Summary: {Summary}";
        }
    }
}
