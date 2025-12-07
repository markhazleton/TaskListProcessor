using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using TaskListProcessor.Web.Models;

namespace TaskListProcessor.Web.Services;

/// <summary>
/// Service for parsing and rendering markdown documents
/// </summary>
public class MarkdownService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MarkdownService> _logger;
    private readonly string _docsPath;
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService(
        IMemoryCache cache,
        ILogger<MarkdownService> logger,
        IWebHostEnvironment environment)
    {
        _cache = cache;
        _logger = logger;
        _docsPath = Path.Combine(environment.ContentRootPath, "..", "..", "docs");
        
        // Configure Markdig pipeline with extensions
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions() // Tables, task lists, etc.
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub) // Auto-generate heading IDs
            .UseDiagrams() // Mermaid diagram support
            .UseEmphasisExtras() // Strikethrough, subscript, superscript
            .UseBootstrap() // Bootstrap CSS classes
            .Build();
    }

    /// <summary>
    /// Render markdown file to HTML
    /// </summary>
    public async Task<string> RenderMarkdownAsync(string relativePath)
    {
        var cacheKey = $"md:content:{relativePath}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedHtml))
        {
            _logger.LogDebug("Serving cached markdown: {Path}", relativePath);
            return cachedHtml!;
        }

        var fullPath = GetFullPath(relativePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Markdown file not found: {relativePath}");
        }

        var markdown = await File.ReadAllTextAsync(fullPath);
        var html = Markdown.ToHtml(markdown, _pipeline);

        // Cache for 1 hour
        _cache.Set(cacheKey, html, TimeSpan.FromHours(1));

        _logger.LogInformation("Rendered markdown: {Path}", relativePath);
        return html;
    }

    /// <summary>
    /// Get metadata for a markdown document
    /// </summary>
    public async Task<DocumentMetadata> GetDocumentMetadataAsync(string relativePath)
    {
        var cacheKey = $"md:metadata:{relativePath}";
        
        if (_cache.TryGetValue(cacheKey, out DocumentMetadata? cachedMetadata))
        {
            return cachedMetadata!;
        }

        var fullPath = GetFullPath(relativePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Markdown file not found: {relativePath}");
        }

        var fileInfo = new FileInfo(fullPath);
        var markdown = await File.ReadAllTextAsync(fullPath);
        var doc = Markdown.Parse(markdown, _pipeline);

        // Extract title from first H1 or filename
        var title = ExtractTitle(doc) ?? Path.GetFileNameWithoutExtension(relativePath);
        
        // Extract description from first paragraph
        var description = ExtractDescription(doc);

        // Parse path components
        var pathParts = relativePath.Replace("\\", "/").Split('/');
        var category = pathParts.Length > 0 ? pathParts[0] : "docs";
        var subcategory = pathParts.Length > 1 && !pathParts[1].EndsWith(".md") ? pathParts[1] : null;

        // Calculate reading time (avg 200 words per minute)
        var wordCount = CountWords(markdown);
        var readingTime = (int)Math.Ceiling(wordCount / 200.0);

        // Determine if this is a tutorial
        var isTutorial = relativePath.Contains("tutorials", StringComparison.OrdinalIgnoreCase);
        var tutorialLevel = subcategory;

        // Extract tags from content
        var tags = ExtractTags(markdown, category, subcategory);

        var metadata = new DocumentMetadata
        {
            Title = title,
            Path = relativePath,
            Category = category,
            Subcategory = subcategory,
            Description = description,
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime,
            ReadingTimeMinutes = readingTime,
            IsTutorial = isTutorial,
            TutorialLevel = tutorialLevel,
            Tags = tags
        };

        // Cache for 1 hour
        _cache.Set(cacheKey, metadata, TimeSpan.FromHours(1));

        return metadata;
    }

    /// <summary>
    /// Get all markdown documents in the docs directory
    /// </summary>
    public async Task<List<DocumentMetadata>> GetAllDocumentsAsync()
    {
        var cacheKey = "md:all-documents";
        
        if (_cache.TryGetValue(cacheKey, out List<DocumentMetadata>? cachedList))
        {
            return cachedList!;
        }

        var documents = new List<DocumentMetadata>();

        if (!Directory.Exists(_docsPath))
        {
            _logger.LogWarning("Docs directory not found: {Path}", _docsPath);
            return documents;
        }

        var mdFiles = Directory.GetFiles(_docsPath, "*.md", SearchOption.AllDirectories);

        foreach (var file in mdFiles)
        {
            try
            {
                var relativePath = Path.GetRelativePath(_docsPath, file);
                var metadata = await GetDocumentMetadataAsync(relativePath);
                documents.Add(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing document: {File}", file);
            }
        }

        // Cache for 1 hour
        _cache.Set(cacheKey, documents, TimeSpan.FromHours(1));

        _logger.LogInformation("Loaded {Count} documents", documents.Count);
        return documents;
    }

    /// <summary>
    /// Generate table of contents from markdown document
    /// </summary>
    public List<TocItem> GenerateTableOfContents(string markdown)
    {
        var doc = Markdown.Parse(markdown, _pipeline);
        var toc = new List<TocItem>();

        foreach (var heading in doc.Descendants<HeadingBlock>())
        {
            // Skip H1 (title) and only include H2-H4
            if (heading.Level < 2 || heading.Level > 4)
                continue;

            var text = GetHeadingText(heading);
            var id = GenerateId(text);

            toc.Add(new TocItem
            {
                Text = text,
                Level = heading.Level,
                Id = id
            });
        }

        return toc;
    }

    /// <summary>
    /// Build document tree for navigation
    /// </summary>
    public async Task<List<DocumentTreeNode>> BuildDocumentTreeAsync()
    {
        var cacheKey = "md:document-tree";
        
        if (_cache.TryGetValue(cacheKey, out List<DocumentTreeNode>? cachedTree))
        {
            return cachedTree!;
        }

        var documents = await GetAllDocumentsAsync();
        var tree = new List<DocumentTreeNode>();

        // Group by category
        var categories = documents.GroupBy(d => d.Category).OrderBy(g => GetCategoryOrder(g.Key));

        foreach (var category in categories)
        {
            var categoryNode = new DocumentTreeNode
            {
                Name = FormatCategoryName(category.Key),
                IsDirectory = true,
                Icon = GetCategoryIcon(category.Key),
                Children = new List<DocumentTreeNode>()
            };

            // Check for subcategories
            var subcategories = category.GroupBy(d => d.Subcategory ?? "");

            if (subcategories.Count() == 1 && string.IsNullOrEmpty(subcategories.First().Key))
            {
                // No subcategories, add documents directly
                foreach (var doc in category.OrderBy(d => d.Path))
                {
                    categoryNode.Children.Add(CreateDocumentNode(doc));
                }
            }
            else
            {
                // Has subcategories
                foreach (var subcategory in subcategories.OrderBy(g => g.Key))
                {
                    if (string.IsNullOrEmpty(subcategory.Key))
                    {
                        // Documents without subcategory go directly under category
                        foreach (var doc in subcategory.OrderBy(d => d.Path))
                        {
                            categoryNode.Children.Add(CreateDocumentNode(doc));
                        }
                    }
                    else
                    {
                        // Create subcategory node
                        var subcategoryNode = new DocumentTreeNode
                        {
                            Name = FormatCategoryName(subcategory.Key),
                            IsDirectory = true,
                            Icon = "bi-folder",
                            Children = new List<DocumentTreeNode>()
                        };

                        // Add badge for tutorial completion
                        if (subcategory.First().IsTutorial)
                        {
                            var total = subcategory.Count();
                            var completed = subcategory.Key.Equals("beginner", StringComparison.OrdinalIgnoreCase) ? 5 : 0;
                            subcategoryNode.Badge = $"{completed}/{total}";
                            subcategoryNode.BadgeClass = completed == total ? "bg-success" : "bg-warning";
                        }

                        foreach (var doc in subcategory.OrderBy(d => d.Path))
                        {
                            subcategoryNode.Children.Add(CreateDocumentNode(doc));
                        }

                        categoryNode.Children.Add(subcategoryNode);
                    }
                }
            }

            tree.Add(categoryNode);
        }

        // Cache for 1 hour
        _cache.Set(cacheKey, tree, TimeSpan.FromHours(1));

        return tree;
    }

    /// <summary>
    /// Search documents
    /// </summary>
    public async Task<List<DocumentMetadata>> SearchDocumentsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<DocumentMetadata>();

        var allDocuments = await GetAllDocumentsAsync();
        var searchTerms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return allDocuments
            .Where(doc =>
            {
                var searchText = $"{doc.Title} {doc.Description} {string.Join(" ", doc.Tags)}".ToLowerInvariant();
                return searchTerms.All(term => searchText.Contains(term));
            })
            .OrderByDescending(doc => CalculateRelevance(doc, searchTerms))
            .ToList();
    }

    // Helper methods

    private string GetFullPath(string relativePath)
    {
        var fullPath = Path.Combine(_docsPath, relativePath);
        
        // Security check: ensure the path is within docs directory
        var normalizedPath = Path.GetFullPath(fullPath);
        var normalizedDocsPath = Path.GetFullPath(_docsPath);
        
        if (!normalizedPath.StartsWith(normalizedDocsPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Path traversal detected");
        }

        return fullPath;
    }

    private string? ExtractTitle(MarkdownDocument doc)
    {
        var h1 = doc.Descendants<HeadingBlock>().FirstOrDefault(h => h.Level == 1);
        return h1 != null ? GetHeadingText(h1) : null;
    }

    private string? ExtractDescription(MarkdownDocument doc)
    {
        var paragraph = doc.Descendants<ParagraphBlock>().FirstOrDefault();
        if (paragraph == null) return null;

        var text = GetBlockText(paragraph);
        return text.Length > 200 ? text.Substring(0, 197) + "..." : text;
    }

    private string GetHeadingText(HeadingBlock heading)
    {
        // Use Markdig's built-in method to extract plain text from inline elements
        var inlineContent = heading.Inline;
        if (inlineContent == null)
            return string.Empty;

        var text = new System.Text.StringBuilder();
        foreach (var inline in inlineContent)
        {
            ExtractTextFromInline(inline, text);
        }
        
        return text.ToString().Trim();
    }

    private void ExtractTextFromInline(Markdig.Syntax.Inlines.Inline inline, System.Text.StringBuilder text)
    {
        switch (inline)
        {
            case LiteralInline literal:
                text.Append(literal.Content.ToString());
                break;
            case Markdig.Syntax.Inlines.CodeInline code:
                text.Append(code.Content);
                break;
            case Markdig.Syntax.Inlines.EmphasisInline emphasis:
                foreach (var child in emphasis)
                {
                    ExtractTextFromInline(child, text);
                }
                break;
            case Markdig.Syntax.Inlines.LinkInline link:
                foreach (var child in link)
                {
                    ExtractTextFromInline(child, text);
                }
                break;
            case Markdig.Syntax.Inlines.ContainerInline container:
                foreach (var child in container)
                {
                    ExtractTextFromInline(child, text);
                }
                break;
        }
    }

    private string GetBlockText(Block block)
    {
        var text = new System.Text.StringBuilder();
        
        if (block is LeafBlock leafBlock && leafBlock.Inline != null)
        {
            foreach (var inline in leafBlock.Inline)
            {
                ExtractTextFromInline(inline, text);
            }
        }
        
        return text.ToString().Trim();
    }

    private string GenerateId(string text)
    {
        return Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
    }

    private int CountWords(string text)
    {
        return Regex.Matches(text, @"\b\w+\b").Count;
    }

    private List<string> ExtractTags(string markdown, string category, string? subcategory)
    {
        var tags = new List<string> { category };
        
        if (!string.IsNullOrEmpty(subcategory))
            tags.Add(subcategory);

        // Extract common keywords
        var keywords = new[] { "async", "await", "task", "parallel", "concurrent", "telemetry", 
            "circuit breaker", "performance", "testing", "error handling", "dependency injection" };

        foreach (var keyword in keywords)
        {
            if (markdown.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                tags.Add(keyword);
        }

        return tags.Distinct().ToList();
    }

    private DocumentTreeNode CreateDocumentNode(DocumentMetadata doc)
    {
        return new DocumentTreeNode
        {
            Name = doc.Title,
            Path = doc.UrlPath,
            IsDirectory = false,
            Icon = doc.IsTutorial ? "bi-journal-code" : "bi-file-text"
        };
    }

    private string FormatCategoryName(string category)
    {
        return string.Join(" ", category.Split('-', '_'))
            .Replace("  ", " ")
            .Trim()
            .ToTitleCase();
    }

    private string GetCategoryIcon(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "getting-started" => "bi-rocket-takeoff",
            "tutorials" => "bi-journal-code",
            "architecture" => "bi-diagram-3",
            "troubleshooting" => "bi-tools",
            "examples" => "bi-code-square",
            "api-reference" => "bi-book",
            "best-practices" => "bi-award",
            "contributing" => "bi-people",
            _ => "bi-folder"
        };
    }

    private int GetCategoryOrder(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "getting-started" => 1,
            "tutorials" => 2,
            "architecture" => 3,
            "examples" => 4,
            "api-reference" => 5,
            "best-practices" => 6,
            "troubleshooting" => 7,
            "contributing" => 8,
            _ => 99
        };
    }

    private double CalculateRelevance(DocumentMetadata doc, string[] searchTerms)
    {
        double score = 0;
        var titleLower = doc.Title.ToLowerInvariant();
        var descLower = (doc.Description ?? "").ToLowerInvariant();

        foreach (var term in searchTerms)
        {
            if (titleLower.Contains(term)) score += 10; // Title matches are most relevant
            if (descLower.Contains(term)) score += 5;  // Description matches
            if (doc.Tags.Any(t => t.ToLowerInvariant().Contains(term))) score += 3; // Tag matches
        }

        return score;
    }
}

/// <summary>
/// Extension method for title casing
/// </summary>
public static class StringExtensions
{
    public static string ToTitleCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(text);
    }
}
