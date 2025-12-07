# Fix-MarkdownEncodingFinal.ps1
# Fixes markdown encoding issues using Unicode character codes
# Usage: .\Fix-MarkdownEncodingFinal.ps1 [-WhatIf]

param(
    [switch]$WhatIf
)

Write-Host "`n=== Markdown Encoding Fix ===" -ForegroundColor Cyan

# Define replacements using Unicode escape sequences to avoid encoding issues in script itself
$replacements = @(
    # Question mark patterns that should be box-drawing characters
    @{ Find = [char]0x3F + [char]0x3F + [char]0x3F; Replace = [char]0x251C + [char]0x2500 + [char]0x2500; Desc = 'Tree branch ???' }
    @{ Find = [char]0x3F + '   ' + [char]0x3F; Replace = [char]0x2502 + '   ' + [char]0x251C; Desc = 'Tree vertical + branch ?   ?' }
    @{ Find = [char]0x3F + '   '; Replace = [char]0x2502 + '   '; Desc = 'Tree vertical ?' }
    
    # Question marks that should be emojis (with context)
    @{ Find = [char]0x3F + ' \*\*Teaches\*\*'; Replace = ([char]0xD83D + [char]0xDCDA) + ' **Teaches**'; Desc = 'Books emoji ??' }
    @{ Find = [char]0x3F + ' \*\*Demonstrates\*\*'; Replace = ([char]0xD83D + [char]0xDCA1) + ' **Demonstrates**'; Desc = 'Light bulb emoji ??' }
    @{ Find = [char]0x3F + ' \*\*Engages\*\*'; Replace = ([char]0xD83E + [char]0xDD1D) + ' **Engages**'; Desc = 'Handshake emoji ??' }
    @{ Find = [char]0x3F + ' \*\*Guides\*\*'; Replace = ([char]0xD83C + [char]0xDF93) + ' **Guides**'; Desc = 'Graduation cap emoji ??' }
    @{ Find = [char]0x3F + ' \*\*Inspires\*\*'; Replace = ([char]0x2728) + ' **Inspires**'; Desc = 'Sparkles emoji ?' }
    @{ Find = 'extraordinary! ' + [char]0x3F + [char]0x3F; Replace = 'extraordinary! ' + ([char]0xD83D + [char]0xDE80); Desc = 'Rocket emoji ??' }
    @{ Find = '### ' + [char]0x3F + [char]0x3F + ' \*\*Vision Statement\*\*'; Replace = '### ' + ([char]0xD83C + [char]0xDFAF) + ' **Vision Statement**'; Desc = 'Target emoji ??' }
)

# Get all markdown files
$files = Get-ChildItem -Path "docs" -Filter "*.md" -Recurse -File

Write-Host "Found $($files.Count) markdown files`n" -ForegroundColor White

$fixedCount = 0
$totalReplacements = 0

foreach ($file in $files) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Yellow
    
    try {
        # Read file as UTF-8
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        $original = $content
        $fileReplacements = 0
        
        # Apply each replacement
        foreach ($repl in $replacements) {
            $before = $content
            $content = $content -replace $repl.Find, $repl.Replace
            if ($content -ne $before) {
                $count = ([regex]::Matches($before, $repl.Find)).Count
                $fileReplacements += $count
                Write-Host "  - $($repl.Desc): $count" -ForegroundColor DarkGray
            }
        }
        
        # Check if changes were made
        if ($content -ne $original) {
            $fixedCount++
            $totalReplacements += $fileReplacements
            
            if ($WhatIf) {
                Write-Host "  Would fix: $fileReplacements replacements" -ForegroundColor DarkYellow
            }
            else {
                # Save with UTF-8 BOM
                $utf8BOM = New-Object System.Text.UTF8Encoding $true
                [System.IO.File]::WriteAllText($file.FullName, $content, $utf8BOM)
                Write-Host "  ? Fixed: $fileReplacements replacements" -ForegroundColor Green
            }
        }
        else {
            Write-Host "  No changes needed" -ForegroundColor DarkGray
        }
    }
    catch {
        Write-Host "  ERROR: $_" -ForegroundColor Red
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Files processed: $($files.Count)" -ForegroundColor White
Write-Host "Files fixed: $fixedCount" -ForegroundColor $(if ($fixedCount -gt 0) { 'Green' } else { 'Gray' })
Write-Host "Total replacements: $totalReplacements" -ForegroundColor $(if ($totalReplacements -gt 0) { 'Green' } else { 'Gray' })

if ($WhatIf) {
    Write-Host "`nRun without -WhatIf to apply changes" -ForegroundColor Yellow
}
elseif ($fixedCount -gt 0) {
    Write-Host "`n? Complete! Verify with:" -ForegroundColor Green
    Write-Host "  git diff docs/" -ForegroundColor White
    Write-Host "  cd examples\TaskListProcessor.Web && dotnet run" -ForegroundColor White
}

Write-Host ""
