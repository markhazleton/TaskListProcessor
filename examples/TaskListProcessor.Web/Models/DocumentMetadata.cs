namespace TaskListProcessor.Web.Models;

/// <summary>
/// Metadata for a markdown document
/// </summary>
public class DocumentMetadata
{
    /// <summary>
    /// The document title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The relative file path from docs/ directory
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The category (getting-started, tutorials, etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The subcategory (beginner, intermediate, advanced, etc.)
    /// </summary>
    public string? Subcategory { get; set; }

    /// <summary>
    /// Document description or summary
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Last modified date
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Estimated reading time in minutes
    /// </summary>
    public int ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Whether this is a tutorial (has completion status)
    /// </summary>
    public bool IsTutorial { get; set; }

    /// <summary>
    /// Tutorial level (if applicable)
    /// </summary>
    public string? TutorialLevel { get; set; }

    /// <summary>
    /// Tags for searchability
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// The URL-friendly path for routing
    /// </summary>
    public string UrlPath => Path.Replace("\\", "/").Replace(".md", "");
}

/// <summary>
/// View model for markdown document viewing
/// </summary>
public class MarkdownViewModel
{
    /// <summary>
    /// The rendered HTML content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Document metadata
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Table of contents (generated from headings)
    /// </summary>
    public List<TocItem> TableOfContents { get; set; } = new();

    /// <summary>
    /// Previous document in sequence (if applicable)
    /// </summary>
    public DocumentMetadata? PreviousDocument { get; set; }

    /// <summary>
    /// Next document in sequence (if applicable)
    /// </summary>
    public DocumentMetadata? NextDocument { get; set; }

    /// <summary>
    /// Breadcrumb navigation
    /// </summary>
    public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
}

/// <summary>
/// Table of contents item
/// </summary>
public class TocItem
{
    /// <summary>
    /// Heading text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Heading level (1-6)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Anchor ID for navigation
    /// </summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Breadcrumb navigation item
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Display text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// URL (null for current page)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Whether this is the current page
    /// </summary>
    public bool IsActive => Url == null;
}

/// <summary>
/// Document tree for navigation
/// </summary>
public class DocumentTreeNode
{
    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL path
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Whether this is a directory
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Child nodes
    /// </summary>
    public List<DocumentTreeNode> Children { get; set; } = new();

    /// <summary>
    /// Icon class (Bootstrap Icons)
    /// </summary>
    public string Icon { get; set; } = "bi-file-text";

    /// <summary>
    /// Badge text (e.g., "5/5" for completed tutorials)
    /// </summary>
    public string? Badge { get; set; }

    /// <summary>
    /// Badge CSS class
    /// </summary>
    public string? BadgeClass { get; set; }
}

/// <summary>
/// View model for document browser
/// </summary>
public class DocumentBrowserViewModel
{
    /// <summary>
    /// All available documents
    /// </summary>
    public List<DocumentMetadata> AllDocuments { get; set; } = new();

    /// <summary>
    /// Document tree for navigation
    /// </summary>
    public List<DocumentTreeNode> DocumentTree { get; set; } = new();

    /// <summary>
    /// Recent documents
    /// </summary>
    public List<DocumentMetadata> RecentDocuments { get; set; } = new();

    /// <summary>
    /// Featured documents
    /// </summary>
    public List<DocumentMetadata> FeaturedDocuments { get; set; } = new();

    /// <summary>
    /// Progress statistics
    /// </summary>
    public ProgressStatistics Progress { get; set; } = new();
}

/// <summary>
/// Learning progress statistics
/// </summary>
public class ProgressStatistics
{
    /// <summary>
    /// Total number of tutorials
    /// </summary>
    public int TotalTutorials { get; set; }

    /// <summary>
    /// Number of completed tutorials (beginner only currently)
    /// </summary>
    public int CompletedTutorials { get; set; }

    /// <summary>
    /// Completion percentage
    /// </summary>
    public double CompletionPercentage => TotalTutorials > 0 
        ? (double)CompletedTutorials / TotalTutorials * 100 
        : 0;

    /// <summary>
    /// Beginner tutorials completed
    /// </summary>
    public int BeginnerCompleted { get; set; }

    /// <summary>
    /// Beginner tutorials total
    /// </summary>
    public int BeginnerTotal { get; set; }

    /// <summary>
    /// Intermediate tutorials completed
    /// </summary>
    public int IntermediateCompleted { get; set; }

    /// <summary>
    /// Intermediate tutorials total
    /// </summary>
    public int IntermediateTotal { get; set; }

    /// <summary>
    /// Advanced tutorials completed
    /// </summary>
    public int AdvancedCompleted { get; set; }

    /// <summary>
    /// Advanced tutorials total
    /// </summary>
    public int AdvancedTotal { get; set; }
}
