// Program.cs - Enhanced TaskListProcessor Console Application
// Demonstrates advanced asynchronous task processing with impressive telemetry and clear output formatting

public class AppConfiguration
{
    public List<string> Cities { get; set; } = new() { "London", "Paris", "New York", "Tokyo", "Sydney", "Chicago", "Dallas", "Wichita" };
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan ShortTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
    public int MaxConcurrentTasks { get; set; } = 10;
    public int MaxResultsToShow { get; set; } = 3;
    public int MaxForecastDays { get; set; } = 5;
    public bool ShowDetailedTelemetry { get; set; } = true;
    public bool ShowIndividualResults { get; set; } = true;
    public bool RunDemoScenarios { get; set; } = true;
}
