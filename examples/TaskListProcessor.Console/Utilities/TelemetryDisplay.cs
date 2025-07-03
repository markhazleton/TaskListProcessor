// Program.cs - Enhanced TaskListProcessor Console Application
// Demonstrates advanced asynchronous task processing with impressive telemetry and clear output formatting

using TaskListProcessing;

// Enhanced telemetry display class
public static class TelemetryDisplay
{
    public static void ShowImpressiveTelemetry(TaskListProcessorImproved processor, string scenarioName)
    {
        var summary = processor.GetTelemetrySummary();
        var telemetryData = processor.Telemetry.ToList();

        OutputFormatter.PrintSubHeader($"[STATS] TELEMETRY DASHBOARD - {scenarioName}");

        // Performance Overview Box
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine("|                            [ROCKET] PERFORMANCE OVERVIEW                       |");
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine($"|  Total Tasks: {summary.TotalTasks,-10} | Success Rate: {summary.SuccessRate,-8:F1}% | Total Time: {summary.TotalExecutionTime,-10:N0}ms |");
        Console.WriteLine($"|  Successful:  {summary.SuccessfulTasks,-10} | Failed:       {summary.FailedTasks,-10} | Avg Time:   {summary.AverageExecutionTime,-10:F0}ms |");
        Console.WriteLine($"|  Fastest:     {summary.MinExecutionTime,-10}ms | Slowest:      {summary.MaxExecutionTime,-10}ms | Throughput: {summary.TotalTasks / (summary.TotalExecutionTime / 1000.0),-10:F1}/s |");
        Console.WriteLine("+---------------------------------------------------------------------------------+");

        // Performance Distribution
        ShowPerformanceDistribution(telemetryData);

        // Success/Failure Breakdown
        ShowSuccessFailureBreakdown(telemetryData);

        // Performance Ranking
        ShowPerformanceRanking(telemetryData);
    }

    private static void ShowPerformanceDistribution(List<TaskTelemetry> telemetryData)
    {
        Console.WriteLine();
        Console.WriteLine("[CHART] PERFORMANCE DISTRIBUTION");
        Console.WriteLine("+------------------------------------------------------------------------------+");

        var successfulTasks = telemetryData.Where(t => t.IsSuccessful).ToList();
        if (successfulTasks.Any())
        {
            var ultraFast = successfulTasks.Count(t => t.ElapsedMilliseconds < 200);
            var fast = successfulTasks.Count(t => t.ElapsedMilliseconds >= 200 && t.ElapsedMilliseconds < 500);
            var normal = successfulTasks.Count(t => t.ElapsedMilliseconds >= 500 && t.ElapsedMilliseconds < 1000);
            var slow = successfulTasks.Count(t => t.ElapsedMilliseconds >= 1000);

            Console.WriteLine($"|  [FAST] Ultra Fast (< 200ms):   {ultraFast,-3} tasks {CreateBar(ultraFast, successfulTasks.Count),-20} |");
            Console.WriteLine($"|  [GOOD] Fast (200-500ms):       {fast,-3} tasks {CreateBar(fast, successfulTasks.Count),-20} |");
            Console.WriteLine($"|  [OK]   Normal (500-1000ms):    {normal,-3} tasks {CreateBar(normal, successfulTasks.Count),-20} |");
            Console.WriteLine($"|  [SLOW] Slow (> 1000ms):        {slow,-3} tasks {CreateBar(slow, successfulTasks.Count),-20} |");
        }

        Console.WriteLine("+------------------------------------------------------------------------------+");
    }

    private static void ShowSuccessFailureBreakdown(List<TaskTelemetry> telemetryData)
    {
        Console.WriteLine();
        Console.WriteLine("[RESULTS] SUCCESS/FAILURE BREAKDOWN");
        Console.WriteLine("+------------------------------------------------------------------------------+");

        var successful = telemetryData.Where(t => t.IsSuccessful).ToList();
        var failed = telemetryData.Where(t => !t.IsSuccessful).ToList();

        if (successful.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|  [OK] SUCCESSFUL TASKS: {successful.Count}                                              |");
            Console.ResetColor();

            foreach (var task in successful.OrderBy(t => t.ElapsedMilliseconds))
            {
                var icon = GetPerformanceIcon(task.ElapsedMilliseconds);
                Console.WriteLine($"|    {icon} {task.TaskName,-50} {task.ElapsedMilliseconds,6}ms |");
            }
        }

        if (failed.Any())
        {
            Console.WriteLine("|                                                                              |");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"|  [FAIL] FAILED TASKS: {failed.Count}                                                   |");
            Console.ResetColor();

            foreach (var task in failed)
            {
                Console.WriteLine($"|    [ERR] {task.TaskName,-40} {task.ErrorType,-15} |");
                Console.WriteLine($"|       Error: {task.ErrorMessage,-60} |");
            }
        }

        Console.WriteLine("+------------------------------------------------------------------------------+");
    }

    private static void ShowPerformanceRanking(List<TaskTelemetry> telemetryData)
    {
        var successful = telemetryData.Where(t => t.IsSuccessful).OrderBy(t => t.ElapsedMilliseconds).ToList();
        if (!successful.Any()) return;

        Console.WriteLine();
        Console.WriteLine("[RANKING] PERFORMANCE LEADERBOARD");
        Console.WriteLine("+------------------------------------------------------------------------------+");
        Console.WriteLine("|  Rank | Task Name                                    | Time    | Performance |");
        Console.WriteLine("+------------------------------------------------------------------------------+");

        for (int i = 0; i < Math.Min(successful.Count, 10); i++)
        {
            var task = successful[i];
            var medal = i switch
            {
                0 => "[1st]",
                1 => "[2nd]",
                2 => "[3rd]",
                _ => $"[{i + 1,2}.]"
            };
            var performance = GetPerformanceIcon(task.ElapsedMilliseconds);
            Console.WriteLine($"|  {medal} | {task.TaskName,-44} | {task.ElapsedMilliseconds,6}ms | {performance,-11} |");
        }

        Console.WriteLine("+------------------------------------------------------------------------------+");
    }

    private static string CreateBar(int value, int total, int maxLength = 20)
    {
        if (total == 0) return new string(' ', maxLength);
        var filled = (int)((double)value / total * maxLength);
        return new string('#', filled) + new string('-', maxLength - filled);
    }

    private static string GetPerformanceIcon(long milliseconds)
    {
        return milliseconds switch
        {
            < 200 => "[FAST]",
            < 500 => "[GOOD]",
            < 1000 => "[OK]",
            < 2000 => "[SLOW]",
            _ => "[CRITICAL]"
        };
    }
}
