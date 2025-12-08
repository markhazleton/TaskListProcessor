namespace TaskListProcessor.Web.Services;

/// <summary>
/// Service for managing SEO metadata across the application
/// </summary>
public class SeoMetadataService
{
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _siteName;
    private readonly string _twitterHandle;
    private readonly string _authorName;

    public SeoMetadataService(IConfiguration configuration)
    {
        _configuration = configuration;
        _baseUrl = configuration["SiteSettings:BaseUrl"] ?? "https://tasklistprocessor.com";
        _siteName = configuration["SiteSettings:SiteName"] ?? "TaskListProcessor";
        _twitterHandle = configuration["SiteSettings:TwitterHandle"] ?? "@markhazleton";
        _authorName = configuration["SiteSettings:AuthorName"] ?? "Mark Hazleton";
    }

    public SeoMetadata GetMetadataForPage(string pageName, string? customTitle = null, string? customDescription = null)
    {
        var metadata = pageName.ToLowerInvariant() switch
        {
            "index" or "home" => new SeoMetadata
            {
                Title = "Enterprise .NET 10 Task Processing Library | TaskListProcessor",
                Description = "TaskListProcessor - A modern, enterprise-grade .NET 10 library for orchestrating asynchronous operations with comprehensive telemetry, circuit breakers, and advanced scheduling capabilities. Build fault-tolerant distributed systems with ease.",
                Keywords = "TaskListProcessor, .NET 10, async task processing, enterprise library, circuit breaker pattern, telemetry, observability, OpenTelemetry, microservices, distributed systems, fault tolerance, task orchestration, C# library, async/await, performance optimization",
                CanonicalUrl = _baseUrl,
                ImageUrl = $"{_baseUrl}/images/tasklistprocessor-social.png",
                Type = "website",
                Author = _authorName
            },
            "demo" => new SeoMetadata
            {
                Title = "Interactive Demo - TaskListProcessor for .NET 10",
                Description = "Try TaskListProcessor's interactive demo showcasing real-time task processing, circuit breaker patterns, and comprehensive telemetry. Experience enterprise-grade asynchronous orchestration with live examples and streaming results.",
                Keywords = "TaskListProcessor demo, .NET task processing demo, interactive demo, live examples, async processing, circuit breaker demo, real-time telemetry, streaming demo",
                CanonicalUrl = $"{_baseUrl}/Home/Demo",
                ImageUrl = $"{_baseUrl}/images/demo-preview.png",
                Type = "article",
                Author = _authorName
            },
            "performance" => new SeoMetadata
            {
                Title = "Performance Benchmarks - TaskListProcessor .NET Library",
                Description = "Explore TaskListProcessor's performance metrics, benchmarks, and optimization techniques. High-throughput async processing with minimal overhead, efficient memory management, and production-ready scalability for enterprise applications.",
                Keywords = "TaskListProcessor performance, .NET performance, async performance, benchmarks, throughput, optimization, scalability, memory efficiency, high performance",
                CanonicalUrl = $"{_baseUrl}/Home/Performance",
                ImageUrl = $"{_baseUrl}/images/performance-preview.png",
                Type = "article",
                Author = _authorName
            },
            "architecture" => new SeoMetadata
            {
                Title = "System Architecture & Design Patterns - TaskListProcessor",
                Description = "Learn about TaskListProcessor's enterprise architecture, design patterns, and best practices. Discover circuit breaker implementation, telemetry integration, dependency injection, and microservices patterns for building resilient .NET applications.",
                Keywords = "TaskListProcessor architecture, system design, design patterns, circuit breaker pattern, microservices architecture, enterprise patterns, .NET architecture, dependency injection, SOLID principles",
                CanonicalUrl = $"{_baseUrl}/Home/Architecture",
                ImageUrl = $"{_baseUrl}/images/architecture-preview.png",
                Type = "article",
                Author = _authorName
            },
            "documentation" => new SeoMetadata
            {
                Title = "API Documentation & Developer Guide - TaskListProcessor",
                Description = "Complete API reference and developer documentation for TaskListProcessor. Learn how to integrate enterprise-grade task processing, implement fault tolerance, configure telemetry, and build production-ready .NET applications.",
                Keywords = "TaskListProcessor documentation, API reference, developer guide, .NET documentation, API docs, integration guide, code examples, tutorials",
                CanonicalUrl = $"{_baseUrl}/Home/Documentation",
                ImageUrl = $"{_baseUrl}/images/docs-preview.png",
                Type = "article",
                Author = _authorName
            },
            "docs" => new SeoMetadata
            {
                Title = "Documentation Browser - TaskListProcessor Tutorials & Guides",
                Description = "Browse comprehensive tutorials, guides, and documentation for TaskListProcessor. From getting started to advanced patterns, find everything you need to build enterprise-grade .NET applications with confidence.",
                Keywords = "TaskListProcessor tutorials, documentation browser, guides, examples, how-to, best practices, learning resources",
                CanonicalUrl = $"{_baseUrl}/Docs/Index",
                ImageUrl = $"{_baseUrl}/images/docs-browser-preview.png",
                Type = "article",
                Author = _authorName
            },
            _ => new SeoMetadata
            {
                Title = $"{pageName} - TaskListProcessor",
                Description = "TaskListProcessor - Enterprise-grade .NET 10 library for asynchronous task processing and orchestration.",
                Keywords = "TaskListProcessor, .NET, async, enterprise",
                CanonicalUrl = _baseUrl,
                ImageUrl = $"{_baseUrl}/images/tasklistprocessor-social.png",
                Type = "website",
                Author = _authorName
            }
        };

        // Override with custom values if provided
        if (!string.IsNullOrWhiteSpace(customTitle))
        {
            metadata.Title = customTitle;
        }

        if (!string.IsNullOrWhiteSpace(customDescription))
        {
            metadata.Description = customDescription;
        }

        // Set common properties
        metadata.SiteName = _siteName;
        metadata.TwitterHandle = _twitterHandle;

        return metadata;
    }

