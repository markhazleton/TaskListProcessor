# .NET 10.0 Migration Plan

## Executive Summary

### Scenario
Upgrade the TaskListProcessor solution from .NET 9.0 to .NET 10.0 (Preview).

### Scope
- **Total Projects**: 8 projects across the solution
- **Current State**: All projects currently targeting net9.0
- **Target State**: All projects will target net10.0

**Project Breakdown**:
- 3 Core Library Projects (TaskListProcessing, CityWeatherService, CityThingsToDo)
- 2 Application Projects (TaskListProcessor.Web Razor Pages app, TaskListProcessor.Console)
- 3 Test Projects (TaskListProcessing.Tests, CityWeatherService.Tests, CityThingsToDo.Tests)

### Selected Strategy
**Big Bang Strategy** - All projects will be upgraded simultaneously in a single coordinated operation.

**Rationale**:
- Small solution with only 8 projects
- Clear, simple dependency structure with 3 foundation libraries
- Total codebase size is manageable (~16,932 LOC)
- All projects currently on .NET 9.0 (modern framework)
- All required package updates have clear target versions available
- No security vulnerabilities present
- Clean dependency graph with no circular dependencies
- Solution is well-suited for atomic upgrade approach

### Complexity Assessment
**Overall Complexity**: Low

**Justification**:
- All projects are SDK-style projects
- Simple, linear dependency structure (3 leaf libraries → 2 apps + 3 tests)
- Package updates are straightforward (mostly Microsoft.Extensions 9.0.8 → 10.0.0)
- No security vulnerabilities to address
- All test infrastructure packages are already compatible
- Preview release means potential for minor breaking changes, but .NET 9 to 10 typically has minimal breaking changes

### Critical Issues
✅ **No Critical Issues Identified**
- No security vulnerabilities in current package versions
- All required packages have clear upgrade paths to 10.0.0
- No deprecated or obsolete packages in use
- No complex multi-targeting scenarios

### Recommended Approach
**Big Bang Migration** with atomic upgrade of all projects followed by comprehensive testing.

---

## Migration Strategy

### 2.1 Approach Selection

**Chosen Strategy**: Big Bang Strategy

**Justification**:
Based on the solution characteristics, the Big Bang approach is optimal:

1. **Solution Size**: 8 projects is well within the ideal range for Big Bang (<30 projects)
2. **Modern Foundation**: All projects already on .NET 9.0, simplifying the upgrade path
3. **Simple Dependencies**: Clean 3-tier structure:
   - **Tier 1 (Leaf Libraries)**: TaskListProcessing, CityWeatherService, CityThingsToDo (0 dependencies)
   - **Tier 2 (Applications)**: TaskListProcessor.Web, TaskListProcessor.Console (depend on Tier 1)
   - **Tier 3 (Tests)**: 3 test projects (depend on their respective libraries)
4. **Package Compatibility**: All packages have known .NET 10 versions or are already compatible
5. **Low Risk**: No security issues, no complex patterns, straightforward upgrade
6. **Efficient Timeline**: Can complete entire upgrade in single coordinated effort

### 2.2 Dependency-Based Ordering

The dependency graph shows a clear bottom-up structure:

**Foundation Layer (Phase 1)**:
- `CityThingsToDo` (0 dependencies, 3 dependants)
- `CityWeatherService` (0 dependencies, 3 dependants)
- `TaskListProcessing` (0 dependencies, 3 dependants)

These are the leaf nodes and must be migrated first.

**Application Layer (Phase 2)**:
- `TaskListProcessor.Web` (depends on all 3 foundation libraries)
- `TaskListProcessor.Console` (depends on all 3 foundation libraries)

**Test Layer (Phase 3)**:
- `CityThingsToDo.Tests` (depends on CityThingsToDo)
- `CityWeatherService.Tests` (depends on CityWeatherService)
- `TaskListProcessing.Tests` (depends on TaskListProcessing)

**Big Bang Execution**: While these represent logical groupings for understanding dependencies, all projects will be updated simultaneously in a single atomic operation. The dependency structure ensures that when we build after the update, the compilation will succeed because all dependencies are at compatible versions.

### 2.3 Parallel vs Sequential Execution

**Big Bang Strategy - Atomic Operation**:
- All project files will be updated simultaneously to net10.0
- All package references will be updated simultaneously to their 10.0.0 versions
- Single restore operation for entire solution
- Single build operation to identify any issues
- All compilation errors addressed in one pass

This is not a sequential or parallel execution in the traditional sense - it's a single coordinated batch update where all changes are made together, then validated together.

---

## Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

**Migration Phases** (for understanding, not execution order):

**Phase 0: Preparation**
- Verify .NET 10.0 SDK installation
- Ensure all tooling is compatible

**Phase 1: Atomic Upgrade** (single operation)
All projects updated simultaneously:
- Foundation Libraries: TaskListProcessing, CityWeatherService, CityThingsToDo
- Applications: TaskListProcessor.Web, TaskListProcessor.Console
- Tests: All 3 test projects

**Phase 2: Validation**
- Build entire solution
- Fix any compilation errors
- Run all tests

### 3.2 Project Groupings

**Foundation Libraries** (Leaf nodes - no dependencies):
1. `TaskListProcessing` - Core processing library (7,831 LOC, 69 files)
   - Most complex project with extensive Microsoft.Extensions packages
   - Used by: TaskListProcessor.Web, TaskListProcessor.Console, TaskListProcessing.Tests
   
2. `CityWeatherService` - Weather service library (101 LOC, 1 file)
   - Simple service with no package dependencies
   - Used by: TaskListProcessor.Web, TaskListProcessor.Console, CityWeatherService.Tests
   
3. `CityThingsToDo` - Things to do service library (129 LOC, 1 file)
   - Simple service with no package dependencies
   - Used by: TaskListProcessor.Web, TaskListProcessor.Console, CityThingsToDo.Tests

**Application Projects**:
4. `TaskListProcessor.Web` - ASP.NET Core Razor Pages application (5,859 LOC, 34 files)
   - Depends on all 3 foundation libraries
   - Requires Microsoft.Extensions packages upgrade
   
5. `TaskListProcessor.Console` - Console application (617 LOC, 5 files)
   - Depends on all 3 foundation libraries
   - Requires Microsoft.Extensions packages upgrade

**Test Projects**:
6. `TaskListProcessing.Tests` - Unit tests (2,272 LOC, 10 files)
   - Tests TaskListProcessing library
   - Test infrastructure already compatible
   
