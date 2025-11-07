using Metrika.Core.Abstractions;
using Metrika.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Console
{
    /// <summary>
    /// Console logger implementation for Metrika measurements with colorful output
    /// </summary>
    public class ConsoleMetrikaLogger : IMetrikaLogger
    {
        private readonly ConsoleColor _defaultColor = System.Console.ForegroundColor;
        private readonly MetrikaColorScheme _colorScheme;
        private readonly bool _useColors;

        /// <summary>
        /// Creates a new console logger with color scheme
        /// </summary>
        /// <param name="colorScheme">Color scheme to use (null = default scheme)</param>
        /// <param name="useColors">Enable colored output (default: true)</param>
        public ConsoleMetrikaLogger(MetrikaColorScheme? colorScheme = null, bool useColors = true)
        {
            _colorScheme = colorScheme ?? MetrikaColorScheme.Default;
            _useColors = useColors;
        }

        public void LogMeasurement(
            MetrikaMeasurementResult result,
            MetrikaLocalization localization,
            MetrikaTimestampFormat timestampFormat)
        {
            var message = BuildMessage(result, localization, timestampFormat);
            var color = DetermineColor(result);

            if (_useColors)
            {
                WriteColoredMessage(color, message);
            }
            else
            {
                System.Console.WriteLine(message);
            }
        }

        private string BuildMessage(
            MetrikaMeasurementResult result,
            MetrikaLocalization localization,
            MetrikaTimestampFormat timestampFormat)
        {
            var parts = new System.Collections.Generic.List<string>();

            // Prefix
            parts.Add($"[{localization.Prefix}]");

            // Timestamp
            if (timestampFormat.Format != null)
            {
                var timestamp = FormatTimestamp(result.Timestamp, timestampFormat);
                if (!string.IsNullOrEmpty(timestamp))
                {
                    parts.Add($"[{timestamp}]");
                }
            }

            // Icon and severity level based on exception/threshold
            string icon;
            if (result.HasException) 
            {
                icon = "[ERROR]";
            }
            else if (result.ThresholdExceeded)
            {
                icon = "[WARN]";
            }
            else
            {
                icon = "[INFO]";
            }
            parts.Add(icon);

            // Operation name and duration
            if (result.HasException) 
            {
                parts.Add($"{result.Name} failed after: {result.ElapsedMilliseconds} {localization.Milliseconds}");

                // Exception details
                if (result.Exception != null)
                {
                    var exceptionType = result.Exception.GetType().Name;
                    var exceptionMessage = result.Exception.Message;
                    parts.Add($"| Exception: {exceptionType}: {exceptionMessage}");
                }
            }
            else
            {
                var durationLabel = result.ThresholdExceeded ? localization.DurationHigh : localization.Duration;
                parts.Add($"{result.Name} {durationLabel}: {result.ElapsedMilliseconds} {localization.Milliseconds}");

                // Threshold info
                if (result.ThresholdMilliseconds > 0 && result.ThresholdExceeded)
                {
                    parts.Add($"({localization.Threshold}: {result.ThresholdMilliseconds} {localization.Milliseconds})");
                }

                // Memory info (only if no exception)
                if (result.MemoryInfo != null)
                {
                    var memInfo = result.MemoryInfo;
                    parts.Add($"| {localization.Memory}: {memInfo.MemoryDeltaMB:+0.00;-0.00} MB");

                    if (memInfo.TotalCollections > 0)
                    {
                        parts.Add($"| {localization.GarbageCollection}: Gen0: {memInfo.Gen0Collections}, Gen1: {memInfo.Gen1Collections}, Gen2: {memInfo.Gen2Collections}");
                    }

                    // Warnings
                    if (memInfo.IsHighMemoryUsage)
                    {
                        parts.Add($"[WARN] {localization.HighMemory}");
                    }
                    else if (memInfo.IsHighGCPressure)
                    {
                        parts.Add($"[WARN] {localization.GCPressure}");
                    }
                }
            }

            return string.Join(" ", parts);
        }

        private string FormatTimestamp(DateTime timestamp, MetrikaTimestampFormat format)
        {
            if (format.Format == null)
                return string.Empty;

            if (format.Format.ToLowerInvariant() == "unix")
            {
                return new DateTimeOffset(timestamp).ToUnixTimeSeconds().ToString();
            }

            return timestamp.ToString(format.Format);
        }

        private ConsoleColor DetermineColor(MetrikaMeasurementResult result)
        {
            // Exception - highest priority (always red)
            if (result.HasException)
            {
                return ConsoleColor.Red;
            }

            // Threshold exceeded
            if (result.ThresholdExceeded)
            {
                return _colorScheme.ThresholdExceededColor;
            }

            // Memory warnings
            if (result.MemoryInfo != null)
            {
                if (result.MemoryInfo.IsHighMemoryUsage)
                {
                    return ConsoleColor.Red;
                }
                if (result.MemoryInfo.IsHighGCPressure)
                {
                    return ConsoleColor.Yellow;
                }
            }

            // Duration-based colors
            if (result.ElapsedMilliseconds > 1000)
            {
                return _colorScheme.SlowColor;
            }
            else if (result.ElapsedMilliseconds > 500)
            {
                return _colorScheme.NormalColor;
            }
            else
            {
                return _colorScheme.FastColor;
            }
        }

        private void WriteColoredMessage(ConsoleColor color, string message)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(message);
            System.Console.ForegroundColor = _defaultColor;
        }
    }
}