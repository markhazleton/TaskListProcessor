# PowerShell script to publish TaskListProcessor Web application to IIS
# Run this script from the project root directory

param(
    [string]$Configuration = "Release",
    [string]$PublishPath = "C:\PublishedWebsites\TaskListProcessor",
    [switch]$Force
)

Write-Host "Publishing TaskListProcessor Web Application..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Publish Path: $PublishPath" -ForegroundColor Yellow

# Ensure we're in the correct directory
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (Test-Path $projectPath)) {
    Write-Error "Project path not found: $projectPath"
    exit 1
}

Set-Location $projectPath

# Create publish directory if it doesn't exist
if (-not (Test-Path $PublishPath)) {
    Write-Host "Creating publish directory: $PublishPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null
}

# Clean existing files if Force is specified
if ($Force -and (Test-Path $PublishPath)) {
    Write-Host "Cleaning existing files..." -ForegroundColor Yellow
    Remove-Item "$PublishPath\*" -Recurse -Force -ErrorAction SilentlyContinue
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "NuGet restore failed"
    exit 1
}

# Build the application
Write-Host "Building application..." -ForegroundColor Cyan
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Cyan
dotnet publish TaskListProcessor.Web.csproj --configuration $Configuration --output $PublishPath --no-build --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed"
    exit 1
}

Write-Host "✅ Application published successfully to: $PublishPath" -ForegroundColor Green

# Display next steps for IIS deployment
Write-Host "`n=== IIS Deployment Instructions ===" -ForegroundColor Magenta
Write-Host "1. Ensure .NET 9.0 Hosting Bundle is installed on the target server" -ForegroundColor White
Write-Host "2. Create a new IIS Application or Virtual Directory pointing to: $PublishPath" -ForegroundColor White
Write-Host "3. Set the Application Pool to use 'No Managed Code' (.NET Core runs out-of-process)" -ForegroundColor White
Write-Host "4. Ensure the Application Pool identity has read access to the publish folder" -ForegroundColor White
Write-Host "5. Configure any necessary environment variables or appsettings.json" -ForegroundColor White
Write-Host "`nPublish completed at $(Get-Date)" -ForegroundColor Green
