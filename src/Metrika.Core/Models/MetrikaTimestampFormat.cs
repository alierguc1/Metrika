using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core.Models
{
        /// <summary>
        /// Timestamp format configuration
        /// </summary>
        public class MetrikaTimestampFormat
        {
            /// <summary>
            /// DateTime format string (uses standard .NET DateTime format specifiers)
            /// </summary>
            public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

            /// <summary>
            /// Whether to show timestamp
            /// </summary>
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// Default format: "yyyy-MM-dd HH:mm:ss.fff" (2025-01-15 14:32:18.456)
            /// </summary>
            public static MetrikaTimestampFormat Default => new()
            {
                Format = "yyyy-MM-dd HH:mm:ss.fff",
                Enabled = true
            };

            /// <summary>
            /// Short format: "HH:mm:ss" (14:32:18)
            /// </summary>
            public static MetrikaTimestampFormat Short => new()
            {
                Format = "HH:mm:ss",
                Enabled = true
            };

            /// <summary>
            /// Time only with milliseconds: "HH:mm:ss.fff" (14:32:18.456)
            /// </summary>
            public static MetrikaTimestampFormat TimeWithMs => new()
            {
                Format = "HH:mm:ss.fff",
                Enabled = true
            };

            /// <summary>
            /// ISO 8601 format: "yyyy-MM-ddTHH:mm:ss.fffZ" (2025-01-15T14:32:18.456Z)
            /// </summary>
            public static MetrikaTimestampFormat ISO8601 => new()
            {
                Format = "yyyy-MM-ddTHH:mm:ss.fffZ",
                Enabled = true
            };

            /// <summary>
            /// Unix timestamp (seconds since epoch): "1705329138"
            /// </summary>
            public static MetrikaTimestampFormat UnixTimestamp => new()
            {
                Format = "unix",
                Enabled = true
            };

            /// <summary>
            /// Date only: "2025-01-15"
            /// </summary>
            public static MetrikaTimestampFormat DateOnly => new()
            {
                Format = "yyyy-MM-dd",
                Enabled = true
            };

            /// <summary>
            /// Disabled (no timestamp)
            /// </summary>
            public static MetrikaTimestampFormat Disabled => new()
            {
                Enabled = false
            };

            /// <summary>
            /// Custom format
            /// </summary>
            /// <param name="format">Custom DateTime format string</param>
            /// <example>
            /// <code>
            /// // US format with AM/PM
            /// var format = MetrikaTimestampFormat.Custom("MM/dd/yyyy hh:mm:ss tt");
            /// 
            /// // European format
            /// var format = MetrikaTimestampFormat.Custom("dd.MM.yyyy HH:mm:ss");
            /// </code>
            /// </example>
            public static MetrikaTimestampFormat Custom(string format) => new()
            {
                Format = format,
                Enabled = true
            };
        }
    }