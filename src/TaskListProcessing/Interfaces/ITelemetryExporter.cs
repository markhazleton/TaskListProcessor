using TaskListProcessing.Telemetry;

namespace TaskListProcessing.Interfaces
{

    /// <summary>
    /// Interface for telemetry exporters.
    /// </summary>
    public interface ITelemetryExporter
    {
        /// <summary>
        /// Exports telemetry data to an external system.
        /// </summary>
        /// <param name="telemetry">The telemetry data to export.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the export operation.</returns>
        Task ExportAsync(IEnumerable<TaskTelemetry> telemetry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the name of the exporter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets whether the exporter is enabled.
        /// </summary>
        bool IsEnabled { get; }
    }
}