7. `CityWeatherService.Tests` - Unit tests (69 LOC, 4 files)
   - Tests CityWeatherService library
   - Test infrastructure already compatible
   
8. `CityThingsToDo.Tests` - Unit tests (54 LOC, 4 files)
   - Tests CityThingsToDo library
   - Test infrastructure already compatible

---

## Project-by-Project Migration Plans

### Project: TaskListProcessing

**Current State**
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project dependencies
- Dependants: TaskListProcessor.Web, TaskListProcessor.Console, TaskListProcessing.Tests
- Package Count: 19 packages (13 require updates)
- LOC: 7,831 lines across 69 files

**Target State**
- Target Framework: net10.0
- Updated Packages: 13 packages

**Migration Steps**

1. **Prerequisites**
   - None - this is a leaf node project

2. **Framework Update**
   - Update `TargetFramework` in `src\TaskListProcessing\TaskListProcessing.csproj` from `net9.0` to `net10.0`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Diagnostics.HealthChecks | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Hosting.Abstractions | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Logging | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Logging.Abstractions | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Logging.Console | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.ObjectPool | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Options | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Options.ConfigurationExtensions | 9.0.8 | 10.0.0 | Framework compatibility |
   | Newtonsoft.Json | 13.0.3 | 13.0.4 | Minor version update |
   | System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Text.Json | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Threading.Channels | 9.0.8 | 10.0.0 | Framework compatibility |

   **Packages remaining unchanged** (already compatible):
   - BenchmarkDotNet (0.15.2)
   - BenchmarkDotNet.Diagnostics.Windows (0.15.2)
   - Microsoft.AspNetCore.Http.Abstractions (2.3.0)
   - Microsoft.AspNetCore.Mvc.Core (2.3.0)
   - OpenTelemetry (1.12.0)
   - OpenTelemetry.Extensions.Hosting (1.12.0)

4. **Expected Breaking Changes**
   - **Microsoft.Extensions.* Packages**: Minimal breaking changes expected from 9.0 to 10.0
     - Most APIs remain stable across minor versions
     - Possible obsolete warnings for deprecated APIs
     - May need to update dependency injection registration patterns if new best practices introduced
   
   - **System.Text.Json**: 
     - Check for any serialization behavior changes
     - Verify custom JsonConverter implementations still work
   
   - **ASP.NET Core Abstractions**:
     - Older 2.3.0 packages may have compatibility considerations
     - Verify IHttpContextAccessor and middleware patterns still work
   
   - **General .NET 10 Changes**:
     - Review any new analyzer warnings
     - Check for performance improvements that may affect existing code
   
   Note: Specific breaking changes will be discovered during compilation and testing.

5. **Code Modifications**
   - **Step 1**: Update project file and packages
   - **Step 2**: Restore dependencies
   - **Step 3**: Build and address compilation errors
   - **Step 4**: Review and address any new analyzer warnings
   
   **Areas requiring manual review**:
   - Dependency injection setup (DI patterns may have new recommendations)
   - Health check configurations
   - Logging patterns and structured logging
   - OpenTelemetry integration (ensure compatibility with .NET 10)
   - BenchmarkDotNet compatibility with .NET 10 Preview

6. **Testing Strategy**
   - **Build Validation**: Project must build without errors
   - **Unit Tests**: Run TaskListProcessing.Tests project
   - **Integration**: Verify through dependent projects (Web, Console)
   
   **Key scenarios to verify**:
   - Dependency injection container initialization
   - Health check execution
   - Logging output and structured logging
   - Task processing workflows
   - OpenTelemetry trace collection

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly after package updates
   - [ ] Project builds without errors
   - [ ] Project builds without warnings (or only expected preview warnings)
   - [ ] All unit tests in TaskListProcessing.Tests pass
   - [ ] No performance regressions in benchmarks
   - [ ] OpenTelemetry traces are collected correctly

---

### Project: CityWeatherService

**Current State**
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project dependencies
- Dependants: TaskListProcessor.Web, TaskListProcessor.Console, CityWeatherService.Tests
- Package Count: 0 packages
- LOC: 101 lines in 1 file

**Target State**
- Target Framework: net10.0
- Updated Packages: None (no packages)

**Migration Steps**

1. **Prerequisites**
   - None - this is a leaf node project

2. **Framework Update**
   - Update `TargetFramework` in `src\CityWeatherService\CityWeatherService.csproj` from `net9.0` to `net10.0`

3. **Package Updates**
   - No package updates required (project has no package dependencies)

4. **Expected Breaking Changes**
   - Minimal risk - simple library with no external dependencies
   - May encounter .NET 10 BCL API changes if using newer .NET 9 APIs
   - Check for any compiler warnings about obsolete APIs

5. **Code Modifications**
   - Update project file TargetFramework property
   - Review for any new compiler warnings
   - No package updates needed

6. **Testing Strategy**
   - **Build Validation**: Project must build without errors
   - **Unit Tests**: Run CityWeatherService.Tests project
   - **Integration**: Verify through dependent projects
   
   **Key scenarios to verify**:
   - Weather service functionality
   - Data retrieval patterns

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests in CityWeatherService.Tests pass
   - [ ] No functional regressions

---

### Project: CityThingsToDo

**Current State**
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project dependencies
- Dependants: TaskListProcessor.Web, TaskListProcessor.Console, CityThingsToDo.Tests
- Package Count: 0 packages
- LOC: 129 lines in 1 file

**Target State**
- Target Framework: net10.0
- Updated Packages: None (no packages)

**Migration Steps**

1. **Prerequisites**
   - None - this is a leaf node project

2. **Framework Update**
   - Update `TargetFramework` in `src\CityThingsToDo\CityThingsToDo.csproj` from `net9.0` to `net10.0`

3. **Package Updates**
   - No package updates required (project has no package dependencies)

4. **Expected Breaking Changes**
   - Minimal risk - simple library with no external dependencies
   - May encounter .NET 10 BCL API changes if using newer .NET 9 APIs
   - Check for any compiler warnings about obsolete APIs

5. **Code Modifications**
   - Update project file TargetFramework property
   - Review for any new compiler warnings
   - No package updates needed

6. **Testing Strategy**
   - **Build Validation**: Project must build without errors
   - **Unit Tests**: Run CityThingsToDo.Tests project
   - **Integration**: Verify through dependent projects
   
   **Key scenarios to verify**:
   - Things to do service functionality
   - Data operations

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests in CityThingsToDo.Tests pass
   - [ ] No functional regressions

---

### Project: TaskListProcessor.Web

