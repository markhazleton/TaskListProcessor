using Microsoft.AspNetCore.Mvc;

namespace TaskListProcessor.Web.ViewComponents;

/// <summary>
/// View component for rendering structured data (Schema.org) markup
/// </summary>
public class StructuredDataViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string pageType = "website")
    {
        var structuredData = pageType.ToLowerInvariant() switch
        {
            "home" or "index" => GetSoftwareApplicationSchema(),
            "article" => GetArticleSchema(),
            "documentation" => GetTechArticleSchema(),
            _ => GetWebPageSchema()
        };

        return Content(structuredData);
    }

    private string GetSoftwareApplicationSchema()
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
                ""url"": ""https://markhazleton.com"",
                ""sameAs"": [
                    ""https://github.com/markhazleton"",
                    ""https://twitter.com/markhazleton""
                ]
            },
            ""programmingLanguage"": {
                ""@type"": ""ComputerLanguage"",
                ""name"": ""C#"",
                ""url"": ""https://docs.microsoft.com/en-us/dotnet/csharp/""
            },
            ""runtimePlatform"": "".NET 10"",
            ""keywords"": ""task processing, asynchronous operations, circuit breaker, telemetry, enterprise library, microservices, distributed systems"",
            ""codeRepository"": ""https://github.com/markhazleton/TaskListProcessor"",
            ""license"": ""https://github.com/markhazleton/TaskListProcessor/blob/main/LICENSE"",
            ""softwareVersion"": ""2.0"",
            ""aggregateRating"": {
                ""@type"": ""AggregateRating"",
                ""ratingValue"": ""5"",
                ""ratingCount"": ""1""
            }
        }";
    }

    private string GetArticleSchema()
    {
        return @"{
            ""@context"": ""https://schema.org"",
            ""@type"": ""Article"",
            ""headline"": ""TaskListProcessor - Enterprise .NET Task Processing"",
            ""author"": {
                ""@type"": ""Person"",
                ""name"": ""Mark Hazleton"",
                ""url"": ""https://markhazleton.com""
            },
            ""publisher"": {
                ""@type"": ""Person"",
                ""name"": ""Mark Hazleton""
            },
            ""datePublished"": ""2024-01-01"",
            ""dateModified"": """ + DateTime.UtcNow.ToString("yyyy-MM-dd") + @"""
        }";
    }

    private string GetTechArticleSchema()
    {
        return @"{
            ""@context"": ""https://schema.org"",
            ""@type"": ""TechArticle"",
            ""headline"": ""TaskListProcessor Documentation"",
            ""author"": {
                ""@type"": ""Person"",
                ""name"": ""Mark Hazleton""
            },
            ""proficiencyLevel"": ""Expert"",
            ""dependencies"": "".NET 10""
        }";
    }

    private string GetWebPageSchema()
    {
        return @"{
            ""@context"": ""https://schema.org"",
            ""@type"": ""WebPage"",
            ""name"": ""TaskListProcessor"",
            ""description"": ""Enterprise-grade .NET 10 library for asynchronous task processing""
        }";
    }
}
