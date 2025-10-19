using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core.Models
{
    /// <summary>
    /// Represents the result of a performance measurement
    /// </summary>
    public class MetrikaMeasurementResult
    {
        /// <summary>
        /// Name of the measured operation
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Elapsed time in milliseconds
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Threshold value in milliseconds (0 = no threshold)
        /// </summary>
        public int ThresholdMilliseconds { get; set; }

        /// <summary>
        /// Whether the threshold was exceeded
        /// </summary>
        public bool ThresholdExceeded => ThresholdMilliseconds > 0 && ElapsedMilliseconds > ThresholdMilliseconds;

        /// <summary>
        /// Memory tracking information (null if not tracked)
        /// </summary>
        public MetrikaMemoryInfo? MemoryInfo { get; set; }

        /// <summary>
        /// Timestamp when the measurement was completed
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Performance level based on elapsed time
        /// </summary>
        public PerformanceLevel Level
        {
            get
            {
                if (ThresholdExceeded)
                    return PerformanceLevel.ThresholdExceeded;
                if (ElapsedMilliseconds > 1000)
                    return PerformanceLevel.Slow;
                if (ElapsedMilliseconds > 500)
                    return PerformanceLevel.Normal;
                return PerformanceLevel.Fast;
            }
        }
    }

    /// <summary>
    /// Performance level classification
    /// </summary>
    public enum PerformanceLevel
    {
        /// <summary>
        /// Fast operation (&lt;500ms)
        /// </summary>
        Fast,

        /// <summary>
        /// Normal operation (500-1000ms)
        /// </summary>
        Normal,

        /// <summary>
        /// Slow operation (&gt;1000ms)
        /// </summary>
        Slow,

        /// <summary>
        /// Threshold exceeded
        /// </summary>
        ThresholdExceeded
    }
}