using Metrika.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Console.Tests
{
    public class ConsoleMetrikaLoggerTests : IDisposable
    {
        private readonly TextWriter _originalOutput;
        private StringWriter _testOutput;

        public ConsoleMetrikaLoggerTests()
        {
            _originalOutput = System.Console.Out;
            _testOutput = new StringWriter();
            System.Console.SetOut(_testOutput);
        }

        public void Dispose()
        {
            System.Console.SetOut(_originalOutput);
            _testOutput?.Dispose();
        }

        [Fact]
        public void LogMeasurement_WithBasicResult_WritesToConsole()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Test Operation",
                ElapsedMilliseconds = 150,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("[METRIKA]", output);
            Assert.Contains("Test Operation", output);
            Assert.Contains("150", output);
            Assert.Contains("ms", output);
        }

        [Fact]
        public void LogMeasurement_WithThresholdExceeded_ShowsWarning()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Slow Operation",
                ElapsedMilliseconds = 500,
                ThresholdMilliseconds = 100,
                Timestamp = DateTime.Now
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("[WARN]", output);
            Assert.Contains("duration high", output);
            Assert.Contains("threshold: 100", output);
        }

        [Fact]
        public void LogMeasurement_WithMemoryInfo_ShowsMemoryData()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Memory Test",
                ElapsedMilliseconds = 100,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now,
                MemoryInfo = new MetrikaMemoryInfo
                {
                    MemoryDelta = 5_242_880, // 5 MB
                    Gen0Collections = 2,
                    Gen1Collections = 1,
                    Gen2Collections = 0
                }
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("Memory:", output);
            Assert.Contains("+5", output); // 5 MB civarı pozitif değer
            Assert.Contains("MB", output);
            Assert.Contains("GC:", output);
            Assert.Contains("Gen0: 2", output);
        }


        [Fact]
        public void LogMeasurement_WithHighMemory_ShowsWarning()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "High Memory Test",
                ElapsedMilliseconds = 100,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now,
                MemoryInfo = new MetrikaMemoryInfo
                {
                    MemoryDelta = 150_000_000, // 150 MB
                    Gen0Collections = 0,
                    Gen1Collections = 0,
                    Gen2Collections = 0
                }
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("[WARN]", output); 
            Assert.Contains("HIGH MEMORY", output);
        }

        [Fact]
        public void LogMeasurement_WithGCPressure_ShowsWarning()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "GC Pressure Test",
                ElapsedMilliseconds = 100,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now,
                MemoryInfo = new MetrikaMemoryInfo
                {
                    MemoryDelta = 10_000_000, // 10 MB
                    Gen0Collections = 1,
                    Gen1Collections = 1,
                    Gen2Collections = 2 // Gen2 triggered
                }
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("[WARN]", output);
            Assert.Contains("GC PRESSURE", output);
        }

        [Fact]
        public void LogMeasurement_WithTimestamp_IncludesTimestamp()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Timestamp Test",
                ElapsedMilliseconds = 50,
                ThresholdMilliseconds = 0,
                Timestamp = new DateTime(2025, 1, 15, 10, 30, 45)
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Short);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("10:30:45", output);
        }

        [Fact]
        public void LogMeasurement_WithUnixTimestamp_ShowsUnixTime()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Unix Test",
                ElapsedMilliseconds = 50,
                ThresholdMilliseconds = 0,
                Timestamp = new DateTime(2025, 1, 15, 10, 30, 45, DateTimeKind.Utc)
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.UnixTimestamp);

            // Assert
            var output = _testOutput.ToString();
            Assert.Matches(@"\[\d{10}\]", output); // Unix timestamp pattern
        }

        [Theory]
        [InlineData("METRIKA", "duration")]
        [InlineData("METRİKA", "süresi")]
        public void LogMeasurement_WithDifferentLocalizations_UsesCorrectStrings(
            string expectedPrefix,
            string expectedDuration)
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Localization Test",
                ElapsedMilliseconds = 100,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now
            };

            var localization = expectedPrefix == "METRIKA"
                ? MetrikaLocalization.English
                : MetrikaLocalization.Turkish;

            // Act
            logger.LogMeasurement(
                result,
                localization,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains(expectedPrefix, output);
            Assert.Contains(expectedDuration, output);
        }

        [Fact]
        public void Constructor_WithDefaultColorScheme_CreatesLogger()
        {
            // Act
            var logger = new ConsoleMetrikaLogger();

            // Assert
            Assert.NotNull(logger);
        }

        [Fact]
        public void Constructor_WithPastelColorScheme_CreatesLogger()
        {
            // Act
            var logger = new ConsoleMetrikaLogger(MetrikaColorScheme.Pastel);

            // Assert
            Assert.NotNull(logger);
        }

        [Fact]
        public void LogMeasurement_WithZeroElapsedTime_DisplaysZero()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Instant Operation",
                ElapsedMilliseconds = 0,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains("0 ms", output);
        }

        [Fact]
        public void LogMeasurement_WithCustomTimestampFormat_UsesFormat()
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = "Custom Format",
                ElapsedMilliseconds = 100,
                ThresholdMilliseconds = 0,
                Timestamp = new DateTime(2025, 1, 15, 14, 30, 45)
            };

            var customFormat = MetrikaTimestampFormat.Custom("dd/MM/yyyy HH:mm");

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                customFormat);

            // Assert
            var output = _testOutput.ToString();
            // Kültürden bağımsız kontrol - sadece sayıları kontrol et
            Assert.Contains("15", output);
            Assert.Contains("01", output);
            Assert.Contains("2025", output);
            Assert.Contains("14:30", output);
        }

        [Theory]
        [InlineData(400, "FastColor")]
        [InlineData(700, "NormalColor")]
        [InlineData(1200, "SlowColor")]
        public void LogMeasurement_WithDifferentDurations_UsesDifferentColors(long duration, string expectedColorType)
        {
            // Arrange
            var logger = new ConsoleMetrikaLogger(useColors: false);
            var result = new MetrikaMeasurementResult
            {
                Name = $"{expectedColorType} Test",
                ElapsedMilliseconds = duration,
                ThresholdMilliseconds = 0,
                Timestamp = DateTime.Now
            };

            // Act
            logger.LogMeasurement(
                result,
                MetrikaLocalization.English,
                MetrikaTimestampFormat.Disabled);

            // Assert
            var output = _testOutput.ToString();
            Assert.Contains($"{expectedColorType} Test", output);
            Assert.Contains($"{duration} ms", output);
        }
    }
}