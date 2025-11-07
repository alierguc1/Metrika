using Metrika.Console;
using Metrika.Core;
using Metrika.Core.Models;
using Metrika.Sample.ConsoleApp.Helpers;

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
        Example9_IQueryableExtensions();
        Example10_NewIQueryableMethods();
        Example11_ExceptionHandling();          
        await Example12_AsyncExceptionHandling(); 
        Example13_ExceptionWithMemoryTracking();
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
    static void Example11_ExceptionHandling()
    {
        System.Console.WriteLine("--- Example 11: Exception Handling ---");

        // Success case for comparison
        var successResult = new Func<int>(() =>
        {
            Thread.Sleep(50);
            return 42;
        }).Metrika("Successful Operation");
        System.Console.WriteLine($"  ✓ Success: {successResult}");

        // Exception in Func - still measures time
        try
        {
            var result = new Func<int>(() =>
            {
                Thread.Sleep(80);
                throw new InvalidOperationException("Database connection failed!");
            }).Metrika("Database Query", thresholdMs: 100);
        }
        catch (InvalidOperationException ex)
        {
            System.Console.WriteLine($"  ✗ Caught: {ex.Message}");
        }

        // Exception in Action
        try
        {
            new Action(() =>
            {
                Thread.Sleep(30);
                throw new ArgumentException("Invalid user input");
            }).Metrika("Validate Input");
        }
        catch (ArgumentException ex)
        {
            System.Console.WriteLine($"  ✗ Caught: {ex.Message}");
        }

        // Exception with threshold exceeded
        try
        {
            var result = new Func<string>(() =>
            {
                Thread.Sleep(250);
                throw new TimeoutException("Operation timed out after delay");
            }).Metrika("Slow Operation with Error", thresholdMs: 200);
        }
        catch (TimeoutException ex)
        {
            System.Console.WriteLine($"  ✗ Caught (exceeded threshold): {ex.Message}");
        }

        System.Console.WriteLine();
    }

    static async Task Example12_AsyncExceptionHandling()
    {
        System.Console.WriteLine("--- Example 12: Async Exception Handling ---");

        // Async Task<T> with exception
        try
        {
            var result = await Task.Run(async () =>
            {
                await Task.Delay(60);
                throw new HttpRequestException("API endpoint not found");
                return "Success"; // Unreachable
            }).MetrikaAsync("External API Call", thresholdMs: 100);
        }
        catch (HttpRequestException ex)
        {
            System.Console.WriteLine($"  ✗ Async Task<T> caught: {ex.Message}");
        }

        // Async Task (void) with exception
        try
        {
            await Task.Run(async () =>
            {
                await Task.Delay(40);
                throw new Exception("Background job failed");
            }).MetrikaAsync("Background Job", thresholdMs: 50);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"  ✗ Async Task caught: {ex.Message}");
        }

        // Mixed success and failure
        System.Console.WriteLine("\n  Batch Processing (3 operations):");
        for (int i = 1; i <= 3; i++)
        {
            try
            {
                await Task.Run(async () =>
                {
                    await Task.Delay(30 * i);
                    if (i == 2)
                        throw new Exception($"Operation {i} failed");
                }).MetrikaAsync($"Batch Operation {i}", thresholdMs: 100);

                System.Console.WriteLine($"    ✓ Operation {i} succeeded");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"    ✗ Operation {i} failed: {ex.Message}");
            }
        }

        System.Console.WriteLine();
    }

    static void Example13_ExceptionWithMemoryTracking()
    {
        System.Console.WriteLine("--- Example 13: Exception with Memory Tracking ---");

        // Success with memory tracking (for comparison)
        var successData = new Func<byte[]>(() =>
        {
            return new byte[5_000_000]; // 5MB
        }).Metrika("Allocate Memory Successfully", trackMemory: true);
        System.Console.WriteLine($"  ✓ Allocated: {successData.Length:N0} bytes");

        // Exception after memory allocation
        try
        {
            var failedData = new Func<byte[]>(() =>
            {
                var data = new byte[10_000_000]; // 10MB allocated
                Thread.Sleep(50);
                throw new OutOfMemoryException("Simulated memory exhaustion");
            }).Metrika("Large Allocation Failure", trackMemory: true);
        }
        catch (OutOfMemoryException ex)
        {
            System.Console.WriteLine($"  ✗ Memory error caught: {ex.Message}");
            System.Console.WriteLine($"    (Memory was tracked before exception)");
        }

        // Exception during file processing
        try
        {
            var result = new Func<string>(() =>
            {
                var buffer = new byte[50_000_000]; // 50MB
                Thread.Sleep(100);
                throw new IOException("File read error after loading");
            }).Metrika("Process Large File", thresholdMs: 150, trackMemory: true);
        }
        catch (IOException ex)
        {
            System.Console.WriteLine($"  ✗ I/O error caught: {ex.Message}");
            System.Console.WriteLine($"    (50MB was allocated before failure)");
        }

        // Successful cleanup after exception
        System.Console.WriteLine("\n  Cleanup Operations:");
        for (int i = 1; i <= 2; i++)
        {
            try
            {
                new Func<int>(() =>
                {
                    var temp = new byte[20_000_000]; // 20MB
                    if (i == 1)
                        throw new Exception("Cleanup failed");
                    return temp.Length;
                }).Metrika($"Cleanup Task {i}", trackMemory: true);

                System.Console.WriteLine($"    ✓ Cleanup {i} succeeded");
            }
            catch
            {
                System.Console.WriteLine($"    ✗ Cleanup {i} failed (memory tracked)");
            }
        }

        System.Console.WriteLine();
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
    static void Example9_IQueryableExtensions()
    {
        System.Console.WriteLine("--- Example 9: IQueryable Extensions ---");

        // Simulate large dataset
        var users = new List<User>();
        for (int i = 1; i <= 10000; i++)
        {
            users.Add(new User { Id = i, Name = $"User{i}", IsActive = i % 3 == 0 });
        }

        var queryable = users.AsQueryable();

        // Test ToListWithMetrika
        var activeUsers = queryable
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListWithMetrika("Query Active Users", thresholdMs: 10, trackMemory: true);

        System.Console.WriteLine($"  Active Users: {activeUsers.Count}");

        // Test CountWithMetrika
        var totalCount = queryable
            .Where(u => u.Id > 5000)
            .CountWithMetrika("Count Users > 5000", thresholdMs: 5);

        System.Console.WriteLine($"  Count: {totalCount}");

        // Test FirstWithMetrika
        var firstUser = queryable
            .Where(u => u.IsActive)
            .OrderBy(u => u.Id)
            .FirstWithMetrika("Get First Active User", thresholdMs: 5);

        System.Console.WriteLine($"  First User: {firstUser.Name}");

        // Test AnyWithMetrika
        var hasInactiveUsers = queryable
            .Where(u => !u.IsActive)
            .AnyWithMetrika("Check Inactive Users Exist", thresholdMs: 5);

        System.Console.WriteLine($"  Has Inactive Users: {hasInactiveUsers}");

        // Complex query chain
        var processed = queryable
            .Where(u => u.IsActive)
            .Select(u => new { u.Id, u.Name, Upper = u.Name.ToUpper() })
            .OrderByDescending(u => u.Id)
            .Take(100)
            .ToListWithMetrika("Complex Query Chain", thresholdMs: 20, trackMemory: true);

        System.Console.WriteLine($"  Processed: {processed.Count}\n");
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
    static void Example10_NewIQueryableMethods()
    {
        System.Console.WriteLine("--- Example 10: New IQueryable Methods (Single, Last, LongCount) ---");

        // Simulate dataset
        var users = new List<User>();
        for (int i = 1; i <= 1000; i++)
        {
            users.Add(new User { Id = i, Name = $"User{i}", IsActive = i % 3 == 0 });
        }

        var queryable = users.AsQueryable();

        // Test SingleWithMetrika - Exactly one result expected
        try
        {
            var specificUser = queryable
                .Where(u => u.Id == 500)
                .SingleWithMetrika("Get User By ID", thresholdMs: 5);

            System.Console.WriteLine($"  ✓ Single User: {specificUser.Name}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"  ✗ Single failed: {ex.Message}");
        }

        // Test SingleOrDefaultWithMetrika - May return null
        var userOrNull = queryable
            .Where(u => u.Id == 9999) // Doesn't exist
            .SingleOrDefaultWithMetrika("Get Non-Existent User", thresholdMs: 5);

        System.Console.WriteLine($"  ✓ SingleOrDefault Result: {(userOrNull == null ? "Not Found (null)" : userOrNull.Name)}");

        // Test LastWithMetrika - Last element in ordered query
        var lastActiveUser = queryable
            .Where(u => u.IsActive)
            .OrderBy(u => u.Id)
            .LastWithMetrika("Get Last Active User", thresholdMs: 10);

        System.Console.WriteLine($"  ✓ Last Active User: {lastActiveUser.Name} (ID: {lastActiveUser.Id})");

        // Test LastOrDefaultWithMetrika - May return null
        var lastInactiveUser = queryable
            .Where(u => !u.IsActive)
            .OrderBy(u => u.Id)
            .LastOrDefaultWithMetrika("Get Last Inactive User", thresholdMs: 10);

        System.Console.WriteLine($"  ✓ Last Inactive User: {lastInactiveUser?.Name ?? "None"}");

        // Test LongCountWithMetrika - For large datasets
        var totalCount = queryable
            .LongCountWithMetrika("Count All Users (Long)", thresholdMs: 5);

        System.Console.WriteLine($"  ✓ Total Count (long type): {totalCount:N0}");

        // Complex scenario: Find unique user with error handling
        try
        {
            var uniqueUser = queryable
                .Where(u => u.Id == 42)
                .SingleWithMetrika("Find User 42", thresholdMs: 5, trackMemory: true);

            System.Console.WriteLine($"  ✓ Unique User Search: {uniqueUser.Name}");
        }
        catch (InvalidOperationException)
        {
            System.Console.WriteLine($"  ✗ Unique User Search: Not found or multiple results");
        }

        // Performance comparison: Count vs LongCount
        System.Console.WriteLine("\n  Performance Comparison:");

        var countResult = queryable
            .Where(u => u.IsActive)
            .CountWithMetrika("Count Active (int)", thresholdMs: 5);

        var longCountResult = queryable
            .Where(u => u.IsActive)
            .LongCountWithMetrika("LongCount Active (long)", thresholdMs: 5);

        System.Console.WriteLine($"  Count result: {countResult} (type: int)");
        System.Console.WriteLine($"  LongCount result: {longCountResult} (type: long)");

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
}