**Current State**
- Target Framework: net9.0
- Project Type: AspNetCore (Razor Pages)
- Dependencies: TaskListProcessing, CityWeatherService, CityThingsToDo
- Dependants: None
- Package Count: 5 packages (all require updates)
- LOC: 5,859 lines across 34 files

**Target State**
- Target Framework: net10.0
- Updated Packages: 5 packages

**Migration Steps**

1. **Prerequisites**
   - Foundation libraries (TaskListProcessing, CityWeatherService, CityThingsToDo) updated to net10.0

2. **Framework Update**
   - Update `TargetFramework` in `examples\TaskListProcessor.Web\TaskListProcessor.Web.csproj` from `net9.0` to `net10.0`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.Extensions.Logging | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Logging.Console | 9.0.8 | 10.0.0 | Framework compatibility |
   | Newtonsoft.Json | 13.0.3 | 13.0.4 | Minor version update |
   | System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Text.Json | 9.0.8 | 10.0.0 | Framework compatibility |

4. **Expected Breaking Changes**
   - **ASP.NET Core 10.0 Razor Pages**:
     - Minimal changes expected for Razor Pages applications
     - Check Program.cs and Startup.cs (if present) for middleware changes
     - Verify authentication/authorization patterns
     - Check dependency injection registrations
   
   - **Logging Changes**:
     - Console logging format may have minor changes
     - Verify log output patterns
   
   - **JSON Serialization**:
     - System.Text.Json may have behavior changes
     - Check API responses for serialization differences
   
   - **Razor Pages Specifics**:
     - Page model binding patterns
     - Tag helpers compatibility
     - Validation patterns

5. **Code Modifications**
   - Update project file and packages
   - Review Program.cs for any .NET 10 specific changes
   - Check middleware pipeline configuration
   - Verify dependency injection setup
   - Review Razor Pages code for obsolete patterns
   
   **Areas requiring manual review**:
   - Application startup configuration
   - Middleware order and configuration
   - Service registrations
   - Authentication/authorization setup
   - Static file serving
   - Routing configuration
   - API controllers (if any)

6. **Testing Strategy**
   - **Build Validation**: Application must build without errors
   - **Startup Test**: Application must start without errors
   - **Functional Testing**: Manual testing of key user flows
   
   **Key scenarios to verify**:
   - Application starts and serves pages
   - Routing works correctly
   - Dependency injection provides correct services
   - Logging works as expected
   - Static files are served correctly
   - Task list processing functionality
   - Weather service integration
   - Things to do service integration

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Application starts successfully
   - [ ] Home page loads correctly
   - [ ] All Razor Pages render correctly
   - [ ] Navigation works properly
   - [ ] No console errors during runtime
   - [ ] Logging output is correct
   - [ ] All integrations with foundation libraries work

---

### Project: TaskListProcessor.Console

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Console)
- Dependencies: TaskListProcessing, CityWeatherService, CityThingsToDo
- Dependants: None
- Package Count: 5 packages (all require updates)
- LOC: 617 lines across 5 files

**Target State**
- Target Framework: net10.0
- Updated Packages: 5 packages

**Migration Steps**

1. **Prerequisites**
   - Foundation libraries (TaskListProcessing, CityWeatherService, CityThingsToDo) updated to net10.0

2. **Framework Update**
   - Update `TargetFramework` in `examples\TaskListProcessor.Console\TaskListProcessor.Console.csproj` from `net9.0` to `net10.0`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.Extensions.Logging | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.Extensions.Logging.Console | 9.0.8 | 10.0.0 | Framework compatibility |
   | Newtonsoft.Json | 13.0.3 | 13.0.4 | Minor version update |
   | System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Text.Json | 9.0.8 | 10.0.0 | Framework compatibility |

4. **Expected Breaking Changes**
   - **Console Application Patterns**:
     - Check Program.cs for any .NET 10 specific changes
     - Verify host builder configuration
     - Check dependency injection setup
   
   - **Logging**:
     - Console logger format may differ
     - Verify log output patterns
   
   - **JSON Serialization**:
     - System.Text.Json behavior changes
     - Check configuration file parsing

5. **Code Modifications**
   - Update project file and packages
   - Review Program.cs for .NET 10 patterns
   - Check dependency injection configuration
   - Verify logging setup
   - Review command-line argument parsing (if any)
   
   **Areas requiring manual review**:
   - Application entry point (Program.cs/Main method)
   - Host builder configuration
   - Service registrations
   - Configuration file loading
   - Console output formatting

6. **Testing Strategy**
   - **Build Validation**: Application must build without errors
   - **Execution Test**: Application must run without errors
   - **Functional Testing**: Verify console output and functionality
   
   **Key scenarios to verify**:
   - Application starts and runs successfully
   - Command-line arguments processed correctly
   - Dependency injection works
   - Logging output is correct
   - Task processing works
   - Service integrations function properly

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Application executes successfully
   - [ ] Console output is correct
   - [ ] No runtime errors
   - [ ] Logging works as expected
   - [ ] All integrations with foundation libraries work

---

### Project: TaskListProcessing.Tests

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test Project)
- Dependencies: TaskListProcessing
- Dependants: None
- Package Count: 7 packages (3 require updates, 4 compatible)
- LOC: 2,272 lines across 10 files

**Target State**
- Target Framework: net10.0
- Updated Packages: 3 packages

**Migration Steps**

1. **Prerequisites**
   - TaskListProcessing library updated to net10.0

2. **Framework Update**
   - Update `TargetFramework` in `tests\TaskListProcessing.Tests\TaskListProcessing.Tests.csproj` from `net9.0` to `net10.0`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.Extensions.Logging | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Text.Json | 9.0.8 | 10.0.0 | Framework compatibility |

   **Packages remaining unchanged** (already compatible):
   - Microsoft.NET.Test.Sdk (17.14.1)
   - Moq (4.20.72)
   - MSTest.TestAdapter (3.10.4)
   - MSTest.TestFramework (3.10.4)

4. **Expected Breaking Changes**
   - **MSTest Framework**: Already compatible, no changes expected
   - **Moq**: Already compatible, mocking patterns should work unchanged
   - **Test Patterns**: Minimal changes expected
   - May encounter new analyzer warnings in test code

5. **Code Modifications**
   - Update project file and packages
   - Run tests to identify any issues
   - Address any test failures related to framework changes
   
   **Areas requiring manual review**:
   - Test initialization patterns
   - Mocking setups (Moq)
   - Assertion patterns
   - Test data setup

