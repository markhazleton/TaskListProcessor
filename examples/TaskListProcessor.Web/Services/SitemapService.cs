using System.Text;
using System.Xml.Linq;

namespace TaskListProcessor.Web.Services;

/// <summary>
/// Service for generating XML sitemaps for search engine optimization
/// </summary>
public class SitemapService
{
    private readonly ILogger<SitemapService> _logger;
    private readonly string _baseUrl;

    public SitemapService(ILogger<SitemapService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _baseUrl = configuration["SiteSettings:BaseUrl"] ?? "https://tasklistprocessor.com";
    }

    /// <summary>
    /// Generate XML sitemap with all site URLs
    /// </summary>
    public string GenerateSitemap()
    {
        try
        {
            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    GetSitemapUrls().Select(url => CreateUrlElement(url))
                )
            );

            var stringBuilder = new StringBuilder();
            using (var writer = System.Xml.XmlWriter.Create(stringBuilder, new System.Xml.XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            }))
            {
                sitemap.Save(writer);
            }

            return stringBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sitemap");
            throw;
        }
    }

    private IEnumerable<SitemapUrl> GetSitemapUrls()
    {
        var now = DateTime.UtcNow;

        return new List<SitemapUrl>
        {
            // Home page - highest priority
            new SitemapUrl
            {
                Url = _baseUrl,
                LastModified = now,
                ChangeFrequency = ChangeFrequency.Weekly,
                Priority = 1.0
            },
            // Main pages
            new SitemapUrl
            {
                Url = $"{_baseUrl}/Home/Demo",
                LastModified = now,
                ChangeFrequency = ChangeFrequency.Weekly,
                Priority = 0.9
            },
            new SitemapUrl
            {
                Url = $"{_baseUrl}/Home/Performance",
                LastModified = now,
                ChangeFrequency = ChangeFrequency.Monthly,
                Priority = 0.8
            },
            new SitemapUrl
            {
                Url = $"{_baseUrl}/Home/Architecture",
                LastModified = now,
                ChangeFrequency = ChangeFrequency.Monthly,
                Priority = 0.8
            },
            new SitemapUrl
            {
                Url = $"{_baseUrl}/Home/Documentation",
                LastModified = now,
                ChangeFrequency = ChangeFrequency.Weekly,
                Priority = 0.9
            },
            new SitemapUrl
            {
                Url = $"{_baseUrl}/Docs/Index",
                LastModified = now,
                ChangeFrequency = ChangeFrequency.Weekly,
                Priority = 0.9
            }
        };
    }

    private XElement CreateUrlElement(SitemapUrl url)
    {
        var element = new XElement("url",
            new XElement("loc", url.Url),
            new XElement("lastmod", url.LastModified.ToString("yyyy-MM-dd")),
            new XElement("changefreq", url.ChangeFrequency.ToString().ToLowerInvariant()),
            new XElement("priority", url.Priority.ToString("F1"))
        );

        return element;
    }
}

public class SitemapUrl
{
    public string Url { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public ChangeFrequency ChangeFrequency { get; set; }
    public double Priority { get; set; }
}

public enum ChangeFrequency
{
    Always,
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Yearly,
    Never
}
