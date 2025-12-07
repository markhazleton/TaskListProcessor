# .NET 10.0 Migration Tasks

## Overview

This task list executes the Big Bang upgrade of the TaskListProcessor solution from .NET 9.0 to .NET 10.0 (Preview) across all 8 projects in a single atomic operation, followed by comprehensive validation and a single commit. All tasks are automatable and reference the migration plan for details.

**Progress**: 4/4 tasks complete (100%) ![100%](https://progress-bar.xyz/100)

## Tasks

### [✓] TASK-001: Verify .NET 10.0 SDK prerequisites *(Completed: 2025-12-07 12:31)*
**References**: Plan §Phase 0

- [✓] (1) Verify .NET 10.0 SDK is installed (`dotnet --list-sdks`)
- [✓] (2) SDK version 10.0.x or higher is present (**Verify**)

### [✓] TASK-002: Atomic upgrade of all projects to .NET 10.0 *(Completed: 2025-12-07 12:33)*
**References**: Plan §Phase 1, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update `<TargetFramework>` to `net10.0` in all 8 project files per Plan §Phase 1
- [✓] (2) Update all package references to target versions per Plan §Package Update Reference
- [✓] (3) Restore all dependencies (`dotnet restore TaskListProcessing.sln`)
- [✓] (4) Build the solution (`dotnet build TaskListProcessing.sln`)
- [✓] (5) Fix all compilation errors, warnings, and address breaking changes per Plan §Breaking Changes Catalog
- [✓] (6) Rebuild solution and confirm 0 errors and 0 warnings (**Verify**)

### [✓] TASK-003: Run and validate all test projects *(Completed: 2025-12-07 12:34)*
**References**: Plan §Phase 2, Plan §Testing and Validation Strategy

- [✓] (1) Run all test projects: `dotnet test TaskListProcessing.sln --no-build`
- [✓] (2) Fix any test failures related to the upgrade (see Plan §Breaking Changes Catalog)
- [✓] (3) Re-run all tests after fixes
- [✓] (4) All tests pass with 0 failures (**Verify**)

### [✓] TASK-004: Commit all upgrade changes *(Completed: 2025-12-07 12:40)*
**References**: Plan §Source Control Strategy

- [✓] (1) Commit all changes with message:  
      "Upgrade solution to .NET 10.0

      - Updated all 8 projects from net9.0 to net10.0
      - Updated Microsoft.Extensions.* packages from 9.0.8 to 10.0.0
      - Updated System.*.* packages from 9.0.8 to 10.0.0
      - Updated Newtonsoft.Json from 13.0.3 to 13.0.4
      - Fixed compilation errors related to breaking changes
      - All tests passing
      - Applications start and run correctly

      Projects updated:
      - TaskListProcessing
      - CityWeatherService
      - CityThingsToDo
      - TaskListProcessor.Web
      - TaskListProcessor.Console
      - TaskListProcessing.Tests
      - CityWeatherService.Tests
      - CityThingsToDo.Tests"
- [✓] (2) Changes committed successfully (**Verify**)
