using Metrika.Core.Abstractions;
using Metrika.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core
{
    /// <summary>
    /// Provides a fluent and extensible API for performance and memory measurement
    /// in synchronous and asynchronous code. Supports localization, timestamp formatting,
    /// and custom logger integrations for detailed metric reporting.
    /// </summary>
    public static class MetrikaCore
    {
        private static readonly List<IMetrikaLogger> _loggers = new();
        private static MetrikaLocalization _localization = MetrikaLocalization.English;
        private static MetrikaTimestampFormat _timestampFormat = MetrikaTimestampFormat.Disabled;
        private static bool _trackMemoryByDefault = false;

        #region Configuration

        /// <summary>
        /// Registers a custom logger that implements the <see cref="IMetrikaLogger"/> interface.
        /// </summary>
        /// <param name="logger">The logger instance to be registered.</param>

        public static void RegisterLogger(IMetrikaLogger logger)
        {
            if (logger != null && !_loggers.Contains(logger))
            {
                _loggers.Add(logger);
            }
        }
        /// <summary>
        /// Removes all registered loggers from the <strong>Metrika</strong> logging pipeline.
        /// </summary>
        public static void ClearLoggers()
        {
            _loggers.Clear();
        }

        /// <summary>
        /// Configures the timestamp format for all subsequent performance logs.
        /// </summary>
        /// <param name="format">
        /// The timestamp format to use. If null, timestamp logging will be disabled.
        /// </param>
        public static void ConfigureTimestampFormat(MetrikaTimestampFormat? format = null)
        {
            _timestampFormat = format ?? MetrikaTimestampFormat.Disabled;
        }

        /// <summary>
        /// Configures the localization language for metric messages and labels.
        /// </summary>
        /// <param name="localization">
        /// The localization option to use. Defaults to English if not specified.
        /// </param>
        public static void ConfigureLocalization(MetrikaLocalization? localization = null)
        {
            _localization = localization ?? MetrikaLocalization.English;
        }

        /// <summary>
        /// Enables or disables automatic memory tracking for all measurements.
        /// </summary>
        /// <param name="trackMemory">
        /// When true, memory usage (delta and GC collection counts) will be measured.
        /// </param>
        public static void ConfigureMemoryTracking(bool trackMemory)
        {
            _trackMemoryByDefault = trackMemory;
        }

        #endregion

        #region Async
        /// <summary>
        /// Measures execution time and optional memory usage of an asynchronous function
        /// that returns a result. Logs the outcome using built-in or custom loggers.
        /// </summary>
        /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
        /// <param name="task">The task to be measured.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">An optional threshold in milliseconds that, when exceeded, marks the measurement as critical. </param>
        /// <param name="logger">Optional <see cref="ILogger"/> instance for structured logging.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The awaited result of the original task.</returns>
        public static async Task<T> MetrikaAsync<T>(
            this Task<T> task,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            var shouldTrackMemory = trackMemory ?? _trackMemoryByDefault;
            MetrikaMemoryInfo? memoryInfo = null;

            if (shouldTrackMemory)
            {
                memoryInfo = BeginMemoryTracking();
            }

            var sw = Stopwatch.StartNew();
            var result = await task;
            sw.Stop();

            if (shouldTrackMemory && memoryInfo != null)
            {
                EndMemoryTracking(memoryInfo);
            }

            var measurementResult = new MetrikaMeasurementResult
            {
                Name = name,
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                ThresholdMilliseconds = thresholdMs,
                MemoryInfo = memoryInfo,
                Timestamp = DateTime.Now
            };

            LogResult(measurementResult, logger, localization, timestampFormat);
            return result!;
        }

        /// <summary>
        /// Measures execution time and optional memory usage of an asynchronous task
        /// that does not return a value. Logs the outcome using built-in or custom loggers.
        /// </summary>
        /// <param name="task">The task to be measured.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold in milliseconds for performance alerts.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> instance for structured logging.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        public static async Task MetrikaAsync(
            this Task task,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            var shouldTrackMemory = trackMemory ?? _trackMemoryByDefault;
            MetrikaMemoryInfo? memoryInfo = null;

            if (shouldTrackMemory)
            {
                memoryInfo = BeginMemoryTracking();
            }

            var sw = Stopwatch.StartNew();
            await task;
            sw.Stop();

            if (shouldTrackMemory && memoryInfo != null)
            {
                EndMemoryTracking(memoryInfo);
            }

            var measurementResult = new MetrikaMeasurementResult
            {
                Name = name,
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                ThresholdMilliseconds = thresholdMs,
                MemoryInfo = memoryInfo,
                Timestamp = DateTime.Now
            };

            LogResult(measurementResult, logger, localization, timestampFormat);
        }

        #endregion

        #region Sync - Actual Measurement

        /// <summary>
        /// Measures the execution time and optional memory usage of a synchronous function
        /// that returns a value. Returns the original result while logging performance metrics.
        /// </summary>
        /// <typeparam name="T">The return type of the function being measured.</typeparam>
        /// <param name="func">The function to execute and measure.</param>
        /// <param name="name">A descriptive name for the measurement entry.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running operations.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The return value of the measured function.</returns>
        public static T Metrika<T>(
            this Func<T> func,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            var shouldTrackMemory = trackMemory ?? _trackMemoryByDefault;
            MetrikaMemoryInfo? memoryInfo = null;

            if (shouldTrackMemory)
            {
                memoryInfo = BeginMemoryTracking();
            }

            var sw = Stopwatch.StartNew();
            var result = func();
            sw.Stop();

            if (shouldTrackMemory && memoryInfo != null)
            {
                EndMemoryTracking(memoryInfo);
            }

            var measurementResult = new MetrikaMeasurementResult
            {
                Name = name,
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                ThresholdMilliseconds = thresholdMs,
                MemoryInfo = memoryInfo,
                Timestamp = DateTime.Now
            };

            LogResult(measurementResult, logger, localization, timestampFormat);
            return result!;
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of a synchronous action
        /// that does not return a value. Logs the measurement results to registered loggers.
        /// </summary>
        /// <param name="action">The action to execute and measure.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running operations.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        public static void Metrika(
            this Action action,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            var shouldTrackMemory = trackMemory ?? _trackMemoryByDefault;
            MetrikaMemoryInfo? memoryInfo = null;

            if (shouldTrackMemory)
            {
                memoryInfo = BeginMemoryTracking();
            }

            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            if (shouldTrackMemory && memoryInfo != null)
            {
                EndMemoryTracking(memoryInfo);
            }

            var measurementResult = new MetrikaMeasurementResult
            {
                Name = name,
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                ThresholdMilliseconds = thresholdMs,
                MemoryInfo = memoryInfo,
                Timestamp = DateTime.Now
            };

            LogResult(measurementResult, logger, localization, timestampFormat);
        }

        #endregion


        #region Memory Tracking
        /// <summary>
        /// Initializes a new memory tracking session by forcing a full garbage collection
        /// and capturing baseline memory and GC metrics.
        /// </summary>
        /// <returns>A <see cref="MetrikaMemoryInfo"/> object representing the initial memory state.</returns>
        private static MetrikaMemoryInfo BeginMemoryTracking()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return new MetrikaMemoryInfo
            {
                MemoryDelta = -GC.GetTotalMemory(false),
                Gen0Collections = -GC.CollectionCount(0),
                Gen1Collections = -GC.CollectionCount(1),
                Gen2Collections = -GC.CollectionCount(2)
            };
        }
        /// <summary>
        /// Finalizes the memory tracking session by calculating memory deltas and GC collection counts.
        /// </summary>
        /// <param name="memoryInfo">The initial memory tracking snapshot to update with final metrics.</param>
        private static void EndMemoryTracking(MetrikaMemoryInfo memoryInfo)
        {
            memoryInfo.MemoryDelta += GC.GetTotalMemory(false);
            memoryInfo.Gen0Collections += GC.CollectionCount(0);
            memoryInfo.Gen1Collections += GC.CollectionCount(1);
            memoryInfo.Gen2Collections += GC.CollectionCount(2);
        }

        #endregion

        #region Internal Logging

        /// <summary>
        /// Logs the measurement result to both standard and custom loggers.
        /// Handles localization, timestamp formatting, and threshold-based severity.
        /// </summary>
        /// <param name="result">The measurement result to log.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> for structured output.</param>
        /// <param name="localization">Optional localization override for this log entry.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this log entry.</param>
        private static void LogResult(
            MetrikaMeasurementResult result,
            ILogger? logger,
            MetrikaLocalization? localization,
            MetrikaTimestampFormat? timestampFormat)
        {
            if (logger != null)
            {
                var logLevel = result.ThresholdExceeded ? LogLevel.Warning : LogLevel.Information;
                var icon = result.ThresholdExceeded ? "⚠️" : "⏱️";
                var durationText = result.ThresholdExceeded ? "duration high" : "duration";

                if (result.MemoryInfo != null)
                {
                    logger.Log(logLevel,
                        "{Icon} {Name} {DurationText}: {Elapsed} ms | Memory: {MemoryDelta:+0.00;-0.00} MB | GC: Gen0: {Gen0}, Gen1: {Gen1}, Gen2: {Gen2}",
                        icon, result.Name, durationText, result.ElapsedMilliseconds,
                        result.MemoryInfo.MemoryDeltaMB,
                        result.MemoryInfo.Gen0Collections,
                        result.MemoryInfo.Gen1Collections,
                        result.MemoryInfo.Gen2Collections);
                }
                else
                {
                    logger.Log(logLevel, "{Icon} {Name} {DurationText}: {Elapsed} ms",
                        icon, result.Name, durationText, result.ElapsedMilliseconds);
                }
            }

            if (_loggers.Count == 0)
                return;

            var loc = localization ?? _localization;
            var format = timestampFormat ?? _timestampFormat;

            foreach (var customLogger in _loggers)
            {
                customLogger.LogMeasurement(result, loc, format);
            }
        }

        #endregion

        #region IQueryable Extensions

        /// <summary>
        /// Measures the execution time and optional memory usage of ToList() operation on IQueryable.
        /// This extension allows performance tracking of LINQ query materialization.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to materialize and measure.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>A materialized List containing all elements from the queryable.</returns>
        public static List<T> ToListWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<List<T>>(() => queryable.ToList())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of ToArray() operation on IQueryable.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to materialize and measure.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>A materialized array containing all elements from the queryable.</returns>
        public static T[] ToArrayWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T[]>(() => queryable.ToArray())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of First() operation on IQueryable.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to execute First() on.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The first element from the queryable.</returns>
        public static T FirstWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T>(() => queryable.First())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of FirstOrDefault() operation on IQueryable.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to execute FirstOrDefault() on.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The first element or default value from the queryable.</returns>
        public static T? FirstOrDefaultWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T?>(() => queryable.FirstOrDefault())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of Count() operation on IQueryable.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to count.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The count of elements in the queryable.</returns>
        public static int CountWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<int>(() => queryable.Count())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of Any() operation on IQueryable.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to check.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>True if the queryable contains any elements, otherwise false.</returns>
        public static bool AnyWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<bool>(() => queryable.Any())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }


        /// <summary>
        /// Measures the execution time and optional memory usage of Single() operation on IQueryable.
        /// Use this when you expect exactly one element in the query result.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to execute Single() on.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The single element from the queryable.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the query returns zero or more than one element.</exception>
        public static T SingleWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T>(() => queryable.Single())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of SingleOrDefault() operation on IQueryable.
        /// Returns null if no element is found, throws if more than one element exists.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to execute SingleOrDefault() on.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The single element from the queryable, or default(T) if no element is found.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the query returns more than one element.</exception>
        public static T? SingleOrDefaultWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T?>(() => queryable.SingleOrDefault())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of Last() operation on IQueryable.
        /// The query must be ordered for this operation to be meaningful.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to execute Last() on.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The last element from the queryable.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the query returns no elements.</exception>
        public static T LastWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T>(() => queryable.Last())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of LastOrDefault() operation on IQueryable.
        /// Returns null if no element is found. The query should be ordered for meaningful results.
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to execute LastOrDefault() on.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The last element from the queryable, or default(T) if no element is found.</returns>
        public static T? LastOrDefaultWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<T?>(() => queryable.LastOrDefault())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }

        /// <summary>
        /// Measures the execution time and optional memory usage of LongCount() operation on IQueryable.
        /// Use this instead of Count() when dealing with very large datasets (> 2 billion records).
        /// </summary>
        /// <typeparam name="T">The element type of the queryable sequence.</typeparam>
        /// <param name="queryable">The IQueryable to count.</param>
        /// <param name="name">A descriptive name for the measured operation.</param>
        /// <param name="thresholdMs">Optional threshold for marking long-running queries.</param>
        /// <param name="logger">Optional logger instance for structured metric output.</param>
        /// <param name="localization">Optional localization override for this measurement.</param>
        /// <param name="timestampFormat">Optional timestamp format override for this measurement.</param>
        /// <param name="trackMemory">Optional flag to enable or disable memory tracking for this call.</param>
        /// <returns>The count of elements in the queryable as a long (64-bit integer).</returns>
        public static long LongCountWithMetrika<T>(
            this IQueryable<T> queryable,
            string name,
            int thresholdMs = 0,
            ILogger? logger = null,
            MetrikaLocalization? localization = null,
            MetrikaTimestampFormat? timestampFormat = null,
            bool? trackMemory = null)
        {
            return new Func<long>(() => queryable.LongCount())
                .Metrika(name, thresholdMs, logger, localization, timestampFormat, trackMemory);
        }


        #endregion

    }
}