6. **Testing Strategy**
   - **Build Validation**: Test project must build without errors
   - **Test Execution**: All tests must pass
   
   **Key scenarios to verify**:
   - All existing tests pass
   - Test discovery works correctly
   - Mocking framework functions properly
   - Code coverage collection works

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All tests are discovered
   - [ ] All tests pass
   - [ ] No test execution errors
   - [ ] Code coverage can be collected

---

### Project: CityWeatherService.Tests

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test Project)
- Dependencies: CityWeatherService
- Dependants: None
- Package Count: 5 packages (1 requires update, 4 compatible)
- LOC: 69 lines across 4 files

**Target State**
- Target Framework: net10.0
- Updated Packages: 1 package

**Migration Steps**

1. **Prerequisites**
   - CityWeatherService library updated to net10.0

2. **Framework Update**
   - Update `TargetFramework` in `tests\CityWeatherService.Tests\CityWeatherService.Tests.csproj` from `net9.0` to `net10.0`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | Framework compatibility |

   **Packages remaining unchanged** (already compatible):
   - coverlet.collector (6.0.4)
   - Microsoft.NET.Test.Sdk (17.14.1)
   - MSTest.TestAdapter (3.10.4)
   - MSTest.TestFramework (3.10.4)

4. **Expected Breaking Changes**
   - Minimal - test infrastructure is already compatible
   - Simple test project with limited complexity

5. **Code Modifications**
   - Update project file and package
   - Run tests to verify

6. **Testing Strategy**
   - **Build Validation**: Test project must build without errors
   - **Test Execution**: All tests must pass

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All tests pass
   - [ ] Code coverage collection works

---

### Project: CityThingsToDo.Tests

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test Project)
- Dependencies: CityThingsToDo
- Dependants: None
- Package Count: 5 packages (1 requires update, 4 compatible)
- LOC: 54 lines across 4 files

**Target State**
- Target Framework: net10.0
- Updated Packages: 1 package

**Migration Steps**

1. **Prerequisites**
   - CityThingsToDo library updated to net10.0

2. **Framework Update**
   - Update `TargetFramework` in `tests\CityThingsToDo.Tests\CityThingsToDo.Tests.csproj` from `net9.0` to `net10.0`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | Framework compatibility |

   **Packages remaining unchanged** (already compatible):
   - coverlet.collector (6.0.4)
   - Microsoft.NET.Test.Sdk (17.14.1)
   - MSTest.TestAdapter (3.10.4)
   - MSTest.TestFramework (3.10.4)

4. **Expected Breaking Changes**
   - Minimal - test infrastructure is already compatible
   - Simple test project with limited complexity

5. **Code Modifications**
   - Update project file and package
   - Run tests to verify

6. **Testing Strategy**
   - **Build Validation**: Test project must build without errors
   - **Test Execution**: All tests must pass

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All tests pass
   - [ ] Code coverage collection works

---

## Package Update Reference

### Common Package Updates (affecting multiple projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.Extensions.Logging | 9.0.8 | 10.0.0 | 4 projects | Framework compatibility with .NET 10 |
| Microsoft.Extensions.Logging.Console | 9.0.8 | 10.0.0 | 3 projects | Framework compatibility with .NET 10 |
| System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.0 | 6 projects | Framework compatibility with .NET 10 |
| System.Text.Json | 9.0.8 | 10.0.0 | 4 projects | Framework compatibility with .NET 10 |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | 3 projects | Minor bug fix update |

**Projects using Microsoft.Extensions.Logging**:
- TaskListProcessing
- TaskListProcessor.Web
- TaskListProcessor.Console
- TaskListProcessing.Tests

**Projects using System.Diagnostics.DiagnosticSource** (all projects):
- TaskListProcessing
- TaskListProcessor.Web
- TaskListProcessor.Console
- TaskListProcessing.Tests
- CityWeatherService.Tests
- CityThingsToDo.Tests

### TaskListProcessing-Specific Updates

The TaskListProcessing project has the most extensive package update requirements:

| Package | Current | Target | Reason |
|---------|---------|--------|---------|
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.0 | DI framework compatibility |
| Microsoft.Extensions.Diagnostics.HealthChecks | 9.0.8 | 10.0.0 | Health check compatibility |
| Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions | 9.0.8 | 10.0.0 | Health check compatibility |
| Microsoft.Extensions.Hosting.Abstractions | 9.0.8 | 10.0.0 | Hosting compatibility |
| Microsoft.Extensions.Logging.Abstractions | 9.0.8 | 10.0.0 | Logging compatibility |
| Microsoft.Extensions.ObjectPool | 9.0.8 | 10.0.0 | Object pooling compatibility |
| Microsoft.Extensions.Options | 9.0.8 | 10.0.0 | Options pattern compatibility |
| Microsoft.Extensions.Options.ConfigurationExtensions | 9.0.8 | 10.0.0 | Configuration compatibility |
| System.Threading.Channels | 9.0.8 | 10.0.0 | Channels compatibility |

### Packages Remaining Unchanged

These packages are already compatible with .NET 10:

**Test Infrastructure**:
- Microsoft.NET.Test.Sdk (17.14.1)
- MSTest.TestAdapter (3.10.4)
- MSTest.TestFramework (3.10.4)
- coverlet.collector (6.0.4)
- Moq (4.20.72)

**Other Compatible Packages**:
- BenchmarkDotNet (0.15.2)
- BenchmarkDotNet.Diagnostics.Windows (0.15.2)
- Microsoft.AspNetCore.Http.Abstractions (2.3.0)
- Microsoft.AspNetCore.Mvc.Core (2.3.0)
- OpenTelemetry (1.12.0)
- OpenTelemetry.Extensions.Hosting (1.12.0)

---

## Breaking Changes Catalog

### .NET 10 Preview Breaking Changes

**Note**: As .NET 10 is currently in preview, the breaking changes list may evolve. Monitor the official .NET breaking changes documentation during the migration.

### Expected Breaking Change Categories

#### 1. ASP.NET Core Changes
- **Razor Pages**: Minimal changes expected, verify tag helpers and page models
- **Middleware**: Check middleware registration order and configuration
- **Dependency Injection**: Verify service lifetime configurations
- **Hosting**: Review Program.cs and WebApplication builder patterns

#### 2. Microsoft.Extensions.* Libraries
- **Logging**: 
  - Console logger format may have changed
  - Structured logging patterns may have new recommendations
  - Log level filtering behavior
  
- **Options Pattern**:
  - Options validation may have new features
  - Configuration binding behavior
  
