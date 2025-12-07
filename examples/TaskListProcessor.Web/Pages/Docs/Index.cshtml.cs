using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskListProcessor.Web.Models;
using TaskListProcessor.Web.Services;

namespace TaskListProcessor.Web.Pages.Docs;

/// <summary>
/// Document browser page model
/// </summary>
public class IndexModel : PageModel
{
    private readonly MarkdownService _markdownService;
    private readonly ILogger<IndexModel> _logger;

    public DocumentBrowserViewModel ViewModel { get; set; } = new();

    public IndexModel(MarkdownService markdownService, ILogger<IndexModel> logger)
    {
        _markdownService = markdownService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var allDocuments = await _markdownService.GetAllDocumentsAsync();
            var documentTree = await _markdownService.BuildDocumentTreeAsync();

            // Calculate progress statistics
            var tutorials = allDocuments.Where(d => d.IsTutorial).ToList();
            var beginnerTutorials = tutorials.Where(t => t.TutorialLevel?.Equals("beginner", StringComparison.OrdinalIgnoreCase) == true).ToList();
            var intermediateTutorials = tutorials.Where(t => t.TutorialLevel?.Equals("intermediate", StringComparison.OrdinalIgnoreCase) == true).ToList();
            var advancedTutorials = tutorials.Where(t => t.TutorialLevel?.Equals("advanced", StringComparison.OrdinalIgnoreCase) == true).ToList();

            ViewModel = new DocumentBrowserViewModel
            {
                AllDocuments = allDocuments,
                DocumentTree = documentTree,
                FeaturedDocuments = GetFeaturedDocuments(allDocuments),
                RecentDocuments = allDocuments.OrderByDescending(d => d.LastModified).Take(5).ToList(),
                Progress = new ProgressStatistics
                {
                    TotalTutorials = tutorials.Count,
                    CompletedTutorials = 5, // Only beginner tutorials are complete
                    BeginnerTotal = beginnerTutorials.Count,
                    BeginnerCompleted = 5,
                    IntermediateTotal = intermediateTutorials.Count,
                    IntermediateCompleted = 0,
                    AdvancedTotal = advancedTutorials.Count,
                    AdvancedCompleted = 0
                }
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading document browser");
            return StatusCode(500, "Error loading documents");
        }
    }

    private List<DocumentMetadata> GetFeaturedDocuments(List<DocumentMetadata> allDocuments)
    {
        var featured = new List<string>
        {
            "getting-started/01-quick-start-5-minutes.md",
            "getting-started/03-your-first-processor.md",
            "tutorials/beginner/01-simple-task-execution.md",
            "architecture/design-principles.md",
            "troubleshooting/faq.md"
        };

        return allDocuments
            .Where(d => featured.Any(f => d.Path.Equals(f, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}
