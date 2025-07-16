using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskListProcessing.Core;
using TaskListProcessing.Models;
using TaskListProcessing.Options;

namespace TaskListProcessing.Tests
{
    [TestClass]
    public class ProgressReportingDebugTests
    {
        private Mock<ILogger> _mockLogger;
        private TaskListProcessorOptions _options;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _options = new TaskListProcessorOptions
            {
                MaxConcurrentTasks = 2,
                EnableProgressReporting = true,
                EnableDetailedTelemetry = true
            };
        }

        [TestMethod]
        public async Task Debug_ProgressReporting_SingleTask()
        {
            // Arrange
            var progressReports = new List<TaskProgress>();
            var progressReporter = new Progress<TaskProgress>(p =>
            {
                Debug.WriteLine($"Progress: {p.CompletedTasks}/{p.TotalTasks} - {p.CurrentTaskName}");
                progressReports.Add(p);
            });

            var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
            await processor.InitializeAsync();

            var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
            {
                ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; }
            };

            // Act
            await processor.ProcessTasksAsync(tasks, progressReporter);

            // Assert
            Debug.WriteLine($"Total progress reports: {progressReports.Count}");
            foreach (var report in progressReports)
            {
                Debug.WriteLine($"  - {report.CompletedTasks}/{report.TotalTasks} - {report.CurrentTaskName}");
            }

            Assert.IsTrue(progressReports.Count > 0);
        }

        [TestMethod]
        public async Task Debug_ProgressReporting_TwoTasks()
        {
            // Arrange
            var progressReports = new List<TaskProgress>();
            var progressReporter = new Progress<TaskProgress>(p =>
            {
                Debug.WriteLine($"Progress: {p.CompletedTasks}/{p.TotalTasks} - {p.CurrentTaskName}");
                progressReports.Add(p);
            });

            var processor = new TaskListProcessorEnhanced("TestProcessor", _mockLogger.Object, _options);
            await processor.InitializeAsync();

            var tasks = new Dictionary<string, Func<CancellationToken, Task<object?>>>
            {
                ["task1"] = async ct => { await Task.Delay(50, ct); return "result1"; },
                ["task2"] = async ct => { await Task.Delay(100, ct); return "result2"; }
            };

            // Act
            await processor.ProcessTasksAsync(tasks, progressReporter);

            // Assert
            Debug.WriteLine($"Total progress reports: {progressReports.Count}");
            foreach (var report in progressReports)
            {
                Debug.WriteLine($"  - {report.CompletedTasks}/{report.TotalTasks} - {report.CurrentTaskName}");
            }

            Assert.IsTrue(progressReports.Count > 0);
        }
    }
}