- **Dependency Injection**:
  - Service descriptor registration patterns
  - Lifetime scope handling
  
- **Health Checks**:
  - Health check registration patterns
  - Health check response format

#### 3. System.Text.Json
- **Serialization Behavior**:
  - Default serialization options may have changed
  - Null handling behavior
  - Number handling
  - DateTime format handling
  
- **JsonConverter**:
  - Custom converter implementations may need updates
  - Verify polymorphic serialization

#### 4. BCL (Base Class Library)
- **Obsolete APIs**: Check for newly obsolete APIs in .NET 10
- **Performance Improvements**: Some APIs may have behavior changes for performance
- **New Analyzers**: .NET 10 may introduce new code analyzers with warnings

#### 5. Compiler Changes
- **C# Language Version**: Verify C# version compatibility
- **Nullable Reference Types**: Enhanced nullable analysis
- **Pattern Matching**: New pattern matching features may affect existing code

### Project-Specific Breaking Change Risks

#### High Risk: TaskListProcessing
- Most complex project with extensive Microsoft.Extensions usage
- OpenTelemetry integration needs verification
- BenchmarkDotNet compatibility with preview runtime
- Health checks configuration
- Custom middleware and DI patterns

#### Medium Risk: TaskListProcessor.Web (Razor Pages)
- ASP.NET Core hosting changes
- Middleware pipeline configuration
- Authentication/authorization patterns
- Static file serving
- Razor Pages compilation

#### Medium Risk: TaskListProcessor.Console
- Host builder patterns
- Console logging format
- Configuration loading

#### Low Risk: Foundation Libraries (CityWeatherService, CityThingsToDo)
- Simple libraries with no external dependencies
- Minimal surface area for breaking changes

#### Low Risk: Test Projects
- Test infrastructure already compatible
- Isolated from runtime changes

### Mitigation Strategies

1. **Incremental Build Validation**:
   - After framework and package updates, build entire solution
   - Address compilation errors systematically
   - Pay attention to new analyzer warnings

2. **Testing at Each Layer**:
   - Run unit tests after fixing compilation errors
   - Verify application startup and basic functionality
   - Test integration points between projects

3. **Reference Documentation**:
   - Consult official .NET 10 breaking changes documentation
   - Review package-specific migration guides
   - Check GitHub issues for known problems

4. **Gradual Code Updates**:
   - Address errors in dependency order (libraries first)
   - Update obsolete API usage
   - Modernize patterns to .NET 10 best practices

---

## Implementation Timeline

### Phase 0: Preparation

**Estimated Duration**: 15-30 minutes

**Activities**:
1. Verify .NET 10.0 SDK is installed
   - Run `dotnet --list-sdks` to confirm .NET 10 SDK presence
   - If missing, download from https://dotnet.microsoft.com/download/dotnet/10.0
   
2. Verify Git repository state
   - Confirm on `upgrade-to-NET10` branch
   - Ensure clean working directory
   
3. Backup current state
   - Repository is already in source control
   - Create a tag for current .NET 9 state if desired: `git tag v-net9-baseline`

**Success Criteria**:
- [ ] .NET 10.0 SDK installed and verified
- [ ] On correct Git branch
- [ ] Ready to begin atomic upgrade

### Phase 1: Atomic Upgrade

**Estimated Duration**: 2-4 hours

**Operations** (performed as single coordinated batch):

1. **Update all project files to net10.0**:
   - `src\TaskListProcessing\TaskListProcessing.csproj`
   - `src\CityWeatherService\CityWeatherService.csproj`
   - `src\CityThingsToDo\CityThingsToDo.csproj`
   - `examples\TaskListProcessor.Web\TaskListProcessor.Web.csproj`
   - `examples\TaskListProcessor.Console\TaskListProcessor.Console.csproj`
   - `tests\TaskListProcessing.Tests\TaskListProcessing.Tests.csproj`
   - `tests\CityWeatherService.Tests\CityWeatherService.Tests.csproj`
   - `tests\CityThingsToDo.Tests\CityThingsToDo.Tests.csproj`

2. **Update all package references** (see Package Update Reference section for complete list):
   - Microsoft.Extensions.* packages: 9.0.8 → 10.0.0
   - System.*.* packages: 9.0.8 → 10.0.0
   - Newtonsoft.Json: 13.0.3 → 13.0.4

3. **Restore and build**:
   - `dotnet restore TaskListProcessing.sln`
   - `dotnet build TaskListProcessing.sln`

4. **Fix compilation errors**:
   - Address breaking changes systematically
   - Reference Breaking Changes Catalog
   - Update obsolete API usage
   - Fix namespace changes
   - Resolve type compatibility issues

5. **Rebuild and verify**:
   - `dotnet build TaskListProcessing.sln --no-incremental`
   - Confirm 0 errors

**Deliverables**:
- All 8 projects targeting net10.0
- All packages updated to target versions
- Solution builds with 0 errors

**Success Criteria**:
- [ ] All project files updated to `<TargetFramework>net10.0</TargetFramework>`
- [ ] All package references updated to 10.0.0 (or 13.0.4 for Newtonsoft.Json)
- [ ] `dotnet restore` completes successfully
- [ ] `dotnet build TaskListProcessing.sln` succeeds with 0 errors
- [ ] No blocking warnings (some preview warnings acceptable)

### Phase 2: Test Validation

**Estimated Duration**: 1-2 hours

**Operations**:

1. **Execute all test projects**:
   ```bash
   dotnet test TaskListProcessing.sln --no-build
   ```

2. **Address test failures**:
   - Investigate and fix failing tests
   - Update test expectations if .NET 10 behavior changed legitimately
   - Fix product code if tests reveal actual bugs

3. **Manual testing**:
   - Start TaskListProcessor.Web and verify basic functionality
   - Run TaskListProcessor.Console and verify output
   - Spot-check key user workflows

4. **Review warnings**:
   - Address any actionable warnings
   - Document any acceptable preview warnings

**Deliverables**:
- All tests passing
- Applications start and run correctly
- No critical warnings

**Success Criteria**:
- [ ] All unit tests pass (100% pass rate)
- [ ] TaskListProcessor.Web starts without errors
- [ ] TaskListProcessor.Web home page loads correctly
- [ ] TaskListProcessor.Console executes successfully
- [ ] No runtime exceptions during smoke testing
- [ ] Logging works as expected
- [ ] Service integrations function properly

---

## Risk Management

### 5.1 High-Risk Changes

