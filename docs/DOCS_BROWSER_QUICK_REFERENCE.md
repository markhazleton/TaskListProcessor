# Documentation Browser - Quick Reference

## ?? Quick Start

### Run the Web Application
```bash
cd examples/TaskListProcessor.Web
dotnet restore
dotnet build
dotnet run
```

### Access Documentation Browser
- **URL**: `https://localhost:5001/Docs`
- **Navigation**: Click "Docs Browser" in the top menu

---

## ?? File Structure

```
examples/TaskListProcessor.Web/
??? Models/
?   ??? DocumentMetadata.cs          # Document models and view models
??? Services/
?   ??? MarkdownService.cs           # Markdown parsing and rendering
??? Pages/
?   ??? Docs/
?       ??? Index.cshtml[.cs]        # Document browser
?       ??? ViewDocument.cshtml[.cs] # Document viewer
?       ??? _DocumentTreeNode.cshtml # Tree partial view
??? Program.cs                        # Service registration
??? Views/Shared/_Layout.cshtml      # Navigation updated
```

---

## ?? Configuration

### Program.cs Setup
```csharp
// Required services
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MarkdownService>();

// Required routing
app.MapRazorPages();
```

### NuGet Packages Required
```xml
<PackageReference Include="Markdig" Version="0.37.0" />
<PackageReference Include="Markdig.SyntaxHighlighting" Version="0.2.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.0" />
```

---

## ?? Key Features

### Document Browser (`/Docs`)
- Search all documents
- View progress statistics
- Navigate document tree
- Featured documents
- Recent updates

### Document Viewer (`/Docs/{path}`)
- Rendered markdown with syntax highlighting
- Table of contents (sticky sidebar)
- Breadcrumb navigation
- Copy-to-clipboard for code
- Mermaid diagram support
- Previous/Next navigation (tutorials)

---

## ?? Routes

| Route | Page | Purpose |
|-------|------|---------|
| `/Docs` | `Index.cshtml` | Document browser/home |
| `/Docs/{path}` | `ViewDocument.cshtml` | View any document |
| `/Docs/getting-started/01-quick-start-5-minutes` | `ViewDocument.cshtml` | Quick start guide |
| `/Docs/tutorials/beginner/01-simple-task-execution` | `ViewDocument.cshtml` | First tutorial |

---

## ?? Current Documentation

### Available Documents (28 total)
- **Getting Started**: 5 docs (100% complete)
- **Beginner Tutorials**: 5 docs (100% complete)
- **Intermediate Tutorials**: 6 docs (0% complete - planned)
- **Advanced Tutorials**: 6 docs (0% complete - planned)
- **Architecture**: 2 docs
- **Troubleshooting**: 1 doc (FAQ)
- **Project Docs**: 14 docs (progress reports, guides)

---

## ??? Troubleshooting

### Issue: Documents Not Found
**Solution**: Ensure `docs/` directory is accessible from web root
```csharp
// MarkdownService constructor
_docsPath = Path.Combine(environment.ContentRootPath, "..", "..", "docs");
```

### Issue: Styling Not Applied
**Solution**: 
1. Ensure Prism.js and Mermaid.js CDNs are accessible
2. Check `ViewDocument.cshtml` Scripts section
3. Verify `site.css` is loading

### Issue: Cache Not Clearing
**Solution**: Restart application or implement cache invalidation
```csharp
// Clear cache programmatically
_cache.Remove($"md:content:{path}");
_cache.Remove("md:all-documents");
```

---

## ?? Security Notes

### Path Traversal Protection
```csharp
// Security check in MarkdownService.GetFullPath()
var normalizedPath = Path.GetFullPath(fullPath);
if (!normalizedPath.StartsWith(normalizedDocsPath))
    throw new UnauthorizedAccessException("Path traversal detected");
```

### Only Markdown Files
- Only `.md` files in `docs/` directory are accessible
- No user-uploaded content
- Static files only

---

## ? Performance Tips

### Caching Strategy
- **Rendered HTML**: 1 hour cache
- **Document metadata**: 1 hour cache
- **Document tree**: 1 hour cache

