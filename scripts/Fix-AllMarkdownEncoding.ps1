# Fix-AllMarkdownEncoding.ps1
# Automatically fixes all special character encoding issues in markdown files
# Run with -WhatIf to preview changes without applying them

param(
    [string]$Path = "docs",
    [switch]$WhatIf,
    [switch]$Verbose
)

Write-Host "=== Markdown Encoding Fix Script ===" -ForegroundColor Cyan
Write-Host "Scanning directory: $Path" -ForegroundColor Cyan
Write-Host ""

# Define all replacements as ordered pairs (to handle multi-character sequences first)
$replacements = @(
    # Multi-character sequences (must be first to avoid partial replacements)
    @{ Pattern = [regex]::Escape('?   ?'); Replacement = '?   ?'; Description = 'Tree: vertical + space + branch' }
    @{ Pattern = [regex]::Escape('?   '); Replacement = '?   '; Description = 'Tree: vertical + spaces' }
    @{ Pattern = [regex]::Escape('???'); Replacement = '???'; Description = 'Tree: branch with line' }
    @{ Pattern = [regex]::Escape('??'); Replacement = '??'; Description = 'Tree: short branch' }
    @{ Pattern = [regex]::Escape('??'); Replacement = '??'; Description = 'Emoji: target' }
    
    # Single character emojis (after multi-char to avoid conflicts)
    @{ Pattern = '(?<![???])' + [regex]::Escape('?') + '(?=\s*\*\*Teaches\*\*)'; Replacement = '??'; Description = 'Emoji: books (Teaches)' }
    @{ Pattern = '(?<![???])' + [regex]::Escape('?') + '(?=\s*\*\*Demonstrates\*\*)'; Replacement = '??'; Description = 'Emoji: light bulb (Demonstrates)' }
    @{ Pattern = '(?<![???])' + [regex]::Escape('?') + '(?=\s*\*\*Engages\*\*)'; Replacement = '??'; Description = 'Emoji: handshake (Engages)' }
    @{ Pattern = '(?<![???])' + [regex]::Escape('?') + '(?=\s*\*\*Guides\*\*)'; Replacement = '??'; Description = 'Emoji: graduation cap (Guides)' }
    @{ Pattern = '(?<![???])' + [regex]::Escape('?') + '(?=\s*\*\*Inspires\*\*)'; Replacement = '?'; Description = 'Emoji: sparkles (Inspires)' }
    @{ Pattern = '(?<=extraordinary!\s*)' + [regex]::Escape('??'); Replacement = '??'; Description = 'Emoji: rocket (extraordinary)' }
    @{ Pattern = '(?<=\s)' + [regex]::Escape('?') + '(?=\s+Complete)'; Replacement = '?'; Description = 'Emoji: check mark' }
    @{ Pattern = '(?<=\s)' + [regex]::Escape('?') + '(?=\s+Planned)'; Replacement = '?'; Description = 'Emoji: hourglass' }
)

function Test-ShouldReplace {
    param(
        [string]$FilePath,
        [string]$Content,
        [hashtable]$Replacement
    )
    
    # Check if pattern exists in content
    if ($Content -match $Replacement.Pattern) {
        return $true
    }
    return $false
}

function Fix-MarkdownFile {
    param(
        [string]$FilePath
    )
    
    try {
        # Read file with UTF-8 encoding
        $content = [System.IO.File]::ReadAllText($FilePath, [System.Text.Encoding]::UTF8)
        $originalContent = $content
        $changesApplied = @()
        
        # Apply replacements in order
        foreach ($replacement in $replacements) {
            if ($content -match $replacement.Pattern) {
                $matchCount = ([regex]::Matches($content, $replacement.Pattern)).Count
                $content = $content -replace $replacement.Pattern, $replacement.Replacement
                $changesApplied += "$($replacement.Description) ($matchCount occurrences)"
                
                if ($Verbose) {
                    Write-Host "  - $($replacement.Description): $matchCount replacements" -ForegroundColor DarkGray
                }
            }
        }
        
        # Check if content changed
        if ($content -ne $originalContent) {
            if ($WhatIf) {
                Write-Host "WOULD FIX: $FilePath" -ForegroundColor Yellow
                foreach ($change in $changesApplied) {
                    Write-Host "  ? $change" -ForegroundColor DarkYellow
                }
                return $true
            }
            else {
                # Create backup
                $backupPath = "$FilePath.backup"
                [System.IO.File]::Copy($FilePath, $backupPath, $true)
                
                # Save with UTF-8 BOM to ensure proper encoding
                $utf8BOM = New-Object System.Text.UTF8Encoding $true
                [System.IO.File]::WriteAllText($FilePath, $content, $utf8BOM)
                
                Write-Host "FIXED: $FilePath" -ForegroundColor Green
                foreach ($change in $changesApplied) {
                    Write-Host "  ? $change" -ForegroundColor DarkGreen
                }
                
                # Remove backup if successful
                Remove-Item $backupPath -Force
                return $true
            }
        }
        
        return $false
    }
    catch {
        Write-Host "ERROR: $FilePath - $_" -ForegroundColor Red
        return $false
    }
}

# Find all markdown files
$mdFiles = Get-ChildItem -Path $Path -Filter "*.md" -Recurse -File | 
    Where-Object { $_.FullName -notlike "*\node_modules\*" -and $_.FullName -notlike "*\.git\*" }

Write-Host "Found $($mdFiles.Count) markdown file(s) to process" -ForegroundColor Cyan
Write-Host ""

# Process files
$fixedCount = 0
$processedCount = 0

foreach ($file in $mdFiles) {
    $processedCount++
    Write-Host "[$processedCount/$($mdFiles.Count)] Processing: $($file.Name)" -ForegroundColor White
    
    if (Fix-MarkdownFile -FilePath $file.FullName) {
        $fixedCount++
    }
    else {
        Write-Host "  No changes needed" -ForegroundColor DarkGray
    }
    
    Write-Host ""
}

# Summary
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Files processed: $processedCount" -ForegroundColor White
Write-Host "Files fixed: $fixedCount" -ForegroundColor $(if ($fixedCount -gt 0) { 'Green' } else { 'Gray' })
Write-Host "Files unchanged: $($processedCount - $fixedCount)" -ForegroundColor Gray

if ($WhatIf) {
    Write-Host ""
    Write-Host "Run without -WhatIf parameter to apply these changes" -ForegroundColor Yellow
}
else {
    Write-Host ""
    Write-Host "? Encoding fix complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Review the changes in Git" -ForegroundColor White
    Write-Host "2. Test the documentation browser at https://localhost:5001/Docs" -ForegroundColor White
    Write-Host "3. View files on GitHub to verify emoji rendering" -ForegroundColor White
}

Write-Host ""
Write-Host "Tip: Use -Verbose flag for detailed replacement information" -ForegroundColor DarkGray