**Strategy Risk Factors**: 
- Big Bang approach means all projects change simultaneously
- If critical issue found, must address before proceeding
- Preview release may have unexpected issues

| Project | Risk Level | Risk Description | Mitigation |
|---------|-----------|------------------|------------|
| TaskListProcessing | Medium-High | Complex library with 13 package updates, extensive Microsoft.Extensions usage, OpenTelemetry integration | Thorough testing, verify all health checks and logging, test benchmarks, consult .NET 10 breaking changes docs |
| TaskListProcessor.Web | Medium | ASP.NET Core Razor Pages application, middleware configuration, authentication patterns | Start application and test all pages, verify routing and DI, test authentication flows |
| TaskListProcessor.Console | Low-Medium | Console app with DI and logging | Run application and verify output, check logging format |
| CityWeatherService | Low | Simple library, no dependencies | Quick build and test verification |
| CityThingsToDo | Low | Simple library, no dependencies | Quick build and test verification |
| All Test Projects | Low | Test infrastructure already compatible | Run tests and verify pass rate |

### 5.2 Mitigation Strategies

#### For TaskListProcessing (Highest Risk)
- **Build First**: Address all compilation errors before proceeding
- **Test Thoroughly**: Run complete test suite multiple times
- **Benchmark Verification**: Run BenchmarkDotNet tests to check for performance changes
- **OpenTelemetry**: Verify traces are collected correctly with .NET 10 runtime
- **Health Checks**: Test all registered health checks
- **Logging Review**: Verify structured logging output format

#### For ASP.NET Core Application (TaskListProcessor.Web)
- **Startup Testing**: Verify application starts without errors
- **Page Verification**: Test all Razor Pages render correctly
- **Middleware**: Verify middleware pipeline executes in correct order
- **Static Files**: Check static file serving
- **DI Verification**: Test that all services resolve correctly
- **End-to-End Testing**: Run through key user scenarios

#### For All Projects
- **Incremental Commits**: Commit after successful build
- **Detailed Commit Messages**: Document what was changed and why
- **Test Coverage**: Maintain or improve test coverage
- **Documentation**: Update any framework-specific documentation

### 5.3 Contingency Plans

#### If Compilation Fails After Package Updates

**Scenario**: Build fails with numerous errors after package updates

**Response**:
1. Review errors systematically by project (start with foundation libraries)
2. Consult official .NET 10 migration documentation
3. Check package release notes for breaking changes
4. Search GitHub issues for known problems
5. If blocking issue found in preview, consider:
   - Reporting issue to .NET team
   - Waiting for next preview release
   - Finding workaround

**Rollback**: Revert changes via Git (`git reset --hard HEAD`)

#### If Tests Fail After Migration

**Scenario**: Solution builds but tests fail

**Response**:
1. Categorize failures (setup issues vs. product bugs vs. expected behavior changes)
2. Fix product code bugs
3. Update test expectations if .NET 10 behavior legitimately changed
4. Verify no regressions in actual functionality
5. Add new tests if gaps identified

**Rollback**: If test failures indicate critical regression, revert migration

#### If Application Won't Start

**Scenario**: TaskListProcessor.Web or .Console won't start after migration

**Response**:
1. Check startup exception details
2. Verify DI container configuration
3. Check middleware registration
4. Verify configuration file format
5. Review application logs
6. Test in isolation (create minimal repro)

**Rollback**: Revert migration if startup issue can't be resolved

#### If Performance Regression Detected

**Scenario**: BenchmarkDotNet or manual testing shows performance degradation

**Response**:
1. Run benchmarks to quantify regression
2. Profile application to identify hotspots
3. Check for known .NET 10 performance issues
4. Consider .NET 10 is preview - may improve in future releases
5. Report to .NET team if significant regression

**Decision Point**: Evaluate whether to proceed or wait for GA release

#### If Preview Runtime Issues Arise

**Scenario**: Encounter unexpected runtime behavior or bugs

**Response**:
1. Create minimal reproduction
2. Search .NET GitHub repo for existing issues
3. Report new issue with repro steps
4. Consider workarounds
5. Evaluate continuing with migration vs. waiting for fix

**Decision Point**: Assess severity and decide to continue or defer migration

---

## Testing and Validation Strategy

### 6.1 Phase-by-Phase Testing

#### After Atomic Upgrade (Phase 1)

**Build Verification**:
- [ ] Clean build: `dotnet clean && dotnet build TaskListProcessing.sln`
- [ ] No compilation errors
- [ ] Review and categorize warnings
- [ ] Verify all 8 projects build successfully

**Quick Validation**:
- [ ] Check project dependency resolution
- [ ] Verify no package restore warnings
- [ ] Confirm target framework is net10.0 in all projects

#### Test Execution (Phase 2)

**Unit Test Validation**:
- [ ] Run all tests: `dotnet test TaskListProcessing.sln`
- [ ] Verify test discovery (all tests found)
- [ ] 100% test pass rate
- [ ] Review test execution output for anomalies
- [ ] Verify code coverage collection works

**Per-Project Test Validation**:
```bash
dotnet test tests\TaskListProcessing.Tests\TaskListProcessing.Tests.csproj
dotnet test tests\CityWeatherService.Tests\CityWeatherService.Tests.csproj
dotnet test tests\CityThingsToDo.Tests\CityThingsToDo.Tests.csproj
```

### 6.2 Application Validation

#### TaskListProcessor.Web (Razor Pages Application)

**Startup Testing**:
- [ ] `dotnet run --project examples\TaskListProcessor.Web`
- [ ] Application starts without exceptions
- [ ] No errors in console output
- [ ] Health check endpoint responds (if configured)

**Functional Testing**:
- [ ] Home page loads correctly
- [ ] All Razor Pages render without errors
- [ ] Navigation between pages works
- [ ] Static assets load (CSS, JS, images)
- [ ] Forms submit correctly
- [ ] Validation works as expected
- [ ] Services are injected correctly (TaskListProcessing, CityWeatherService, CityThingsToDo)

**Integration Testing**:
- [ ] Task list processing functionality works
- [ ] Weather service integration functions
- [ ] Things to do service integration functions
- [ ] Logging output is correct and formatted properly
- [ ] No console errors during usage

#### TaskListProcessor.Console Application

**Execution Testing**:
- [ ] `dotnet run --project examples\TaskListProcessor.Console`
- [ ] Application executes without exceptions
- [ ] Console output is correct
- [ ] All services resolve correctly

