using Microsoft.AspNetCore.Mvc;
using TaskListProcessor.Web.Models;
using TaskListProcessor.Web.Services;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskListProcessor.Web.Controllers;

public class HomeController : Controller
{
    private readonly TaskProcessingService _taskProcessingService;
    private readonly ILogger<HomeController> _logger;
    private readonly SeoMetadataService _seoMetadataService;
    private readonly SitemapService _sitemapService;

    public HomeController(
        TaskProcessingService taskProcessingService, 
        ILogger<HomeController> logger, 
        SeoMetadataService seoMetadataService,
        SitemapService sitemapService)
    {
        _taskProcessingService = taskProcessingService;
        _logger = logger;
        _seoMetadataService = seoMetadataService;
        _sitemapService = sitemapService;
    }

    public IActionResult Index()
    {
        SetSeoMetadata("home");
        
        var model = new ProcessingConfigurationViewModel
        {
            SelectedCities = new List<string> { "London", "Paris", "New York", "Tokyo" },
            MaxConcurrentTasks = 10,
            TimeoutMinutes = 5,
            EnableDetailedTelemetry = true,
            ShowIndividualResults = true,
            Scenario = ProcessingScenario.MainProcessing
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessTasks([FromBody] ProcessingConfigurationViewModel config)
    {
        try
        {
            // Log raw request body for debugging
            Request.EnableBuffering();
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            Request.Body.Position = 0;
            _logger.LogInformation("Raw request body: {Body}", body);

            // Try manual deserialization if model binding failed
            if (config == null && !string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Model binding failed, attempting manual deserialization");
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    config = JsonSerializer.Deserialize<ProcessingConfigurationViewModel>(body, options)!;
                    _logger.LogInformation("Manual deserialization result: {Config}", config == null ? "NULL" : "SUCCESS");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Manual deserialization failed");
                }
            }

            // Add comprehensive debugging and null checking
            _logger.LogInformation("ProcessTasks called with config: {Config}", config == null ? "NULL" : "NOT NULL");

            if (config == null)
            {
                _logger.LogError("Configuration is null - Model binding failed");
                return BadRequest("Configuration cannot be null - check JSON format");
            }

            _logger.LogInformation("Config.SelectedCities: {Cities}", config.SelectedCities == null ? "NULL" : $"Count={config.SelectedCities.Count}");
            _logger.LogInformation("Config.Scenario: {Scenario}", config.Scenario);

            if (_taskProcessingService == null)
            {
                _logger.LogError("TaskProcessingService is null");
                return StatusCode(500, new { error = "Service not available" });
            }

            if (config.SelectedCities == null || !config.SelectedCities.Any())
            {
                _logger.LogWarning("No cities selected");
                return BadRequest("At least one city must be selected");
            }

            _logger.LogInformation("About to process scenario: {Scenario}", config.Scenario);

            TaskProcessingResultViewModel result = config.Scenario switch
            {
                ProcessingScenario.MainProcessing => await _taskProcessingService.ProcessCityDataAsync(config),
                ProcessingScenario.IndividualTask => await _taskProcessingService.ProcessIndividualTaskAsync(
                    config.SelectedCities.FirstOrDefault() ?? "London"), // Use null-safe approach with fallback
                ProcessingScenario.CancellationDemo => await _taskProcessingService.ProcessCancellationDemoAsync(),
                _ => await _taskProcessingService.ProcessCityDataAsync(config)
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tasks for configuration: {@Config}", config);

            // Return a more detailed error response
            var errorResponse = new
            {
                error = "An error occurred while processing tasks",
                details = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray() // First 5 lines only
            };

            return StatusCode(500, errorResponse);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessStreamingTasks([FromBody] ProcessingConfigurationViewModel config)
    {
        if (config.SelectedCities == null || !config.SelectedCities.Any())
        {
            return BadRequest("At least one city must be selected");
        }

        _logger.LogInformation("Starting streaming processing for cities: {Cities}", string.Join(", ", config.SelectedCities));

        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";

        // Configure JSON serializer to handle infinity values
        var jsonOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            var resultCount = 0;
            await foreach (var result in _taskProcessingService.ProcessStreamingDemoAsync(config.SelectedCities))
            {
                resultCount++;
                _logger.LogInformation("Streaming result #{Count}: IsCompleted={IsCompleted}, TaskCount={TaskCount}",
                    resultCount, result.IsCompleted, result.DetailedTelemetry?.Count ?? 0);

                var json = System.Text.Json.JsonSerializer.Serialize(result, jsonOptions);

                // Send SSE event with explicit formatting
                await Response.WriteAsync($"id: {resultCount}\n");
                await Response.WriteAsync($"event: taskUpdate\n");
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();

                _logger.LogInformation("Sent SSE event #{Count}", resultCount);
            }

            _logger.LogInformation("Streaming completed. Total results sent: {Count}", resultCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming processing");
            var errorResult = new { error = "An error occurred during streaming", details = ex.Message };
            var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResult, jsonOptions);
            await Response.WriteAsync($"data: {errorJson}\n\n");
        }

        return new EmptyResult();
    }

    [HttpGet]
    public async Task<IActionResult> TestStreaming()
    {
        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";

        _logger.LogInformation("Starting test streaming");

        try
        {
            for (int i = 1; i <= 5; i++)
            {
                var testData = new { message = $"Test message {i}", timestamp = DateTime.Now };
                var json = System.Text.Json.JsonSerializer.Serialize(testData);

                await Response.WriteAsync($"id: {i}\n");
                await Response.WriteAsync($"event: test\n");
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();

                _logger.LogInformation("Sent test message {i}", i);
                await Task.Delay(1000); // 1 second delay
            }

            _logger.LogInformation("Test streaming completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test streaming");
        }

        return new EmptyResult();
    }

    [HttpGet]
    public async Task<IActionResult> StreamingDemo([FromQuery] string cities = "London,Paris")
    {
        var cityList = cities.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(c => c.Trim())
                             .ToList();

        if (!cityList.Any())
        {
            cityList = new List<string> { "London", "Paris" };
        }

        _logger.LogInformation("Starting GET streaming processing for cities: {Cities}", string.Join(", ", cityList));

        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";

        var jsonOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            var resultCount = 0;
            await foreach (var result in _taskProcessingService.ProcessStreamingDemoAsync(cityList))
            {
                resultCount++;
                _logger.LogInformation("GET Streaming result #{Count}: IsCompleted={IsCompleted}, TaskCount={TaskCount}",
                    resultCount, result.IsCompleted, result.DetailedTelemetry?.Count ?? 0);

                var json = System.Text.Json.JsonSerializer.Serialize(result, jsonOptions);

                await Response.WriteAsync($"id: {resultCount}\n");
                await Response.WriteAsync($"event: taskUpdate\n");
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();

                _logger.LogInformation("GET Sent SSE event #{Count}", resultCount);
            }

            _logger.LogInformation("GET Streaming completed. Total results sent: {Count}", resultCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET streaming processing");
            var errorResult = new { error = "An error occurred during streaming", details = ex.Message };
            var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResult, jsonOptions);
            await Response.WriteAsync($"data: {errorJson}\n\n");
        }

        return new EmptyResult();
    }

    public IActionResult Demo()
    {
        SetSeoMetadata("demo");
        return View();
    }

    public IActionResult Documentation()
    {
        SetSeoMetadata("documentation");
        return View();
    }

    public IActionResult Performance()
    {
        SetSeoMetadata("performance");
        return View();
    }

    public IActionResult Architecture()
    {
        SetSeoMetadata("architecture");
        return View();
    }

    private void SetSeoMetadata(string pageName)
    {
        var metadata = _seoMetadataService.GetMetadataForPage(pageName);
        
        ViewData["Title"] = metadata.Title;
        ViewData["Description"] = metadata.Description;
        ViewData["Keywords"] = metadata.Keywords;
        ViewData["CanonicalUrl"] = metadata.CanonicalUrl;
        ViewData["OgImage"] = metadata.ImageUrl;
        ViewData["OgType"] = metadata.Type;
        ViewData["StructuredData"] = _seoMetadataService.GenerateStructuredData(pageName);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("sitemap.xml")]
    [ResponseCache(Duration = 3600)] // Cache for 1 hour
    public IActionResult Sitemap()
    {
        try
        {
            var sitemap = _sitemapService.GenerateSitemap();
            return Content(sitemap, "application/xml", System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sitemap");
            return StatusCode(500, "Error generating sitemap");
        }
    }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
