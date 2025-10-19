using Metrika.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core.Abstractions
{
    /// <summary>
    /// Defines a contract for custom loggers that handle performance measurement output.
    /// Implementations of this interface determine how and where the collected metrics
    /// are recorded — such as console output, file storage, external telemetry systems,
    /// or application monitoring dashboards.
    /// </summary>
    public interface IMetrikaLogger
    {
        /// <summary>
        /// Logs a completed performance measurement result, including duration,
        /// threshold status, and optional memory statistics.
        /// </summary>
        /// <param name="result">
        /// The <see cref="MetrikaMeasurementResult"/> instance containing performance data
        /// such as elapsed time, memory usage, and threshold information.
        /// </param>
        /// <param name="localization">
        /// Optional localization preference used to format messages or labels.
        /// If not provided, the global configuration from <c>MetrikaCore</c> is used.
        /// </param>
        /// <param name="timestampFormat">
        /// Optional timestamp formatting configuration. When set, determines how
        /// timestamps are included in the log entry; otherwise, defaults to the global
        /// timestamp format configuration.
        /// </param>
        void LogMeasurement(
            MetrikaMeasurementResult result,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null);
    }
}