**Functional Testing**:
- [ ] Command-line arguments processed correctly (if applicable)
- [ ] Task processing executes successfully
- [ ] Service integrations work (CityWeatherService, CityThingsToDo)
- [ ] Logging output is correct
- [ ] Exit code is 0 on success

### 6.3 Smoke Tests

Quick validation checks to run after any code changes:

**Build Smoke Test** (< 2 minutes):
```bash
dotnet clean
dotnet build TaskListProcessing.sln
# Verify: 0 errors
```

**Test Smoke Test** (< 5 minutes):
```bash
dotnet test TaskListProcessing.sln --no-build
# Verify: All tests pass
```

**Application Smoke Test** (< 2 minutes):
```bash
# Terminal 1: Start Web app
dotnet run --project examples\TaskListProcessor.Web

# Terminal 2: Test Console app
dotnet run --project examples\TaskListProcessor.Console

# Verify: Both start/run without errors
```

### 6.4 Comprehensive Validation

Before marking the migration complete:

#### Technical Validation
- [ ] All 8 projects target net10.0
- [ ] All package versions match target versions
- [ ] No package dependency conflicts
- [ ] Solution builds with 0 errors
- [ ] All unit tests pass (100% pass rate)
- [ ] Applications start and run correctly
- [ ] No runtime exceptions during testing

#### Quality Validation
- [ ] Code quality maintained (no new code smells introduced)
- [ ] Test coverage maintained or improved
- [ ] No performance regressions (run benchmarks if applicable)
- [ ] Logging output is clear and useful
- [ ] No security warnings

#### Documentation Validation
- [ ] README updated with .NET 10 requirements
- [ ] Any framework-specific docs updated
- [ ] Breaking changes documented
- [ ] Migration notes captured

#### Operational Validation
- [ ] CI/CD pipeline works with .NET 10 (if applicable)
- [ ] Deployment process validated (if applicable)
- [ ] Development environment setup documented

---

## Source Control Strategy

### Branching Strategy

**Main Upgrade Branch**: `upgrade-to-NET10`
- Already created and active
- All migration work happens on this branch
- Keeps main/master branch stable during migration

**Branch Lifecycle**:
1. **During Migration**: All commits go to `upgrade-to-NET10`
2. **After Completion**: Merge to main via pull request
3. **Post-Merge**: Delete `upgrade-to-NET10` branch (if desired)

### Commit Strategy

**Approach**: Single atomic commit for entire Big Bang upgrade

**Rationale**:
- Big Bang strategy means all changes happen together
- Single commit represents the complete upgrade
- Easier to revert if issues discovered
- Clearer history ("Upgraded to .NET 10")

**Recommended Commit Sequence**:

**Commit 1: Complete .NET 10 Upgrade** (after Phase 1 & 2 complete)
```bash
git add .
git commit -m "Upgrade solution to .NET 10.0

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
```

**Alternative: Phased Commits** (if needed for traceability)

If you prefer more granular history:

**Commit 1: Update project target frameworks**
```bash
git add **/*.csproj
git commit -m "Update target framework to net10.0 in all projects"
```

**Commit 2: Update package references**
```bash
git add **/*.csproj
git commit -m "Update NuGet packages to .NET 10 compatible versions

- Microsoft.Extensions.* packages: 9.0.8 → 10.0.0
- System.* packages: 9.0.8 → 10.0.0
- Newtonsoft.Json: 13.0.3 → 13.0.4"
```

**Commit 3: Fix breaking changes**
```bash
git add .
git commit -m "Fix compilation errors from .NET 10 breaking changes

- Updated obsolete API usage
- Fixed namespace changes
- Updated DI registration patterns
- All projects now build successfully"
```

**Commit 4: Update documentation**
```bash
git add README.md docs/
git commit -m "Update documentation for .NET 10 requirements"
```

### Commit Message Format

Use clear, descriptive commit messages following this template:

```
<Type>: <Short summary>

<Detailed description>
- Bullet point 1
- Bullet point 2

<Affected components>
```

**Types**: 
- `feat`: New feature or capability
- `fix`: Bug fix
- `chore`: Maintenance task (like framework upgrade)
- `docs`: Documentation changes
- `test`: Test changes

### Review and Merge Process

#### Pull Request Checklist

When creating PR to merge `upgrade-to-NET10` into main:

**PR Title**: "Upgrade solution to .NET 10.0"

**PR Description**:
```markdown
## Summary
Upgrades all projects in the solution from .NET 9.0 to .NET 10.0 (Preview).

## Changes
- Updated all 8 projects to target net10.0
- Updated Microsoft.Extensions.* packages to 10.0.0
- Updated System.* packages to 10.0.0
- Updated Newtonsoft.Json to 13.0.4
- Fixed breaking changes related to framework upgrade

## Testing
- [x] All projects build successfully
- [x] All unit tests pass (100% pass rate)
- [x] TaskListProcessor.Web starts and functions correctly
- [x] TaskListProcessor.Console executes successfully
- [x] No runtime exceptions during smoke testing

## Migration Details
See `.github/upgrades/plan.md` for complete migration plan.
See `.github/upgrades/assessment.md` for analysis details.

## Breaking Changes
- None identified - all changes are internal framework updates

## Risks
- .NET 10 is currently in Preview - may have unexpected issues
- Recommend thorough testing in staging environment before production

## Rollback Plan
If issues arise, revert this PR/merge to return to .NET 9.0.
```

**Review Checklist**:
- [ ] All tests pass in CI/CD pipeline
- [ ] Code builds successfully
- [ ] No new warnings introduced
- [ ] Documentation updated
- [ ] Migration plan and assessment reviewed
- [ ] At least one approval from team member

#### Integration Validation Steps

After merging to main:

1. **CI/CD Verification**:
   - Verify CI/CD pipeline runs successfully
   - Check build output for warnings
   - Verify test execution
   - Check deployment process (if applicable)

2. **Environment Testing**:
   - Deploy to staging environment
   - Run smoke tests
   - Run integration tests
   - Monitor for issues

3. **Monitoring**:
   - Watch application metrics
   - Monitor error logs
   - Check performance metrics
   - Verify no regressions

### Handling Merge Conflicts

If merge conflicts occur when integrating to main:

1. **Sync with main**:
   ```bash
   git checkout upgrade-to-NET10
   git fetch origin
   git merge origin/main
   ```

2. **Resolve conflicts**:
   - Prioritize upgrade changes for project files
   - Carefully merge code changes
   - Re-run tests after resolution

3. **Verify resolution**:
   ```bash
   dotnet build TaskListProcessing.sln
   dotnet test TaskListProcessing.sln
   ```

