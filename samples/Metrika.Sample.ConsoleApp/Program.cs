using Metrika.Console;
using Metrika.Core;
using Metrika.Core.Models;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("=== Metrika Console Logger Examples ===\n");

        // Setup
        ConfigureMetrika();

        // Run examples
        Example1_BasicMeasurement();
        Example2_WithColors();
        Example3_ThresholdWarnings();
        await Example4_AsyncOperations();
        Example5_MemoryTracking();
        Example6_DifferentLocalizations();
        Example7_TimestampFormats();
        await Example8_RealWorldScenario();

        System.Console.WriteLine("\n=== All Examples Completed ===");
        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }

    static void ConfigureMetrika()
    {
        // Register console logger with default colors
        MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(
            colorScheme: MetrikaColorScheme.Default,
            useColors: true));

        // Global settings
        MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.Short);
        MetrikaCore.ConfigureLocalization(MetrikaLocalization.English);

        System.Console.WriteLine("✓ Metrika Console Logger configured\n");
    }

    static void Example1_BasicMeasurement()
    {
        System.Console.WriteLine("--- Example 1: Basic Measurement ---");

        var result = new Func<int>(() =>
        {
            Thread.Sleep(100);
            return 42;
        }).Metrika("Calculate Answer");

        System.Console.WriteLine($"  Result: {result}\n");
    }

    static void Example2_WithColors()
    {
        System.Console.WriteLine("--- Example 2: Color Schemes ---");

        // Fast (Green)
        new Func<int>(() =>
        {
            Thread.Sleep(50);
            return 1;
        }).Metrika("Fast Operation");

        // Normal (Blue)
        new Func<int>(() =>
        {
            Thread.Sleep(600);
            return 2;
        }).Metrika("Normal Operation");

        // Slow (Yellow)
        new Func<int>(() =>
        {
            Thread.Sleep(1100);
            return 3;
        }).Metrika("Slow Operation");

        System.Console.WriteLine();
    }

    static void Example3_ThresholdWarnings()
    {
        System.Console.WriteLine("--- Example 3: Threshold Warnings ---");

        // Below threshold (Green/Blue)
        new Func<string>(() =>
        {
            Thread.Sleep(30);
            return "Fast";
        }).Metrika("Fast Operation", thresholdMs: 100);

        // Exceeds threshold (Red)
        new Func<string>(() =>
        {
            Thread.Sleep(150);
            return "Slow";
        }).Metrika("Slow Operation", thresholdMs: 100);

        System.Console.WriteLine();
    }

    static async Task Example4_AsyncOperations()
    {
        System.Console.WriteLine("--- Example 4: Async Operations ---");

        var result = await Task.Run(async () =>
        {
            await Task.Delay(80);
            return "Async Result";
        }).MetrikaAsync("Async Operation");

        await Task.Delay(120).MetrikaAsync("Another Async Operation", thresholdMs: 100);

        System.Console.WriteLine($"  Result: {result}\n");
    }

    static void Example5_MemoryTracking()
    {
        System.Console.WriteLine("--- Example 5: Memory Tracking ---");

        // Small allocation
        var smallData = new Func<byte[]>(() =>
        {
            return new byte[1_000_000]; // 1 MB
        }).Metrika("Allocate 1MB", trackMemory: true);

        // Large allocation (will show HIGH MEMORY warning)
        var largeData = new Func<byte[]>(() =>
        {
            return new byte[120_000_000]; // 120 MB
        }).Metrika("Allocate 120MB", trackMemory: true);

        System.Console.WriteLine($"  Small: {smallData.Length:N0} bytes");
        System.Console.WriteLine($"  Large: {largeData.Length:N0} bytes\n");
    }

    static void Example6_DifferentLocalizations()
    {
        System.Console.WriteLine("--- Example 6: Multiple Localizations ---");

        // English
        new Func<int>(() =>
        {
            Thread.Sleep(25);
            return 1;
        }).Metrika("English Test", localization: MetrikaLocalization.English);

        // Turkish
        new Func<int>(() =>
        {
            Thread.Sleep(25);
            return 2;
        }).Metrika("Turkish Test", localization: MetrikaLocalization.Turkish);

        // French
        new Func<int>(() =>
        {
            Thread.Sleep(25);
            return 3;
        }).Metrika("French Test", localization: MetrikaLocalization.French);

        // German
        new Func<int>(() =>
        {
            Thread.Sleep(25);
            return 4;
        }).Metrika("German Test", localization: MetrikaLocalization.German);

        System.Console.WriteLine();
    }

    static void Example7_TimestampFormats()
    {
        System.Console.WriteLine("--- Example 7: Timestamp Formats ---");

        // Short format
        new Action(() => Thread.Sleep(10))
            .Metrika("Short Timestamp", timestampFormat: MetrikaTimestampFormat.Short);

        // ISO 8601
        new Action(() => Thread.Sleep(10))
            .Metrika("ISO8601 Timestamp", timestampFormat: MetrikaTimestampFormat.ISO8601);

        // Unix timestamp
        new Action(() => Thread.Sleep(10))
            .Metrika("Unix Timestamp", timestampFormat: MetrikaTimestampFormat.UnixTimestamp);

        // Custom format
        new Action(() => Thread.Sleep(10))
            .Metrika("Custom Timestamp",
                timestampFormat: MetrikaTimestampFormat.Custom("dd/MM/yyyy HH:mm:ss"));

        System.Console.WriteLine();
    }

    static async Task Example8_RealWorldScenario()
    {
        System.Console.WriteLine("--- Example 8: Real-World Scenario ---");

        // Simulate multi-step process
        var users = await Task.Run(async () =>
        {
            await Task.Delay(50);
            return Enumerable.Range(1, 100)
                .Select(i => new User { Id = i, Name = $"User{i}", IsActive = i % 2 == 0 })
                .ToList();
        }).MetrikaAsync("Fetch Users from DB", thresholdMs: 100, trackMemory: true);

        var activeUsers = new Func<List<User>>(() =>
        {
            Thread.Sleep(30);
            return users.Where(u => u.IsActive).ToList();
        }).Metrika("Filter Active Users", trackMemory: true);

        var processed = new Func<List<ProcessedUser>>(() =>
        {
            Thread.Sleep(40);
            return activeUsers.Select(u => new ProcessedUser
            {
                Id = u.Id,
                Name = u.Name.ToUpper(),
                ProcessedAt = DateTime.Now
            }).ToList();
        }).Metrika("Process Users", thresholdMs: 50, trackMemory: true);

        await Task.Run(async () =>
        {
            await Task.Delay(20);
        }).MetrikaAsync("Save to Cache");

        System.Console.WriteLine($"  Total Users: {users.Count}");
        System.Console.WriteLine($"  Active Users: {activeUsers.Count}");
        System.Console.WriteLine($"  Processed: {processed.Count}\n");
    }

    // Helper classes
    class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    class ProcessedUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}