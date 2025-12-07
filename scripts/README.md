# Markdown Encoding Fix Scripts

This directory contains PowerShell scripts to fix special character encoding issues in markdown files.

## The Problem

Several markdown files contain Unicode characters (emojis, box-drawing characters) that appear as '?' due to encoding issues. This happens when:
- Files aren't saved with UTF-8 encoding
- Text editors don't properly handle Unicode characters
- Copy/paste operations lose encoding information

## Available Scripts

### 1. Fix-AllMarkdownEncoding.ps1 (Comprehensive)

**Features:**
- Ordered replacement logic (multi-char first, then single chars)
- Context-aware replacements using regex
- Detailed reporting of each change
- Backup creation before changes
- Verbose mode for debugging

**Usage:**
```powershell
# Preview changes without applying
.\scripts\Fix-AllMarkdownEncoding.ps1 -WhatIf

# Apply fixes
.\scripts\Fix-AllMarkdownEncoding.ps1

# Apply fixes with detailed output
.\scripts\Fix-AllMarkdownEncoding.ps1 -Verbose
```

### 2. Fix-MarkdownEncodingSimple.ps1 (Simple & Fast)

**Features:**
- Straightforward pattern replacements
- Quick execution
- Easy to understand and modify
- No complex regex patterns

**Usage:**
```powershell
# Preview changes
.\scripts\Fix-MarkdownEncodingSimple.ps1 -WhatIf

# Apply fixes
.\scripts\Fix-MarkdownEncodingSimple.ps1
```

## What Gets Fixed

### Box-Drawing Characters (Directory Trees)

| Before | After | Description |
|--------|-------|-------------|
| `???` | `???` | Tree branch with line |
| `?   ?` | `?   ?` | Vertical + branch |
| `?   ` | `?   ` | Vertical + spaces |

### Emojis

| Before | After | Usage |
|--------|-------|-------|
| `? **Teaches**` | `?? **Teaches**` | Learning content |
| `? **Demonstrates**` | `?? **Demonstrates**` | Examples |
| `? **Engages**` | `?? **Engages**` | Community |
| `? **Guides**` | `?? **Guides**` | Education |
| `? **Inspires**` | `? **Inspires**` | Motivation |
| `extraordinary! ??` | `extraordinary! ??` | Launch/Deployment |
| `## ?? **Vision**` | `### ?? **Vision**` | Goals |

## Recommended Workflow

### Step 1: Preview Changes
```powershell
cd C:\GitHub\MarkHazleton\TaskListProcessor
.\scripts\Fix-MarkdownEncodingSimple.ps1 -WhatIf
```

### Step 2: Apply Fixes
```powershell
.\scripts\Fix-MarkdownEncodingSimple.ps1
```

### Step 3: Review Changes
```powershell
git diff docs/
```

### Step 4: Test Documentation
```powershell
cd examples\TaskListProcessor.Web
dotnet run
# Navigate to https://localhost:5001/Docs
```

### Step 5: Commit Changes
```powershell
git add docs/
git commit -m "fix: correct special character encoding in markdown files"
```

## Affected Files

The following files contain encoding issues:

1. **docs/IMPROVEMENT_PLAN.md** - Multiple issues (tree structures, emojis)
2. **docs/QUICK_START_PLAN.md** - Emojis in headings and lists
3. **docs/REPOSITORY_REVIEW.md** - Section heading emojis
4. **docs/PROJECT_STATUS.md** - Multiple emoji issues
5. **docs/tutorials/\*.md** - Various files with arrow characters

## Prevention

### VS Code Settings

Add to `.vscode/settings.json`:
```json
{
  "files.encoding": "utf8bom",
  "files.autoGuessEncoding": true,
  "files.eol": "\\n",
  "[markdown]": {
    "files.encoding": "utf8bom"
  }
}
```

### EditorConfig

Add to `.editorconfig`:
```ini
[*.md]
charset = utf-8-bom
end_of_line = lf
insert_final_newline = true
```

### Git Configuration

Ensure Git handles UTF-8 properly:
```bash
git config --global core.autocrlf false
git config --global core.quotepath false
git config --global i18n.commitencoding utf-8
git config --global i18n.logoutputencoding utf-8
```

## Troubleshooting

### Issue: Script shows no changes needed but '?' still visible

**Solution:** The file may have different encoding. Try:
```powershell
# Check file encoding
Get-Content docs/IMPROVEMENT_PLAN.md -Encoding UTF8 | Select-String -Pattern "Vision Statement"

# Force re-save with UTF-8 BOM
$content = Get-Content docs/IMPROVEMENT_PLAN.md -Raw -Encoding UTF8
[System.IO.File]::WriteAllText("docs/IMPROVEMENT_PLAN.md", $content, (New-Object System.Text.UTF8Encoding $true))
```

### Issue: Emojis don't render on GitHub

**Solution:** Ensure files are saved with UTF-8 BOM:
- The scripts automatically add BOM
- Verify with: `(Get-Content docs/IMPROVEMENT_PLAN.md -AsByteStream -First 3)` should show `239 187 191` (UTF-8 BOM)

### Issue: Tree characters look broken in terminal

**Solution:** This is normal. They'll render correctly in:
- GitHub web interface
- VS Code markdown preview
- Documentation browser at `/Docs`

## Testing

After running the fix script, verify:

1. **GitHub Preview:**
   - Commit and push changes
   - View files on GitHub
   - Emojis should render correctly

2. **Documentation Browser:**
   ```powershell
   cd examples\TaskListProcessor.Web
   dotnet run
   ```
   Navigate to https://localhost:5001/Docs

3. **VS Code Preview:**
   - Open any fixed `.md` file
   - Press `Ctrl+Shift+V` for preview
   - Verify emojis and tree characters

## Additional Resources

- **Full Fix Guide:** `docs/MARKDOWN_ENCODING_FIX_GUIDE.md`
- **Character Reference:** See guide for complete emoji/character mapping
- **GitHub Markdown:** https://docs.github.com/en/get-started/writing-on-github

## Quick Reference

```powershell
# Preview what would be fixed
.\scripts\Fix-MarkdownEncodingSimple.ps1 -WhatIf

# Apply all fixes
.\scripts\Fix-MarkdownEncodingSimple.ps1

# Review changes
git diff

# Test in browser
cd examples\TaskListProcessor.Web && dotnet run
```

---

**Last Updated:** 2024-12-07  
**Maintainer:** TaskListProcessor Team  
**Status:** Ready for use
