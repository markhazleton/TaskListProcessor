using CityThingsToDo;
using CityWeatherService;
using TaskListProcessor.Web.Services;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container with JSON configuration
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep property names as-is
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        // Add enum string conversion
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add Razor Pages support for Docs section
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Configure Razor Pages to use Views/Shared for layouts
        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
    });

// Add memory cache for markdown rendering
builder.Services.AddMemoryCache();

// Register our services
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<CityThingsToDoService>();
builder.Services.AddScoped<TaskProcessingService>();
builder.Services.AddSingleton<MarkdownService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages(); // Add Razor Pages routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
