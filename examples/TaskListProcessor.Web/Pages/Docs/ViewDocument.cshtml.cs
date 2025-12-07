using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskListProcessor.Web.Models;
using TaskListProcessor.Web.Services;

namespace TaskListProcessor.Web.Pages.Docs;

/// <summary>
/// Document viewer page model
/// </summary>
public class ViewDocumentModel : PageModel
{
    private readonly MarkdownService _markdownService;
    private readonly ILogger<ViewDocumentModel> _logger;

    public MarkdownViewModel ViewModel { get; set; } = new();

    public ViewDocumentModel(MarkdownService markdownService, ILogger<ViewDocumentModel> logger)
    {
        _markdownService = markdownService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return RedirectToPage("/Docs/Index");
        }

        try
        {
            // Ensure .md extension
            var filePath = path.EndsWith(".md") ? path : $"{path}.md";

            // Render markdown
            var content = await _markdownService.RenderMarkdownAsync(filePath);
            var metadata = await _markdownService.GetDocumentMetadataAsync(filePath);

            // Get raw markdown for TOC generation
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs", filePath);
            var rawMarkdown = await System.IO.File.ReadAllTextAsync(fullPath);
            var toc = _markdownService.GenerateTableOfContents(rawMarkdown);

            // Build breadcrumbs
            var breadcrumbs = BuildBreadcrumbs(metadata);

            // Get previous/next documents (for tutorials)
            var (previous, next) = await GetAdjacentDocuments(metadata);

            ViewModel = new MarkdownViewModel
            {
                Content = content,
                Metadata = metadata,
                TableOfContents = toc,
                Breadcrumbs = breadcrumbs,
                PreviousDocument = previous,
                NextDocument = next
            };

            return Page();
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("Document not found: {Path}", path);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading document: {Path}", path);
            return StatusCode(500, "Error loading document");
        }
    }

    private List<BreadcrumbItem> BuildBreadcrumbs(DocumentMetadata metadata)
    {
        var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Text = "Docs", Url = "/Docs" },
            new BreadcrumbItem { Text = FormatCategoryName(metadata.Category), Url = $"/Docs#{metadata.Category}" }
        };

        if (!string.IsNullOrEmpty(metadata.Subcategory))
        {
            breadcrumbs.Add(new BreadcrumbItem 
            { 
                Text = FormatCategoryName(metadata.Subcategory), 
                Url = $"/Docs#{metadata.Category}-{metadata.Subcategory}" 
            });
        }

        breadcrumbs.Add(new BreadcrumbItem { Text = metadata.Title, Url = null });

        return breadcrumbs;
    }

    private async Task<(DocumentMetadata? Previous, DocumentMetadata? Next)> GetAdjacentDocuments(DocumentMetadata current)
    {
        if (!current.IsTutorial)
            return (null, null);

        var allDocuments = await _markdownService.GetAllDocumentsAsync();
        var sameLevelDocs = allDocuments
            .Where(d => d.Category == current.Category && d.Subcategory == current.Subcategory)
            .OrderBy(d => d.Path)
            .ToList();

        var currentIndex = sameLevelDocs.FindIndex(d => d.Path == current.Path);
        if (currentIndex == -1)
            return (null, null);

        var previous = currentIndex > 0 ? sameLevelDocs[currentIndex - 1] : null;
        var next = currentIndex < sameLevelDocs.Count - 1 ? sameLevelDocs[currentIndex + 1] : null;

        return (previous, next);
    }

    private string FormatCategoryName(string category)
    {
        return string.Join(" ", category.Split('-', '_'))
            .Replace("  ", " ")
            .Trim()
            .ToTitleCase();
    }
}

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
