#!/usr/bin/env pwsh
# Script to test removal of potentially unused packages

$projectPath = "src/TaskListProcessing/TaskListProcessing.csproj"
$potentiallyUnusedPackages = @(
    "BenchmarkDotNet",
    "BenchmarkDotNet.Diagnostics.Windows", 
    "OpenTelemetry",
    "OpenTelemetry.Extensions.Hosting",
    "Microsoft.AspNetCore.Mvc.Core",
    "Microsoft.AspNetCore.Http.Abstractions",
    "Microsoft.Extensions.Options.ConfigurationExtensions",
    "Microsoft.Extensions.Hosting.Abstractions",
    "Microsoft.Extensions.Diagnostics.HealthChecks",
    "Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions"
)

Write-Host "🔍 Testing removal of potentially unused packages..." -ForegroundColor Yellow
Write-Host ""

# Create backup
Copy-Item $projectPath "$projectPath.backup"

foreach ($package in $potentiallyUnusedPackages) {
    Write-Host "Testing removal of: $package" -ForegroundColor Cyan
    
    # Remove package
    $result = dotnet remove $projectPath package $package 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Package removed" -ForegroundColor Green
        
        # Test build
        $buildResult = dotnet build $projectPath 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Build successful - Package is likely unused!" -ForegroundColor Green
            Write-Host "     Consider removing $package permanently" -ForegroundColor Green
        }
        else {
            Write-Host "  ❌ Build failed - Package is needed" -ForegroundColor Red
            Write-Host "     Restoring package..." -ForegroundColor Yellow
            # Restore from backup
            Copy-Item "$projectPath.backup" $projectPath
        }
    }
    else {
        Write-Host "  ⚠️  Package not found or already removed" -ForegroundColor Yellow
    }
    
    Write-Host ""
}

Write-Host "🔄 Restoring original project file..." -ForegroundColor Yellow
Copy-Item "$projectPath.backup" $projectPath
Remove-Item "$projectPath.backup"

Write-Host "✅ Test completed. Original project file restored." -ForegroundColor Green