### CDN Usage
- Prism.js: `cdn.jsdelivr.net`
- Mermaid.js: `cdn.jsdelivr.net`
- Reduces server bandwidth

### Optimization Checklist
- [x] Memory caching enabled
- [x] Lazy document loading
- [x] CDN for external assets
- [x] Minified CSS/JS (production)
- [x] Gzip compression (IIS/Kestrel)

---

## ?? Mobile Support

### Responsive Design
- Bootstrap 5 responsive grid
- Collapsible TOC sidebar on mobile
- Touch-friendly navigation
- Optimized font sizes

### Testing on Mobile
```bash
# Access from mobile device on same network
dotnet run --urls="http://0.0.0.0:5000"
# Then visit http://<your-ip>:5000/Docs
```

---

## ?? Customization

### Styling
Edit `/Pages/Docs/ViewDocument.cshtml` Scripts section:

```css
/* Markdown Content Styles */
.markdown-content {
    font-size: 1.1rem;  /* Adjust reading size */
    line-height: 1.7;   /* Adjust line spacing */
}
```

### Code Theme
Change Prism theme in ViewDocument.cshtml:
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism-tomorrow.min.css">
```

### Mermaid Theme
Change Mermaid theme in ViewDocument.cshtml:
```javascript
mermaid.initialize({
    theme: 'dark', // Options: default, dark, forest, neutral
});
```

---

## ?? Adding New Documents

### Step 1: Create Markdown File
```bash
# Create a new tutorial
touch docs/tutorials/intermediate/01-dependency-injection.md
```

### Step 2: Write Content
```markdown
# Tutorial Title

**Level**: Intermediate
**Duration**: 20 minutes

## What You'll Learn

- Bullet point 1
- Bullet point 2

## Step-by-Step

### Step 1: ...
```

### Step 3: Refresh Browser
- Document automatically appears in document tree
- Metadata automatically extracted
- No code changes needed!

---

## ?? Testing Checklist

### Functional Testing
- [ ] Navigate to `/Docs` - Document browser loads
- [ ] Search for "async" - Results appear
- [ ] Click a category - Expands/collapses
- [ ] Click a document - Renders correctly
- [ ] Syntax highlighting works
- [ ] Copy button copies code
- [ ] TOC links scroll to sections
- [ ] Previous/Next navigation works
- [ ] Breadcrumbs navigate correctly

### Performance Testing
- [ ] First load < 1 second
- [ ] Cached load < 100ms
- [ ] Search returns < 500ms
- [ ] Memory usage stable

### Mobile Testing
- [ ] Responsive on small screens
- [ ] Touch navigation works
- [ ] TOC collapsible
- [ ] Copy buttons accessible

---

## ?? Tips & Tricks

### Quick Navigation
- Use browser back button - It works!
- Bookmark favorite documents
- Use search for quick access

### Code Copying
- Hover over code block
- Click copy button (top-right)
- Paste into your editor

### Print Documents
- Browser print works
- CSS optimized for print
- TOC removed in print view

---

## ?? Related Documentation

- [Improvement Plan](IMPROVEMENT_PLAN.md) - Overall project roadmap
- [Implementation Summary](MARKDOWN_VIEWER_IMPLEMENTATION.md) - Detailed implementation notes
- [Phase 0 Complete](PHASE0_COMPLETE.md) - Foundation documentation
- [Phase 1 Progress](PHASE1_PROGRESS.md) - Tutorial creation status
- [Phase 2 Complete](PHASE2_COMPLETE.md) - Web experience enhancements

---

## ?? Support

### Questions?
- Check the [FAQ](../docs/troubleshooting/faq.md)
- Review the [Implementation Guide](MARKDOWN_VIEWER_IMPLEMENTATION.md)
- Open a [GitHub Issue](https://github.com/markhazleton/TaskListProcessor/issues)

### Contributions Welcome!
- Add new tutorials
- Improve existing docs
- Fix typos or errors
- Suggest enhancements

---

**Happy Documenting! ??**