    public string GenerateStructuredData(string pageName)
    {
        var structuredData = pageName.ToLowerInvariant() switch
        {
            "index" or "home" => GenerateOrganizationSchema(),
            _ => GenerateSoftwareApplicationSchema()
        };

        return structuredData;
    }

    private string GenerateOrganizationSchema()
    {
        return @"{
            ""@context"": ""https://schema.org"",
            ""@type"": ""SoftwareApplication"",
            ""name"": ""TaskListProcessor"",
            ""applicationCategory"": ""DeveloperApplication"",
            ""operatingSystem"": ""Windows, Linux, macOS"",
            ""description"": ""Enterprise-grade .NET 10 library for orchestrating asynchronous operations with comprehensive telemetry, circuit breakers, and advanced scheduling capabilities."",
            ""offers"": {
                ""@type"": ""Offer"",
                ""price"": ""0"",
                ""priceCurrency"": ""USD""
            },
            ""author"": {
                ""@type"": ""Person"",
                ""name"": ""Mark Hazleton"",
                ""url"": ""https://markhazleton.com""
            },
            ""programmingLanguage"": ""C#"",
            ""runtimePlatform"": "".NET 10"",
            ""keywords"": ""task processing, asynchronous operations, circuit breaker, telemetry, enterprise library"",
            ""codeRepository"": ""https://github.com/markhazleton/TaskListProcessor"",
            ""license"": ""https://github.com/markhazleton/TaskListProcessor/blob/main/LICENSE""
        }";
    }

    private string GenerateSoftwareApplicationSchema()
    {
        return @"{
            ""@context"": ""https://schema.org"",
            ""@type"": ""SoftwareSourceCode"",
            ""name"": ""TaskListProcessor"",
            ""description"": ""Modern .NET 10 library for enterprise task processing"",
            ""programmingLanguage"": ""C#"",
            ""runtimePlatform"": "".NET 10"",
            ""codeRepository"": ""https://github.com/markhazleton/TaskListProcessor"",
            ""author"": {
                ""@type"": ""Person"",
                ""name"": ""Mark Hazleton""
            }
        }";
    }
}

public class SeoMetadata
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Type { get; set; } = "website";
    public string SiteName { get; set; } = string.Empty;
    public string TwitterHandle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}
