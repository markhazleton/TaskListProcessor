# Fix-MarkdownEncoding.ps1
# Fixes special character encoding issues in markdown files

param(
    [string]$Path = "docs",
    [switch]$WhatIf
)

# Unicode replacements
$replacements = @{
    # Tree structure characters
    '???' = '???'
    '?   ?' = '?   ?'
    '?   ' = '?   '
    '??' = '??'
    
    # Emojis
    '?' = '??'  # Target
    '?' = '??'  # Books
    '?' = '??'  # Light bulb
    '?' = '??'  # Handshake
    '?' = '??'  # Graduate cap
    '?' = '?'  # Sparkles
    '?' = '?'  # Check mark
    '?' = '??'  # Warning
    '?' = '??'  # Rocket
    '?' = '??'  # Hammer
    '?' = '??'  # Trophy
    '?' = '??'  # Chart
    '?' = '??'  # Computer
    '?' = '??'  # Test tube
    '?' = '??'  # Artist palette
    '?' = '?'  # Star
    '?' = '??'  # Glowing star
    '?' = '??'  # Memo
    '?' = '???'  # Eye
    '?' = '??'  # Gem
    '?' = '??'  # Car
    '?' = '?'  # Question
    '?' = '???'  # Medal
    '?' = '??'  # Phone
    '?' = '??'  # Briefcase
    '?' = '??'  # Globe
    '?' = '??'  # Chart increasing
}

function Fix-MarkdownFile {
    param([string]$FilePath)
    
    $content = Get-Content $FilePath -Raw -Encoding UTF8
    $originalContent = $content
    
    foreach ($key in $replacements.Keys) {
        $content = $content -replace [regex]::Escape($key), $replacements[$key]
    }
    
    if ($content -ne $originalContent) {
        if ($WhatIf) {
            Write-Host "Would fix: $FilePath" -ForegroundColor Yellow
        } else {
            # Save with UTF-8 BOM to ensure proper encoding
            [System.IO.File]::WriteAllText($FilePath, $content, [System.Text.UTF8Encoding]::new($true))
            Write-Host "Fixed: $FilePath" -ForegroundColor Green
        }
        return $true
    }
    return $false
}

# Process markdown files
$mdFiles = Get-ChildItem -Path $Path -Filter "*.md" -Recurse
$fixedCount = 0

foreach ($file in $mdFiles) {
    if (Fix-MarkdownFile -FilePath $file.FullName) {
        $fixedCount++
    }
}

Write-Host "`n? Fixed $fixedCount file(s)" -ForegroundColor Green

if ($WhatIf) {
    Write-Host "`nRun without -WhatIf to apply changes" -ForegroundColor Cyan
}
