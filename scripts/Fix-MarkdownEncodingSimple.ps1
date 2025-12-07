# Fix-MarkdownEncodingSimple.ps1
# Simple script to fix common markdown encoding issues
# Usage: .\Fix-MarkdownEncodingSimple.ps1 [-WhatIf]

param(
    [switch]$WhatIf
)

Write-Host "`n=== Markdown Encoding Fix ===" -ForegroundColor Cyan

# Get all markdown files in docs folder
$files = Get-ChildItem -Path "docs" -Filter "*.md" -Recurse

Write-Host "Found $($files.Count) markdown files`n" -ForegroundColor White

$fixedCount = 0

foreach ($file in $files) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Yellow
    
    # Read file as UTF-8
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content
    
    # Fix tree characters
    $content = $content -replace '???', '???'
    $content = $content -replace '\?   \?', '?   ?'
    $content = $content -replace '\?   ', '?   '
    
    # Fix common emojis with context
    $content = $content -replace '\? \*\*Teaches\*\*', '?? **Teaches**'
    $content = $content -replace '\? \*\*Demonstrates\*\*', '?? **Demonstrates**'
    $content = $content -replace '\? \*\*Engages\*\*', '?? **Engages**'
    $content = $content -replace '\? \*\*Guides\*\*', '?? **Guides**'
    $content = $content -replace '\? \*\*Inspires\*\*', '? **Inspires**'
    $content = $content -replace 'extraordinary! \?\?', 'extraordinary! ??'
    $content = $content -replace '## \?\? \*\*Vision Statement\*\*', '### ?? **Vision Statement**'
    
    # Check if changes were made
    if ($content -ne $original) {
        $fixedCount++
        
        if ($WhatIf) {
            Write-Host "  Would fix this file" -ForegroundColor DarkYellow
        }
        else {
            # Save with UTF-8 BOM
            $utf8BOM = New-Object System.Text.UTF8Encoding $true
            [System.IO.File]::WriteAllText($file.FullName, $content, $utf8BOM)
            Write-Host "  ? Fixed" -ForegroundColor Green
        }
    }
    else {
        Write-Host "  No changes needed" -ForegroundColor DarkGray
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Files fixed: $fixedCount of $($files.Count)" -ForegroundColor $(if ($fixedCount -gt 0) { 'Green' } else { 'Gray' })

if ($WhatIf) {
    Write-Host "`nRun without -WhatIf to apply changes" -ForegroundColor Yellow
}
elseif ($fixedCount -gt 0) {
    Write-Host "`n? Complete! Review changes with: git diff" -ForegroundColor Green
}

Write-Host ""
