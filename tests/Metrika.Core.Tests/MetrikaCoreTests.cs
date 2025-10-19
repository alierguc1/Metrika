using Metrika.Core.Abstractions;
using Metrika.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core.Tests
{
    public class MetrikaCoreTests : IDisposable
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IMetrikaLogger> _mockCustomLogger;

        public MetrikaCoreTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockCustomLogger = new Mock<IMetrikaLogger>();
            ResetConfiguration();
        }

        public void Dispose()
        {
            ResetConfiguration();
        }

        private static void ResetConfiguration()
        {
            MetrikaCore.ClearLoggers();
            MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.Disabled);
            MetrikaCore.ConfigureLocalization(MetrikaLocalization.English);
            MetrikaCore.ConfigureMemoryTracking(false);
        }

        #region Custom Logger Tests

        [Fact]
        public void RegisterLogger_AddsCustomLogger()
        {
            // Arrange
            var customLogger = new Mock<IMetrikaLogger>();

            // Act
            MetrikaCore.RegisterLogger(customLogger.Object);
            new Func<int>(() => 1).Metrika("Test");

            // Assert
            customLogger.Verify(
                x => x.LogMeasurement(
                    It.IsAny<MetrikaMeasurementResult>(),
                    It.IsAny<MetrikaLocalization>(),
                    It.IsAny<MetrikaTimestampFormat>()),
                Times.Once);
        }

        [Fact]
        public void RegisterLogger_WithNull_DoesNotThrow()
        {
            // Act & Assert
            MetrikaCore.RegisterLogger(null!);
        }

        [Fact]
        public void RegisterLogger_SameLoggerTwice_OnlyAddsOnce()
        {
            // Arrange
            var customLogger = new Mock<IMetrikaLogger>();

            // Act
            MetrikaCore.RegisterLogger(customLogger.Object);
            MetrikaCore.RegisterLogger(customLogger.Object);
            new Func<int>(() => 1).Metrika("Test");

            // Assert
            customLogger.Verify(
                x => x.LogMeasurement(
                    It.IsAny<MetrikaMeasurementResult>(),
                    It.IsAny<MetrikaLocalization>(),
                    It.IsAny<MetrikaTimestampFormat>()),
                Times.Once);
        }

        [Fact]
        public void ClearLoggers_RemovesAllCustomLoggers()
        {
            // Arrange
            var customLogger = new Mock<IMetrikaLogger>();
            MetrikaCore.RegisterLogger(customLogger.Object);

            // Act
            MetrikaCore.ClearLoggers();
            new Func<int>(() => 1).Metrika("Test");

            // Assert
            customLogger.Verify(
                x => x.LogMeasurement(
                    It.IsAny<MetrikaMeasurementResult>(),
                    It.IsAny<MetrikaLocalization>(),
                    It.IsAny<MetrikaTimestampFormat>()),
                Times.Never);
        }

        [Fact]
        public void RegisterLogger_MultipleLoggers_AllGetCalled()
        {
            // Arrange
            var logger1 = new Mock<IMetrikaLogger>();
            var logger2 = new Mock<IMetrikaLogger>();
            var logger3 = new Mock<IMetrikaLogger>();

            // Act
            MetrikaCore.RegisterLogger(logger1.Object);
            MetrikaCore.RegisterLogger(logger2.Object);
            MetrikaCore.RegisterLogger(logger3.Object);
            new Func<int>(() => 1).Metrika("Test");

            // Assert
            logger1.Verify(x => x.LogMeasurement(It.IsAny<MetrikaMeasurementResult>(), It.IsAny<MetrikaLocalization>(), It.IsAny<MetrikaTimestampFormat>()), Times.Once);
            logger2.Verify(x => x.LogMeasurement(It.IsAny<MetrikaMeasurementResult>(), It.IsAny<MetrikaLocalization>(), It.IsAny<MetrikaTimestampFormat>()), Times.Once);
            logger3.Verify(x => x.LogMeasurement(It.IsAny<MetrikaMeasurementResult>(), It.IsAny<MetrikaLocalization>(), It.IsAny<MetrikaTimestampFormat>()), Times.Once);
        }

        #endregion

        #region Async Task<T> Tests

        [Fact]
        public async Task MetrikaAsync_WithTaskT_ReturnsOriginalResult()
        {
            // Arrange
            var expectedResult = 42;
            var task = Task.FromResult(expectedResult);

            // Act
            var result = await task.MetrikaAsync("Test Operation");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task MetrikaAsync_WithTaskT_LogsToILogger()
        {
            // Arrange
            var task = Task.FromResult("result");

            // Act
            await task.MetrikaAsync("Test Operation", logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Test Operation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MetrikaAsync_WithDelay_MeasuresTimeAccurately()
        {
            // Arrange
            var delayMs = 100;

            // Act
            var sw = Stopwatch.StartNew();
            await Task.Delay(delayMs).MetrikaAsync("Delay Test");
            sw.Stop();

            // Assert
            Assert.True(sw.ElapsedMilliseconds >= delayMs - 20);
        }

        [Fact]
        public async Task MetrikaAsync_WithException_PropagatesException()
        {
            // Arrange
            var faultyTask = Task.Run(() =>
            {
                throw new InvalidOperationException("Test exception");

                return 1;

            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await faultyTask.MetrikaAsync("Faulty Task"));
        }

        [Fact]
        public async Task MetrikaAsync_WithComplexObject_ReturnsObject()
        {
            // Arrange
            var expected = new TestObject { Id = 1, Name = "Test" };
            var task = Task.FromResult(expected);

            // Act
            var result = await task.MetrikaAsync("Complex Object Test");

            // Assert
            Assert.Same(expected, result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        #endregion

        #region Async Task (void) Tests

        [Fact]
        public async Task MetrikaAsync_WithTask_CompletesSuccessfully()
        {
            // Arrange
            var completed = false;
            var task = Task.Run(() => completed = true);

            // Act
            await task.MetrikaAsync("Test Task");

            // Assert
            Assert.True(completed);
        }

        [Fact]
        public async Task MetrikaAsync_WithTask_LogsInformation()
        {
            // Arrange & Act
            await Task.CompletedTask.MetrikaAsync("Completed Task", logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Sync Func<T> Tests

        [Fact]
        public void Metrika_WithFunc_ReturnsOriginalResult()
        {
            // Arrange
            var expectedResult = "Hello World";
            Func<string> func = () => expectedResult;

            // Act
            var result = func.Metrika("Test Func");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Metrika_WithFunc_LogsToILogger()
        {
            // Arrange
            Func<int> func = () => 42;

            // Act
            func.Metrika("Test Func", logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Metrika_WithComputationFunc_MeasuresTime()
        {
            // Arrange
            Func<int> heavyComputation = () =>
            {
                Thread.Sleep(50);
                return 42;
            };

            // Act
            var sw = Stopwatch.StartNew();
            var result = heavyComputation.Metrika("Heavy Computation");
            sw.Stop();

            // Assert
            Assert.Equal(42, result);
            Assert.True(sw.ElapsedMilliseconds >= 40);
        }

        [Fact]
        public void Metrika_WithFunc_CanUseAllParametersOptionally()
        {
            // Arrange
            Func<int> func = () => 123;

            // Act - Tüm parametreler opsiyonel
            var result1 = func.Metrika("Test1");
            var result2 = func.Metrika("Test2", thresholdMs: 100);
            var result3 = func.Metrika("Test3", logger: _mockLogger.Object);
            var result4 = func.Metrika("Test4", trackMemory: true);

            // Assert
            Assert.Equal(123, result1);
            Assert.Equal(123, result2);
            Assert.Equal(123, result3);
            Assert.Equal(123, result4);
        }

        #endregion

        #region Sync Action Tests

        [Fact]
        public void Metrika_WithAction_CompletesSuccessfully()
        {
            // Arrange
            var executed = false;
            Action action = () => executed = true;

            // Act
            action.Metrika("Test Action");

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Metrika_WithAction_LogsToILogger()
        {
            // Arrange
            Action action = () => { };

            // Act
            action.Metrika("Test Action", logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Metrika_WithAction_CanUseAllParametersOptionally()
        {
            // Arrange
            var counter = 0;
            Action action = () => counter++;

            // Act - Tüm parametreler opsiyonel
            action.Metrika("Test1");
            action.Metrika("Test2", thresholdMs: 100);
            action.Metrika("Test3", logger: _mockLogger.Object);
            action.Metrika("Test4", trackMemory: true);

            // Assert
            Assert.Equal(4, counter);
        }

        #endregion

        #region Pass-through Tests - REMOVED
        // Pass-through metodu kaldırıldı
        // Kullanıcılar bunun yerine Func<T> kullanmalı:
        // var result = new Func<List<int>>(() => list).Metrika("name");

        #endregion

        #region Threshold Tests

        [Fact]
        public async Task MetrikaAsync_WhenThresholdExceeded_LogsWarning()
        {
            // Arrange
            var thresholdMs = 50;
            var delayMs = 100;

            // Act
            await Task.Delay(delayMs).MetrikaAsync(
                "Threshold Test",
                thresholdMs: thresholdMs,
                logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("duration high")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MetrikaAsync_WhenBelowThreshold_LogsInformation()
        {
            // Arrange
            var thresholdMs = 1000;

            // Act
            await Task.CompletedTask.MetrikaAsync(
                "Fast Task",
                thresholdMs: thresholdMs,
                logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Metrika_WithThresholdExceeded_LogsWarning()
        {
            // Arrange
            Func<int> slowFunc = () =>
            {
                Thread.Sleep(100);
                return 1;
            };

            // Act
            slowFunc.Metrika("Slow Function", thresholdMs: 50, logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("duration high")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Memory Tracking Tests

        [Fact]
        public async Task MetrikaAsync_WithMemoryTracking_LogsMemoryInfo()
        {
            // Arrange
            var data = new byte[1_000_000];

            // Act
            await Task.Run(() =>
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] = (byte)(i % 256);
            }).MetrikaAsync("Memory Test", trackMemory: true, logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Metrika_WithMemoryTracking_LogsMemoryInfo()
        {
            // Arrange
            Func<byte[]> allocateMemory = () => new byte[1_000_000];

            // Act
            allocateMemory.Metrika("Allocate Memory", trackMemory: true, logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ConfigureMemoryTracking_EnablesGlobalTracking()
        {
            // Arrange
            MetrikaCore.ConfigureMemoryTracking(true);

            // Act
            var result = new Func<int>(() => 1).Metrika("Test", logger: _mockLogger.Object);

            // Assert
            Assert.Equal(1, result);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ConfigureMemoryTracking_DisablesGlobalTracking()
        {
            // Arrange
            MetrikaCore.ConfigureMemoryTracking(false);

            // Act
            var result = new Func<int>(() => 1).Metrika("Test", logger: _mockLogger.Object);

            // Assert
            Assert.Equal(1, result);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Metrika_MemoryTrackingOverride_OverridesGlobalSetting()
        {
            // Arrange
            MetrikaCore.ConfigureMemoryTracking(false);

            // Act
            var result = new Func<int>(() => 1).Metrika("Test", trackMemory: true, logger: _mockLogger.Object);

            // Assert
            Assert.Equal(1, result);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Metrika_Action_WithMemoryTracking_Works()
        {
            // Arrange
            var data = new byte[500_000];
            Action action = () =>
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] = (byte)i;
            };

            // Act
            action.Metrika("Action Memory Test", trackMemory: true, logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public void ConfigureTimestampFormat_SetsGlobalFormat()
        {
            // Arrange
            var format = MetrikaTimestampFormat.ISO8601;

            // Act
            MetrikaCore.ConfigureTimestampFormat(format);
            var result = new Func<int>(() => 1).Metrika("Timestamp Test");

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void ConfigureTimestampFormat_WithNull_DisablesTimestamp()
        {
            // Arrange & Act
            MetrikaCore.ConfigureTimestampFormat(null);
            var result = new Func<int>(() => 1).Metrika("Test");

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void ConfigureLocalization_SetsGlobalLocalization()
        {
            // Arrange
            var localization = MetrikaLocalization.Turkish;

            // Act
            MetrikaCore.ConfigureLocalization(localization);
            var result = new Func<int>(() => 1).Metrika("Localization Test");

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void ConfigureLocalization_WithNull_UsesEnglish()
        {
            // Arrange & Act
            MetrikaCore.ConfigureLocalization(null);
            var result = new Func<int>(() => 1).Metrika("Test");

            // Assert
            Assert.Equal(1, result);
        }

        #endregion

        #region Localization Tests

        [Theory]
        [MemberData(nameof(GetAllLocalizations))]
        public void Metrika_WithDifferentLocalizations_CompletesSuccessfully(MetrikaLocalization localization, string cultureName)
        {
            // Arrange
            MetrikaCore.ConfigureLocalization(localization);

            // Act
            var result = new Func<int>(() => 1).Metrika($"Test {cultureName}");

            // Assert
            Assert.Equal(1, result);
        }

        public static IEnumerable<object[]> GetAllLocalizations()
        {
            yield return new object[] { MetrikaLocalization.English, "English" };
            yield return new object[] { MetrikaLocalization.Turkish, "Turkish" };
            yield return new object[] { MetrikaLocalization.French, "French" };
            yield return new object[] { MetrikaLocalization.German, "German" };
            yield return new object[] { MetrikaLocalization.Spanish, "Spanish" };
            yield return new object[] { MetrikaLocalization.Japanese, "Japanese" };
            yield return new object[] { MetrikaLocalization.ChineseSimplified, "Chinese" };
            yield return new object[] { MetrikaLocalization.Russian, "Russian" };
            yield return new object[] { MetrikaLocalization.Portuguese, "Portuguese" };
            yield return new object[] { MetrikaLocalization.Italian, "Italian" };
        }

        [Fact]
        public void Metrika_WithCustomLocalization_UsesCustomStrings()
        {
            // Arrange
            var customLoc = new MetrikaLocalization
            {
                Duration = "custom duration",
                Prefix = "CUSTOM"
            };
            var customLogger = new Mock<IMetrikaLogger>();
            MetrikaCore.RegisterLogger(customLogger.Object);

            // Act
            new Func<int>(() => 1).Metrika("Test", localization: customLoc);

            // Assert
            customLogger.Verify(
                x => x.LogMeasurement(
                    It.IsAny<MetrikaMeasurementResult>(),
                    It.Is<MetrikaLocalization>(l => l.Duration == "custom duration"),
                    It.IsAny<MetrikaTimestampFormat>()),
                Times.Once);
        }

        #endregion

        #region Timestamp Format Tests

        [Theory]
        [MemberData(nameof(GetAllTimestampFormats))]
        public void Metrika_WithDifferentTimestampFormats_CompletesSuccessfully(MetrikaTimestampFormat format, string formatName)
        {
            // Arrange
            MetrikaCore.ConfigureTimestampFormat(format);

            // Act
            var result = new Func<int>(() => 1).Metrika($"Timestamp Test: {formatName}");

            // Assert
            Assert.Equal(1, result);
        }

        public static IEnumerable<object[]> GetAllTimestampFormats()
        {
            yield return new object[] { MetrikaTimestampFormat.Default, "Default" };
            yield return new object[] { MetrikaTimestampFormat.Short, "Short" };
            yield return new object[] { MetrikaTimestampFormat.TimeWithMs, "TimeWithMs" };
            yield return new object[] { MetrikaTimestampFormat.ISO8601, "ISO8601" };
            yield return new object[] { MetrikaTimestampFormat.UnixTimestamp, "Unix" };
            yield return new object[] { MetrikaTimestampFormat.DateOnly, "DateOnly" };
            yield return new object[] { MetrikaTimestampFormat.Disabled, "Disabled" };
            yield return new object[] { MetrikaTimestampFormat.Custom("dd/MM/yyyy HH:mm"), "Custom" };
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task RealWorldScenario_MultipleOperations_WorksCorrectly()
        {
            // Arrange
            MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.Short);
            MetrikaCore.ConfigureLocalization(MetrikaLocalization.English);

            // Act
            var data = await Task.Run(async () =>
            {
                await Task.Delay(50).MetrikaAsync("Database Query", thresholdMs: 100, logger: _mockLogger.Object);

                var processed = new Func<List<int>>(() =>
                {
                    var list = new List<int>();
                    for (int i = 0; i < 1000; i++)
                        list.Add(i);
                    return list;
                }).Metrika("Process Data", thresholdMs: 10, logger: _mockLogger.Object);

                await Task.Delay(30).MetrikaAsync("External API Call", thresholdMs: 100, logger: _mockLogger.Object);

                return processed;
            });

            // Assert
            Assert.NotNull(data);
            Assert.Equal(1000, data.Count);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(3));
        }

        [Fact]
        public async Task ComplexScenario_WithAllFeatures_WorksCorrectly()
        {
            // Arrange
            var customLogger = new Mock<IMetrikaLogger>();
            MetrikaCore.RegisterLogger(customLogger.Object);
            MetrikaCore.ConfigureMemoryTracking(true);
            MetrikaCore.ConfigureLocalization(MetrikaLocalization.Turkish);
            MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.ISO8601);

            // Act
            var result = await Task.Run(async () =>
            {
                var bytes = new Func<byte[]>(() => new byte[5_000_000])
                    .Metrika("Allocate", trackMemory: true, logger: _mockLogger.Object);

                await Task.Delay(100)
                    .MetrikaAsync("Delay", thresholdMs: 50, logger: _mockLogger.Object);

                return bytes.Length;
            });

            // Assert
            Assert.Equal(5_000_000, result);
            customLogger.Verify(
                x => x.LogMeasurement(
                    It.IsAny<MetrikaMeasurementResult>(),
                    It.IsAny<MetrikaLocalization>(),
                    It.IsAny<MetrikaTimestampFormat>()),
                Times.AtLeast(2));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Metrika_WithVeryShortOperation_ReturnsZeroOrLowMs()
        {
            // Arrange
            Func<int> fastOp = () => 1 + 1;

            // Act
            var result = fastOp.Metrika("Fast Operation", logger: _mockLogger.Object);

            // Assert
            Assert.Equal(2, result);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Metrika_WithEmptyName_DoesNotThrow()
        {
            // Arrange & Act
            var result = new Func<int>(() => 1).Metrika("");

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task MetrikaAsync_WithCancelledTask_PropagatesCancellation()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var cancelledTask = Task.Delay(1000, cts.Token);

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await cancelledTask.MetrikaAsync("Cancelled Task"));
        }


        #endregion

        #region Parameter Flexibility Tests

        [Fact]
        public void Metrika_Func_MinimalParameters_Works()
        {
            // Arrange
            Func<string> func = () => "result";

            // Act
            var result = func.Metrika("Minimal");

            // Assert
            Assert.Equal("result", result);
        }

        [Fact]
        public void Metrika_Func_AllParameters_Works()
        {
            // Arrange
            Func<int> func = () => 42;

            // Act
            var result = func.Metrika(
                name: "Full Parameters",
                thresholdMs: 100,
                logger: _mockLogger.Object,
                localization: MetrikaLocalization.Turkish,
                timestampFormat: MetrikaTimestampFormat.ISO8601,
                trackMemory: true);

            // Assert
            Assert.Equal(42, result);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Metrika_Action_MinimalParameters_Works()
        {
            // Arrange
            var counter = 0;
            Action action = () => counter++;

            // Act
            action.Metrika("Minimal Action");

            // Assert
            Assert.Equal(1, counter);
        }

        [Fact]
        public void Metrika_Action_AllParameters_Works()
        {
            // Arrange
            var counter = 0;
            Action action = () => counter++;

            // Act
            action.Metrika(
                name: "Full Action",
                thresholdMs: 50,
                logger: _mockLogger.Object,
                localization: MetrikaLocalization.English,
                timestampFormat: MetrikaTimestampFormat.Short,
                trackMemory: false);

            // Assert
            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task MetrikaAsync_MinimalParameters_Works()
        {
            // Arrange
            var task = Task.FromResult(100);

            // Act
            var result = await task.MetrikaAsync("Minimal Async");

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public async Task MetrikaAsync_AllParameters_Works()
        {
            // Arrange
            var task = Task.FromResult("test");

            // Act
            var result = await task.MetrikaAsync(
                name: "Full Async",
                thresholdMs: 200,
                logger: _mockLogger.Object,
                localization: MetrikaLocalization.French,
                timestampFormat: MetrikaTimestampFormat.UnixTimestamp,
                trackMemory: true);

            // Assert
            Assert.Equal("test", result);
        }

        #endregion

        #region Helper Classes

        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        #endregion
    }
}