---

## Success Criteria

### 9.1 Strategy-Specific Success Criteria

**Big Bang Strategy Completion Criteria**:
- [ ] All 8 projects upgraded in single coordinated operation
- [ ] All changes committed together (or in logical sequence)
- [ ] No intermediate states left in codebase
- [ ] Complete validation performed before marking complete

### 9.2 Technical Success Criteria

**Framework Migration**:
- [ ] All 8 projects target net10.0
- [ ] All project files correctly specify `<TargetFramework>net10.0</TargetFramework>`
- [ ] No multi-targeting remnants from .NET 9

**Package Updates**:
- [ ] All Microsoft.Extensions.* packages updated to 10.0.0
- [ ] All System.* packages updated to 10.0.0
- [ ] Newtonsoft.Json updated to 13.0.4
- [ ] No security vulnerabilities in dependencies
- [ ] No package version conflicts

**Build Quality**:
- [ ] `dotnet restore TaskListProcessing.sln` completes successfully
- [ ] `dotnet build TaskListProcessing.sln` succeeds with 0 errors
- [ ] No blocking warnings (some preview warnings acceptable)
- [ ] All 8 projects build individually
- [ ] Solution builds with `--no-incremental` flag

**Test Coverage**:
- [ ] All automated tests pass (100% pass rate)
- [ ] No tests skipped or ignored
- [ ] Test execution time within expected range
- [ ] Code coverage maintained or improved

**Application Functionality**:
- [ ] TaskListProcessor.Web starts without errors
- [ ] TaskListProcessor.Web pages render correctly
- [ ] TaskListProcessor.Console executes successfully
- [ ] No runtime exceptions during smoke testing
- [ ] Logging output is correct

### 9.3 Quality Criteria

**Code Quality**:
- [ ] Code quality maintained (no new code smells)
- [ ] Follows .NET 10 best practices where applicable
- [ ] No technical debt introduced
- [ ] Obsolete API usage updated

**Testing Quality**:
- [ ] Test coverage maintained or improved
- [ ] All test projects use .NET 10 compatible test frameworks
- [ ] Test execution is reliable and repeatable
- [ ] No flaky tests introduced

**Documentation Quality**:
- [ ] README updated with .NET 10 requirements
- [ ] SDK installation instructions current
- [ ] Any breaking changes documented
- [ ] Migration notes captured in `.github/upgrades/` folder

**Performance**:
- [ ] No significant performance regressions
- [ ] Benchmark results acceptable (if benchmarks exist)
- [ ] Application startup time within expected range
- [ ] Memory usage within expected range

### 9.4 Process Criteria

**Migration Process**:
- [ ] Big Bang Strategy principles followed throughout migration
- [ ] All phases completed in order (Preparation → Atomic Upgrade → Validation)
- [ ] Atomic operation completed successfully (all projects updated together)
- [ ] Comprehensive testing performed

**Source Control**:
- [ ] All work performed on `upgrade-to-NET10` branch
- [ ] Commits are clear and descriptive
- [ ] Commit message follows team conventions
- [ ] Git history is clean and understandable

**Collaboration**:
- [ ] Pull request created with detailed description
- [ ] Code reviewed by team member(s)
- [ ] Review checklist completed
- [ ] Approval obtained before merge

**Operational Readiness**:
- [ ] CI/CD pipeline updated for .NET 10 (if needed)
- [ ] Deployment scripts updated (if needed)
- [ ] Development environment setup documented
- [ ] Team informed of .NET 10 requirements

### 9.5 Final Acceptance Criteria

**Before Marking Migration Complete**:

1. **All Technical Criteria Met**:
   - Every checkbox in section 9.2 is checked
   - No outstanding compilation or runtime errors
   - All tests pass consistently

2. **All Quality Criteria Met**:
   - Every checkbox in section 9.3 is checked
   - Documentation is current
   - No known regressions

3. **All Process Criteria Met**:
   - Every checkbox in section 9.4 is checked
   - Source control is clean
   - Team is informed

4. **Validation Complete**:
   - Full solution validation performed (section 6.3)
   - Smoke tests pass
   - Applications run correctly
   - No issues outstanding

5. **Sign-Off**:
   - Team lead approval
   - Technical review complete
   - Ready for merge to main branch

**Acceptance Statement**:

"The .NET 10 migration is complete when all projects build, all tests pass, applications run correctly, documentation is updated, and the code is merged to the main branch following team review and approval."

---

## Next Steps After Migration

### Post-Migration Activities

1. **Monitor Production** (if deployed):
   - Watch error logs for unexpected issues
   - Monitor performance metrics
   - Track user-reported issues
   - Be ready to rollback if critical issues arise

2. **Take Advantage of .NET 10 Features**:
   - Review .NET 10 release notes for new features
   - Consider adopting new performance improvements
   - Explore new APIs and patterns
   - Update code to use new language features (if C# version updated)

3. **Clean Up**:
   - Remove any temporary workarounds
   - Delete upgrade branches after successful merge
   - Archive migration documentation
   - Update team knowledge base

4. **Continuous Improvement**:
   - Collect lessons learned
   - Document any issues encountered
   - Update migration procedures for future upgrades
   - Share knowledge with team

### Future Considerations

- **Preview to GA**: When .NET 10 reaches General Availability, verify no changes needed
- **.NET 11 Planning**: Begin planning next upgrade cycle
- **Dependency Updates**: Stay current with package updates
- **Security Patches**: Monitor for security updates

---

## Additional Resources

### Official Documentation
- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [ASP.NET Core 10.0 Migration](https://learn.microsoft.com/en-us/aspnet/core/migration/9x-to-10)
- [What's New in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)

### Package Documentation
- [Microsoft.Extensions.Logging 10.0](https://www.nuget.org/packages/Microsoft.Extensions.Logging/10.0.0)
- [System.Text.Json 10.0](https://www.nuget.org/packages/System.Text.Json/10.0.0)

### Tools
- [.NET SDK Downloads](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Upgrade Assistant](https://dotnet.microsoft.com/platform/upgrade-assistant)

### Migration Files Location
- Assessment: `.github/upgrades/assessment.md`
- Plan: `.github/upgrades/plan.md` (this document)
- Branch: `upgrade-to-NET10`

---

**Document Version**: 1.0  
**Created**: 2025-01-29  
**Target Framework**: .NET 10.0 (Preview)  
**Strategy**: Big Bang  
**Estimated Total Effort**: 4-8 hours  
**Risk Level**: Low-Medium