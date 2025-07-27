# TaskListProcessor Web - Setup Script
# This script sets up the npm build environment and builds the frontend assets

Write-Host "Setting up TaskListProcessor Web frontend build system..." -ForegroundColor Green

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "Node.js version: $nodeVersion" -ForegroundColor Cyan
}
catch {
    Write-Host "Error: Node.js is not installed. Please install Node.js from https://nodejs.org/" -ForegroundColor Red
    exit 1
}

# Check if npm is installed
try {
    $npmVersion = npm --version
    Write-Host "npm version: $npmVersion" -ForegroundColor Cyan
}
catch {
    Write-Host "Error: npm is not installed. Please install npm." -ForegroundColor Red
    exit 1
}

# Install npm dependencies
Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
npm install

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to install npm dependencies." -ForegroundColor Red
    exit 1
}

# Build frontend assets
Write-Host "Building frontend assets..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to build frontend assets." -ForegroundColor Red
    exit 1
}

Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host "You can now run 'dotnet run' to start the application." -ForegroundColor Cyan
Write-Host "For development, use 'npm run dev' to watch for changes." -ForegroundColor Cyan
