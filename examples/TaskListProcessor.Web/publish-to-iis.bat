@echo off
REM Batch file to publish TaskListProcessor Web application to IIS
REM Run this from the project root directory

echo Publishing TaskListProcessor Web Application...
echo.

REM Set variables
set CONFIGURATION=Release
set PUBLISH_PATH=C:\PublishedWebsites\TaskListProcessor

echo Configuration: %CONFIGURATION%
echo Publish Path: %PUBLISH_PATH%
echo.

REM Ensure we're in the correct directory
cd /d "%~dp0"

REM Create publish directory if it doesn't exist
if not exist "%PUBLISH_PATH%" (
    echo Creating publish directory: %PUBLISH_PATH%
    mkdir "%PUBLISH_PATH%"
)

REM Restore packages
echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Error: NuGet restore failed
    pause
    exit /b 1
)

REM Build the application
echo Building application...
dotnet build --configuration %CONFIGURATION% --no-restore
if %ERRORLEVEL% neq 0 (
    echo Error: Build failed
    pause
    exit /b 1
)

REM Publish the application
echo Publishing application...
dotnet publish --configuration %CONFIGURATION% --output "%PUBLISH_PATH%" --no-build --no-restore
if %ERRORLEVEL% neq 0 (
    echo Error: Publish failed
    pause
    exit /b 1
)

echo.
echo ✅ Application published successfully to: %PUBLISH_PATH%
echo.
echo === IIS Deployment Instructions ===
echo 1. Ensure .NET 9.0 Hosting Bundle is installed on the target server
echo 2. Create a new IIS Application or Virtual Directory pointing to: %PUBLISH_PATH%
echo 3. Set the Application Pool to use 'No Managed Code' (.NET Core runs out-of-process)
echo 4. Ensure the Application Pool identity has read access to the publish folder
echo 5. Configure any necessary environment variables or appsettings.json
echo.
echo Publish completed at %DATE% %TIME%
pause
