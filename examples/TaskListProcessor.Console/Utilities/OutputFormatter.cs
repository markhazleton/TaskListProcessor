// Program.cs - Enhanced TaskListProcessor Console Application
// Demonstrates advanced asynchronous task processing with impressive telemetry and clear output formatting

public static class OutputFormatter
{
    private const string SectionSeparator = "===============================================================================";
    private const string SubSectionSeparator = "-------------------------------------------------------------------------------";

    // Safe characters that work in most console environments
    private const string SuccessIcon = "[OK]";
    private const string ErrorIcon = "[ERROR]";
    private const string WarningIcon = "[WARN]";
    private const string InfoIcon = "[INFO]";
    private const string RocketIcon = "[ROCKET]";
    private const string StatsIcon = "[STATS]";
    private const string CityIcon = "[CITY]";
    private const string ProcessIcon = "[PROC]";
    private const string TimeIcon = "[TIME]";

    public static void PrintHeader(string title, string subtitle = "")
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(SectionSeparator);
        Console.WriteLine($"  {RocketIcon} {title.ToUpper()}");
        if (!string.IsNullOrEmpty(subtitle))
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {subtitle}");
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(SectionSeparator);
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintSubHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(SubSectionSeparator);
        Console.WriteLine($"  {ProcessIcon} {title}");
        Console.WriteLine(SubSectionSeparator);
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{SuccessIcon} {message}");
        Console.ResetColor();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{ErrorIcon} {message}");
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{WarningIcon} {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{InfoIcon} {message}");
        Console.ResetColor();
    }
}
