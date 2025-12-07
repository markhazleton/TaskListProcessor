# TaskListProcessor Web Application - IIS Deployment Guide

## Overview

This guide explains how to deploy the TaskListProcessor Web Application to a Windows Server running IIS.

## Prerequisites

### Server Requirements

- Windows Server 2016 or later / Windows 10 or later
- Internet Information Services (IIS) 7.0 or later
- .NET 10.0 Hosting Bundle installed

### .NET 10.0 Hosting Bundle Installation

1. Download the .NET 10.0 Hosting Bundle from: <https://dotnet.microsoft.com/download/dotnet/10.0>
2. Run the installer on the target server
3. Restart IIS after installation: `iisreset`

## Publishing the Application

### Option 1: Using PowerShell Script (Recommended)

```powershell
# Navigate to project directory
cd "c:\GitHub\MarkHazleton\TaskListProcessor\examples\TaskListProcessor.Web"

# Run the publish script
.\publish-to-iis.ps1

# Or with custom parameters
.\publish-to-iis.ps1 -Configuration Release -PublishPath "C:\PublishedWebsites\TaskListProcessor" -Force
```

### Option 2: Using Batch File

```cmd
cd "c:\GitHub\MarkHazleton\TaskListProcessor\examples\TaskListProcessor.Web"
publish-to-iis.bat
```

### Option 3: Using Visual Studio

1. Right-click the project in Solution Explorer
2. Select "Publish..."
3. Choose "IISProfile" publish profile
4. Click "Publish"

### Option 4: Using .NET CLI

```cmd
cd "c:\GitHub\MarkHazleton\TaskListProcessor\examples\TaskListProcessor.Web"
dotnet publish --configuration Release --output "C:\PublishedWebsites\TaskListProcessor"
```

## IIS Configuration

### 1. Create Application Pool

1. Open IIS Manager
2. Right-click "Application Pools" → "Add Application Pool"
3. Name: `TaskListProcessor`
4. .NET CLR Version: `No Managed Code`
5. Managed Pipeline Mode: `Integrated`
6. Start Immediately: `True`

### 2. Configure Application Pool Identity

1. Right-click the "TaskListProcessor" application pool
2. Select "Advanced Settings"
3. Set "Identity" to `ApplicationPoolIdentity` (recommended) or custom account
4. Ensure the identity has read access to `C:\PublishedWebsites\TaskListProcessor`

### 3. Create IIS Application

1. Right-click "Default Web Site" (or your desired site)
2. Select "Add Application"
3. Alias: `TaskListProcessor`
4. Application Pool: `TaskListProcessor`
5. Physical Path: `C:\PublishedWebsites\TaskListProcessor`
6. Click "OK"

### 4. Configure Permissions

```cmd
# Grant IIS_IUSRS read access to the application folder
icacls "C:\PublishedWebsites\TaskListProcessor" /grant "IIS_IUSRS:(OI)(CI)R" /T

# Grant Application Pool identity full access to logs folder (if needed)
icacls "C:\PublishedWebsites\TaskListProcessor\logs" /grant "IIS AppPool\TaskListProcessor:(OI)(CI)F" /T
```

## Configuration Files

### web.config

The `web.config` file is automatically generated during publish and includes:

- ASP.NET Core Module configuration
- Security headers
- Static file caching
- Compression settings
- Error page handling

### appsettings.Production.json

Production-specific settings:

- Logging configuration
- Performance settings
- Task processor configuration
- Error handling settings

## Environment Variables (Optional)

You can set environment variables in IIS Manager:

1. Select your application in IIS Manager
2. Double-click "Configuration Editor"
3. Section: `system.webServer/aspNetCore`
4. Click on "environmentVariables" → "..." button
5. Add variables as needed:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `ASPNETCORE_URLS`: `http://*:80;https://*:443`

## SSL/HTTPS Configuration

1. Obtain an SSL certificate
2. Bind the certificate to your site in IIS Manager
3. Update `appsettings.Production.json` if needed
4. Consider configuring HTTPS redirection

## Monitoring and Troubleshooting

### Log Locations

- **Stdout logs**: `C:\PublishedWebsites\TaskListProcessor\logs\stdout_*.log`
- **Application logs**: Check Windows Event Viewer
- **IIS logs**: `C:\inetpub\logs\LogFiles\W3SVC1\`

### Common Issues

1. **500.19 Error**: Check web.config syntax and file permissions
2. **500.30 Error**: .NET Core runtime not installed or wrong version
3. **502.5 Error**: Application failed to start - check stdout logs
4. **403 Error**: Permission issues - check folder permissions

### Verification

After deployment, verify the application is working:

1. Browse to: `http://yourserver/TaskListProcessor`
2. Test all demo scenarios
3. Check streaming functionality
4. Verify error handling

## Maintenance

### Updating the Application

1. Stop the application pool
2. Run the publish script again
3. Start the application pool

### Backup Strategy

- Backup `C:\PublishedWebsites\TaskListProcessor` folder
- Backup IIS configuration
- Document any custom configuration changes

## Performance Optimization

- Enable IIS compression (already configured in web.config)
- Configure response caching
- Monitor application pool recycling
- Set appropriate timeout values
- Consider using a reverse proxy for load balancing

## Security Considerations

- Keep .NET runtime updated
- Review and customize security headers in web.config
- Implement proper authentication if needed
- Use HTTPS in production
- Regularly review access permissions
- Monitor logs for suspicious activity

## Support

For issues specific to the TaskListProcessor application, check:

- Application logs in the `logs` folder
- GitHub repository: <https://github.com/markhazleton/TaskListProcessor>
- Project documentation
