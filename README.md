# ⏱️ Metrika - Performance Measurement Library for .NET


<p align="center">
  <a href="https://www.nuget.org/packages/Metrika.Core/">
    <img src="https://img.shields.io/nuget/v/Metrika.Core?style=flat-square&logo=nuget&label=Core" alt="NuGet Core">
  </a>
  <a href="https://www.nuget.org/packages/Metrika.Console/">
    <img src="https://img.shields.io/nuget/v/Metrika.Console?style=flat-square&logo=nuget&color=blue&label=Console" alt="NuGet Console">
  </a>
  <a href="https://www.nuget.org/packages/Metrika.Core/">
    <img src="https://img.shields.io/nuget/dt/Metrika.Core?style=flat-square&color=004880&label=Core%20Downloads" alt="Core Downloads">
  </a>
   <a href="https://www.nuget.org/packages/Metrika.Console/">
    <img src="https://img.shields.io/nuget/dt/Metrika.Console?style=flat-square&color=004880&label=Console%20Downloads" alt="Console Downloads">
  </a>
  <a href="https://github.com/alierguc1/Metrika/actions/workflows/build.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/alierguc1/Metrika/build.yml?branch=master&style=flat-square&logo=github&label=build" alt="Build">
  </a>
  <a href="https://github.com/alierguc1/Metrika">
    <img src="https://img.shields.io/github/stars/alierguc1/Metrika?style=flat-square&logo=github" alt="Stars">
  </a>
  <a href="https://github.com/alierguc1/Metrika/blob/master/LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square" alt="MIT License">
  </a>
</p>


<p align="center">
  <img src="assets/Metrika_512x512.png" alt="Metrika Logo" width="512" height="512">
</p>

<p align="center">
  <strong>Fluent, lightweight, and powerful performance measurement library for .NET applications</strong>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#installation">Installation</a> •
  <a href="#why-metrika">Why Metrika?</a> •
  <a href="#quick-start">Quick Start</a> •
  <a href="#color-schemes">Color Schemes</a> •
  <a href="#documentation">Documentation</a> •
  <a href="#examples">Examples</a>
</p>

---

## ✨ Features

- ⚡ **Fluent API** - Intuitive extension methods for measuring async/sync operations
- 🎨 **Colorful Console Output** - Beautiful, color-coded console logging with 4 themes
- 🧠 **Memory Tracking** - Track memory allocation and GC collections
- ⏰ **Threshold Warnings** - Get alerted when operations exceed time limits
- 🌍 **Multi-Language** - Support for 10+ languages
- 🎯 **Custom Loggers** - Implement your own logging strategy
- 📊 **Zero Dependencies** - Core library has no external dependencies
- 🚀 **High Performance** - Minimal overhead on your application
- 🔌 **Easy Integration** - Works with ILogger, Serilog, NLog, etc.
- 🔧 **Multi-target Support** - Works with .NET 6.0, 7.0, 8.0, and 9.0
- 🔍 **IQueryable Extensions** - Measure LINQ query performance
  - ToList, ToArray, First, FirstOrDefault
  - **Single, SingleOrDefault** (v1.3.0+) - For unique element queries
  - **Last, LastOrDefault** (v1.3.0+) - For ordered sequence queries
  - Count, **LongCount** (v1.3.0+) - For large datasets
  - Any
- ⚡ **Zero Overhead** - Minimal performance impact
- 🛡️ **Exception Handling** - Captures performance metrics even on failure (v1.4.0+)

---

## 📊 IQueryable Extensions Guide

Metrika provides powerful extension methods for measuring LINQ query performance in real-time. Available in **v1.3.0+**.

### 🎯 Available Methods

| Method | Use Case | Return Type | Throws if Empty | Throws if Multiple |
|--------|----------|-------------|-----------------|-------------------|
| `ToList` | Materialize query to list | `List<T>` | ❌ No | ❌ No |
| `ToArray` | Materialize query to array | `T[]` | ❌ No | ❌ No |
| `First` | Get first element | `T` | ✅ Yes | ❌ No |
| `FirstOrDefault` | Get first or null | `T?` | ❌ No (null) | ❌ No |
| **`Single`** ⭐ | **Exactly 1 expected** | `T` | ✅ Yes | ✅ Yes |
| **`SingleOrDefault`** ⭐ | **0 or 1 expected** | `T?` | ❌ No (null) | ✅ Yes |
| **`Last`** ⭐ | **Get last (ordered)** | `T` | ✅ Yes | ❌ No |
| **`LastOrDefault`** ⭐ | **Get last or null (ordered)** | `T?` | ❌ No (null) | ❌ No |
| `Count` | Count elements | `int` | ❌ No (0) | ❌ No |
| **`LongCount`** ⭐ | **Count large datasets** | `long` | ❌ No (0) | ❌ No |
| `Any` | Check existence | `bool` | ❌ No (false) | ❌ No |

⭐ = **New in v1.3.0**

---

### 💡 When to Use Each Method

#### Single vs First vs Last
```csharp
// ✅ Use Single - When you expect EXACTLY one result (ID lookup)
var user = users
    .Where(u => u.Id == userId)
    .SingleWithMetrika("Get User By ID", thresholdMs: 50);
// Throws if: 0 results OR multiple results

// ✅ Use SingleOrDefault - When 0 or 1 expected (email search)
var user = users
    .Where(u => u.Email == email)
    .SingleOrDefaultWithMetrika("Find User By Email", thresholdMs: 100);
// Returns null if not found, throws if multiple results

// ✅ Use First - When you want the first match (any amount)
var latestPost = posts
    .OrderByDescending(p => p.CreatedDate)
    .FirstWithMetrika("Get Latest Post", thresholdMs: 50);
// Works with 1 or many results

// ✅ Use Last - When you want the last match (ordered query)
var oldestOrder = orders
    .OrderBy(o => o.CreatedDate)
    .LastWithMetrika("Get Oldest Order", thresholdMs: 100);
// Works with 1 or many results
```

---

#### Count vs LongCount
```csharp
// ✅ Use Count - For standard datasets (<2.1 billion records)
var userCount = users
    .Where(u => u.IsActive)
    .CountWithMetrika("Count Active Users", thresholdMs: 200);
// Returns: int (max 2,147,483,647)

// ✅ Use LongCount - For very large datasets (>2 billion records)
var totalTransactions = transactions
    .LongCountWithMetrika("Count All Transactions", thresholdMs: 5000);
// Returns: long (max 9,223,372,036,854,775,807)
```

**When to use LongCount:**
- ✅ Big data / data warehouse tables
- ✅ Historical transaction logs
- ✅ Time-series data over many years
- ✅ Event logging tables

---

### 🔥 Real-World Examples

#### Example 1: User Authentication
```csharp
public User? AuthenticateUser(string email, string password)
{
    // Use SingleOrDefault - email should be unique, but might not exist
    var user = _context.Users
        .Where(u => u.Email == email)
        .SingleOrDefaultWithMetrika("Authenticate User", thresholdMs: 50);
    
    return user?.VerifyPassword(password) == true ? user : null;
}
```

#### Example 2: Dashboard Statistics
```csharp
public DashboardStats GetDashboardStats()
{
    // Use Last - Get most recent order
    var latestOrder = _context.Orders
        .OrderBy(o => o.CreatedDate)
        .LastOrDefaultWithMetrika("Get Latest Order", thresholdMs: 100);
    
    // Use LongCount - Large historical data
    var totalUsers = _context.Users
        .LongCountWithMetrika("Count Total Users", thresholdMs: 200);
    
    return new DashboardStats 
    { 
        LatestOrder = latestOrder,
        TotalUsers = totalUsers 
    };
}
```

#### Example 3: Order Processing
```csharp
public async Task ProcessOrderAsync(int orderId)
{
    // Use Single - Order ID is primary key (must exist and be unique)
    var order = await _context.Orders
        .Where(o => o.Id == orderId)
        .SingleAsync()
        .MetrikaAsync("Get Order By ID", thresholdMs: 50);
    
    // Use Last - Get latest status update
    var lastStatus = _context.OrderStatusHistory
        .Where(s => s.OrderId == orderId)
        .OrderBy(s => s.UpdatedAt)
        .LastWithMetrika("Get Latest Status", thresholdMs: 50);
    
    // Process order...
}
```

#### Example 4: Analytics Query
```csharp
public AnalyticsReport GenerateReport(DateTime startDate, DateTime endDate)
{
    // Use LongCount - Potentially billions of records
    var totalEvents = _context.AnalyticsEvents
        .Where(e => e.Timestamp >= startDate && e.Timestamp < endDate)
        .LongCountWithMetrika("Count Analytics Events", 
            thresholdMs: 10000,
            trackMemory: true);
    
    // Output: [METRIKA] Count Analytics Events duration: 8,247 ms | Memory: +15.3 MB
    
    return new AnalyticsReport { TotalEvents = totalEvents };
}
```

---

### ⚠️ Common Mistakes
```csharp
// ❌ BAD: Using Single when multiple results expected
var posts = _context.Posts
    .Where(p => p.AuthorId == authorId)
    .SingleWithMetrika("Get Posts"); // THROWS! Multiple results

// ✅ GOOD: Use First or ToList
var post = _context.Posts
    .Where(p => p.AuthorId == authorId)
    .FirstWithMetrika("Get Latest Post");

// ❌ BAD: Using First when you need exactly one
var user = _context.Users
    .Where(u => u.Id == userId)
    .FirstWithMetrika("Get User"); // Works, but doesn't validate uniqueness

// ✅ GOOD: Use Single for ID lookups
var user = _context.Users
    .Where(u => u.Id == userId)
    .SingleWithMetrika("Get User"); // Validates exactly one result

// ❌ BAD: Using Count on huge tables
var count = _context.BigDataTable
    .CountWithMetrika("Count Records"); // May overflow! (>2B records)

// ✅ GOOD: Use LongCount for large datasets
var count = _context.BigDataTable
    .LongCountWithMetrika("Count Records"); // Safe for >2B records
```

---

### 🎯 Performance Tips
```csharp
// ✅ TIP 1: Use Any() to check existence (fastest!)
var exists = _context.Users
    .Where(u => u.Email == email)
    .AnyWithMetrika("Check Email Exists", thresholdMs: 10);
// 3ms - Only checks if records exist

// ❌ Don't use Count() for existence checks
var count = _context.Users
    .Where(u => u.Email == email)
    .CountWithMetrika("Count Users"); // 45ms - Counts all matches

// ✅ TIP 2: Use Last instead of OrderByDescending + First
var latest = orders
    .OrderBy(o => o.Date)
    .LastWithMetrika("Get Latest"); // More semantic

// ✅ TIP 3: Add indexes for Single/Last queries
// Single queries on indexed columns are blazing fast
var user = _context.Users
    .Where(u => u.Id == userId) // Primary key (indexed)
    .SingleWithMetrika("Get User", thresholdMs: 5); // <5ms
```


## 🆚 Why Metrika?

### Comparison with Popular Alternatives

| Feature | Metrika | BenchmarkDotNet | MiniProfiler | App Metrics |
|---------|---------|-----------------|--------------|-------------|
| **Fluent API** | ✅ | ❌ | ⚠️ Partial | ❌ |
| **Colorful Console** | ✅ 4 themes | ❌ | ❌ | ❌ |
| **Memory Tracking** | ✅ Built-in | ✅ Advanced | ❌ | ✅ |
| **Easy Setup** | ✅ 2 lines | ❌ Complex | ⚠️ Moderate | ❌ Complex |
| **Real-time Monitoring** | ✅ | ❌ | ✅ | ✅ |
| **Zero Config** | ✅ | ❌ | ❌ | ❌ |
| **ILogger Integration** | ✅ | ❌ | ⚠️ Partial | ✅ |
| **Localization** | ✅ 10+ languages | ❌ | ❌ | ❌ |
| **Production Ready** | ✅ | ⚠️ Dev/Test only | ✅ | ✅ |
| **Learning Curve** | 🟢 Easy | 🔴 Steep | 🟡 Moderate | 🔴 Steep |
| **Package Size** | 🟢 ~50 KB | 🔴 ~2 MB | 🟡 ~500 KB | 🔴 ~5 MB |

### 🎯 Key Differentiators

#### **1. Fluent API Design**

```csharp
// ✅ Metrika - Clean and intuitive
var result = await GetUsersAsync().MetrikaAsync("Get Users");

// ❌ BenchmarkDotNet - Requires separate benchmark class
[Benchmark]
public async Task GetUsers() => await GetUsersAsync();

// ⚠️ MiniProfiler - Verbose
using (MiniProfiler.Current.Step("Get Users"))
{
    var result = await GetUsersAsync();
}

// ❌ App Metrics - Complex setup
var timer = _metrics.Measure.Timer.Time(new TimerOptions { Name = "Get Users" });
var result = await GetUsersAsync();
timer.Dispose();
```

#### **2. Zero Configuration**

```csharp
// ✅ Metrika - 2 lines to start
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger());
var result = myFunc.Metrika("Operation Name");

// ❌ BenchmarkDotNet - Complex configuration
var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddExporter(HtmlExporter.Default)
    .AddDiagnoser(MemoryDiagnoser.Default);
BenchmarkRunner.Run<MyBenchmarks>(config);

// ❌ App Metrics - Complex setup
var metrics = new MetricsBuilder()
    .Configuration.Configure(options => { /* ... */ })
    .OutputMetrics.AsPrometheusPlainText()
    .Build();
```

#### **3. Colorful Console Output**

- **Metrika**: Color-coded output with 4 customizable themes
- **BenchmarkDotNet**: Plain text tables (no colors)
- **MiniProfiler**: Web UI only (no console output)
- **App Metrics**: Plain text metrics (no colors)

#### **4. Production-Ready**

- **Metrika**: Designed for both development and production with minimal overhead
- **BenchmarkDotNet**: Primarily for benchmarking/testing, not recommended for production
- **MiniProfiler**: Production-ready but focused on web profiling
- **App Metrics**: Production-ready but requires complex infrastructure setup

#### **5. Lightweight**

- **Metrika.Core**: ~50 KB, zero dependencies
- **BenchmarkDotNet**: ~2 MB, multiple dependencies
- **MiniProfiler**: ~500 KB, requires database for storage
- **App Metrics**: ~5 MB, many dependencies for full functionality

---

## 📦 Installation

### NuGet Packages

```bash
# Core library (required)
dotnet add package Metrika.Core

# Console logger (optional)
dotnet add package Metrika.Console
```

### Manual Installation

```bash
git clone https://github.com/alierguc1/Metrika.git
cd Metrika
dotnet build
```

---

## 🎨 Color Schemes

Metrika comes with 4 built-in color schemes to match your terminal preferences:

### 🌈 Default Theme (Recommended)

Perfect for light and dark terminals with full color support.

```
🟢 Fast Operations (<500ms)    - Green
🔵 Normal Operations (500-1s)  - Blue  
🟡 Slow Operations (>1s)       - Yellow
🔴 Threshold Exceeded          - Red
```

**Example:**

```csharp
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Default));
```

**Output:**

```
[METRIKA] [14:30:45] [INFO] Fast Operation duration: 150 ms        🟢
[METRIKA] [14:30:46] [INFO] Normal Operation duration: 750 ms     🔵
[METRIKA] [14:30:47] [INFO] Slow Operation duration: 1500 ms      🟡
[METRIKA] [14:30:48] [WARN] Threshold Exceeded duration: 600 ms   🔴
```

---

### 🎀 Pastel Theme

Soft colors for extended viewing sessions.

```
🔷 Fast Operations    - Cyan
🟣 Normal Operations  - Magenta
🟤 Slow Operations    - Dark Yellow
🔺 Threshold Exceeded - Dark Red
```

**Example:**

```csharp
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Pastel));
```

**Output:**

```
[METRIKA] [14:30:45] [INFO] API Call duration: 200 ms            🔷
[METRIKA] [14:30:46] [INFO] Database Query duration: 650 ms      🟣
[METRIKA] [14:30:47] [INFO] File Processing duration: 1200 ms    🟤
[METRIKA] [14:30:48] [WARN] Slow Query duration: 3000 ms         🔺
```

---

### ⚫ Monochrome Theme

For terminals without color support or minimalist preference.

```
⚪ Fast Operations    - Gray
⚪ Normal Operations  - White
⚫ Slow Operations    - Dark Gray
⚪ Threshold Exceeded - White
```

**Example:**

```csharp
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Monochrome));
```

**Output:**

```
[METRIKA] [14:30:45] [INFO] Operation 1 duration: 100 ms
[METRIKA] [14:30:46] [INFO] Operation 2 duration: 600 ms
[METRIKA] [14:30:47] [INFO] Operation 3 duration: 1100 ms
[METRIKA] [14:30:48] [WARN] Operation 4 duration: 2000 ms
```

---

### 🌑 Dark Theme

Optimized for dark terminal backgrounds.

```
🟢 Fast Operations    - Dark Green
🔷 Normal Operations  - Dark Cyan
🟡 Slow Operations    - Dark Yellow
🔴 Threshold Exceeded - Dark Red
```

**Example:**

```csharp
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Dark));
```

**Output:**

```
[METRIKA] [14:30:45] [INFO] Load Data duration: 180 ms           🟢
[METRIKA] [14:30:46] [INFO] Transform duration: 720 ms           🔷
[METRIKA] [14:30:47] [INFO] Save Results duration: 1300 ms       🟡
[METRIKA] [14:30:48] [WARN] Bulk Insert duration: 5000 ms        🔴
```

---

### 🎨 Custom Color Scheme

Create your own color scheme:

```csharp
var customScheme = new MetrikaColorScheme
{
    FastColor = ConsoleColor.Green,
    NormalColor = ConsoleColor.Cyan,
    SlowColor = ConsoleColor.Magenta,
    ThresholdExceededColor = ConsoleColor.Red
};

MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(customScheme));
```

---

### 📊 Color Scheme Comparison

| Theme | Fast | Normal | Slow | Threshold | Best For |
|-------|------|--------|------|-----------|----------|
| **Default** | 🟢 Green | 🔵 Blue | 🟡 Yellow | 🔴 Red | Most terminals |
| **Pastel** | 🔷 Cyan | 🟣 Magenta | 🟤 Dark Yellow | 🔺 Dark Red | Long sessions |
| **Monochrome** | ⚪ Gray | ⚪ White | ⚫ Dark Gray | ⚪ White | No color support |
| **Dark** | 🟢 Dark Green | 🔷 Dark Cyan | 🟡 Dark Yellow | 🔴 Dark Red | Dark terminals |

---

### 💡 Choosing a Color Scheme

```csharp
// For Windows Terminal, PowerShell, macOS Terminal
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Default));

// For VS Code integrated terminal
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Dark));

// For CI/CD pipelines (Jenkins, GitHub Actions)
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Monochrome));

// For extended coding sessions
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Pastel));

// Disable colors
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(useColors: false));
```

---

## 🚀 Quick Start

### 1. Basic Setup

```csharp
using Metrika.Core;
using Metrika.Core.Models;
using Metrika.Console;

// Register console logger with your preferred theme
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger(MetrikaColorScheme.Default));

// Configure global settings
MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.Short);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.English);
```

### 2. Measure Sync Operations

```csharp
// Measure a function
var result = new Func<int>(() =>
{
    // Your code here
    return 42;
}).Metrika("Calculate Answer");

// Measure an action
new Action(() =>
{
    // Your code here
}).Metrika("Process Data");
```

### 3. Measure Async Operations

```csharp
// Measure async Task<T>
var users = await _dbContext.Users
    .ToListAsync()
    .MetrikaAsync("Get Users", thresholdMs: 500);

// Measure async Task
await SendEmailAsync(email)
    .MetrikaAsync("Send Email", thresholdMs: 2000);
```

### 4. Measure LINQ Queries (⭐ NEW in v1.2.0)
```csharp
// Measure database queries directly
var activeUsers = queryable
    .Where(x => x.IsActive)
    .OrderBy(x => x.Name)
    .ToListWithMetrika("Get Active Users", thresholdMs: 500, trackMemory: true);

// Measure count operations
var userCount = queryable
    .Where(x => x.IsActive)
    .CountWithMetrika("Count Active Users", thresholdMs: 100);

// Measure existence checks (fastest!)
var hasUsers = queryable
    .Where(x => x.IsActive)
    .AnyWithMetrika("Check Active Users Exist", thresholdMs: 50);
```
---

### 5. Exception Handling (⭐ NEW in v1.4.0)

Metrika captures performance metrics **even when operations fail**, providing valuable debugging insights:
```csharp
try
{
    var result = new Func<List>(() =>
    {
        return database.GetUsers(); // May throw
    }).Metrika("Get Users", thresholdMs: 500);
}
catch (SqlException ex)
{
    // Exception is logged with timing before reaching here
    Console.WriteLine($"Failed: {ex.Message}");
}

// Output:
// [METRIKA] [14:30:45] [ERROR] Get Users failed after: 3247 ms | Exception: SqlException: Connection timeout
```

**Key Benefits:**
- ✅ **Stopwatch stops** even on exception
- ✅ **Memory tracking completes** even on failure
- ✅ **Performance data preserved** with error context
- ✅ **Exception re-thrown** normally (stack trace intact)
- ✅ **Red color output** for easy error identification


## 📚 Documentation

### Basic Usage

#### Measuring Functions

```csharp
var result = new Func<List<User>>(() =>
{
    return database.GetUsers();
}).Metrika("Fetch Users");
```

#### Measuring with Threshold

```csharp
var result = new Func<string>(() =>
{
    return HeavyCalculation();
}).Metrika("Heavy Calculation", thresholdMs: 100);

// Output if exceeded:
// [METRIKA] [14:30:45] [WARN] Heavy Calculation duration high: 150 ms (threshold: 100 ms)
```

#### Measuring with Memory Tracking

```csharp
var data = new Func<byte[]>(() =>
{
    return new byte[5_000_000]; // 5 MB
}).Metrika("Allocate Memory", trackMemory: true);

// Output:
// [METRIKA] [14:30:45] [INFO] Allocate Memory duration: 10 ms | Memory: +5.00 MB | GC: Gen0: 1, Gen1: 0, Gen2: 0
```

#### Async Operations

```csharp
// Database query
var users = await _dbContext.Users
    .Where(u => u.IsActive)
    .ToListAsync()
    .MetrikaAsync("Get Active Users", thresholdMs: 500);

// API call
var response = await _httpClient
    .GetAsync("https://api.example.com/data")
    .MetrikaAsync("External API Call", thresholdMs: 2000);
```


#### IQueryable Extensions (⭐ NEW in v1.2.0)

Measure LINQ query materialization performance directly:
```csharp
// ToList - Most common operation
var data = queryable
    .Where(x => x.DeviceId == deviceId && x.ReadDateTime >= startDate)
    .OrderBy(x => x.ReadDateTime)
    .ToListWithMetrika("Device Data Query", thresholdMs: 200, trackMemory: true);
// Output: ⏱️ Device Data Query duration: 145 ms | Memory: +2.3 MB | GC: Gen0: 1

// Count - Check record count
var totalProducts = queryable
    .Where(x => x.IsActive && x.Stock > 0)
    .CountWithMetrika("Count Active Products", thresholdMs: 100);
// Output: ⏱️ Count Active Products duration: 25 ms

// Any - Fastest existence check
var hasItems = queryable
    .Where(x => x.UserId == userId)
    .AnyWithMetrika("Check Cart Items Exist", thresholdMs: 50);
// Output: ⏱️ Check Cart Items Exist duration: 3 ms

// First - Get single record
var lastOrder = queryable
    .Where(x => x.UserId == userId)
    .OrderByDescending(x => x.CreatedDate)
    .FirstWithMetrika("Get Last Order", thresholdMs: 50);

// FirstOrDefault - Safe single record
var subscription = queryable
    .Where(x => x.UserId == userId && x.IsActive)
    .FirstOrDefaultWithMetrika("Check User Subscription", thresholdMs: 100);

// ToArray - Get array instead of list
var monthNames = queryable
    .Select(x => x.MonthName)
    .Distinct()
    .OrderBy(x => x)
    .ToArrayWithMetrika("Get Month Names", thresholdMs: 100);
```

**Why use IQueryable extensions?**

✅ **Measures actual database query execution time**  
✅ **Tracks SQL generation and data loading**  
✅ **Detects N+1 query problems**  
✅ **Identifies missing indexes**  
✅ **Memory usage during materialization**

**Performance Comparison:**
```csharp
// ❌ Slow - Loads everything then filters in memory
var data = queryable.ToList().Where(x => x.IsActive).ToList();
// ⚠️ 2100ms, 45 MB memory

// ✅ Fast - Filters in database
var data = queryable
    .Where(x => x.IsActive)
    .ToListWithMetrika("Filtered Query", trackMemory: true);
// ⏱️ 280ms, 8 MB memory

// ⚡ Fastest - Only checks existence
var exists = queryable
    .Where(x => x.IsActive)
    .AnyWithMetrika("Check Exists");
// ⏱️ 3ms
```

---

### Advanced Configuration

#### Timestamp Formats

```csharp
// Short format: HH:mm:ss
MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.Short);

// ISO 8601: 2025-10-19T14:30:45.123Z
MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.ISO8601);

// Unix timestamp
MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.UnixTimestamp);

// Custom format
MetrikaCore.ConfigureTimestampFormat(
    MetrikaTimestampFormat.Custom("dd/MM/yyyy HH:mm:ss")
);

// Disable timestamp
MetrikaCore.ConfigureTimestampFormat(MetrikaTimestampFormat.Disabled);
```

#### Localization

```csharp
// English (default)
MetrikaCore.ConfigureLocalization(MetrikaLocalization.English);

// Turkish
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Turkish);

// Other supported languages
MetrikaCore.ConfigureLocalization(MetrikaLocalization.French);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.German);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Spanish);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Japanese);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.ChineseSimplified);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Russian);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Portuguese);
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Italian);

// Custom localization
MetrikaCore.ConfigureLocalization(new MetrikaLocalization
{
    Duration = "tempo",
    DurationHigh = "tempo alto",
    Prefix = "MEDIDA",
    Memory = "Memória",
    Milliseconds = "ms",
    Threshold = "limite"
});
```

#### Memory Tracking

```csharp
// Enable globally for all measurements
MetrikaCore.ConfigureMemoryTracking(true);

// Or enable per-measurement
var result = new Func<byte[]>(() => new byte[1_000_000])
    .Metrika("Allocate", trackMemory: true);

// Disable globally
MetrikaCore.ConfigureMemoryTracking(false);

// Override global setting per-measurement
var result = new Func<byte[]>(() => new byte[1_000_000])
    .Metrika("Allocate", trackMemory: false);
```

#### Using ILogger (ASP.NET Core)

```csharp
// In your controller or service
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly AppDbContext _dbContext;

    public UserService(ILogger<UserService> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _dbContext.Users
            .ToListAsync()
            .MetrikaAsync("Get Users", thresholdMs: 500, logger: _logger);
    }
}
```

**ILogger Output:**

```
info: MyApp.Services.UserService[0]
      Get Users duration: 245 ms
```

---

### Custom Loggers

Implement your own logger by implementing `IMetrikaLogger`:

```csharp
using Metrika.Core.Abstractions;
using Metrika.Core.Models;

public class FileMetrikaLogger : IMetrikaLogger
{
    private readonly string _filePath;

    public FileMetrikaLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void LogMeasurement(
        MetrikaMeasurementResult result,
        MetrikaLocalization localization,
        MetrikaTimestampFormat timestampFormat)
    {
        var message = $"{result.Timestamp:yyyy-MM-dd HH:mm:ss} | " +
                      $"{result.Name} | " +
                      $"{result.ElapsedMilliseconds} ms";
        
        if (result.MemoryInfo != null)
        {
            message += $" | Memory: {result.MemoryInfo.MemoryDeltaMB:+0.00;-0.00} MB";
        }
        
        File.AppendAllText(_filePath, message + Environment.NewLine);
    }
}

// Register custom logger
MetrikaCore.RegisterLogger(new FileMetrikaLogger("performance.log"));

// You can register multiple loggers
MetrikaCore.RegisterLogger(new ConsoleMetrikaLogger());
MetrikaCore.RegisterLogger(new FileMetrikaLogger("performance.log"));
```

---

## 💡 Examples

### Example 1: Web API Performance Monitoring

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly AppDbContext _dbContext;

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dbContext.Users
            .ToListAsync()
            .MetrikaAsync("Get All Users", thresholdMs: 500, logger: _logger);

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        await _dbContext.Users.AddAsync(user);
        
        await _dbContext
            .SaveChangesAsync()
            .MetrikaAsync("Save User to DB", thresholdMs: 200, logger: _logger);

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
}
```

---

### Example 2: Multi-Step Process

```csharp
public async Task<ProcessResult> ProcessOrderAsync(int orderId)
{
    // Step 1: Fetch order
    var order = await _dbContext.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId)
        .MetrikaAsync("Fetch Order", thresholdMs: 300, logger: _logger);

    // Step 2: Validate order
    var isValid = new Func<bool>(() => ValidateOrder(order))
        .Metrika("Validate Order", logger: _logger);

    // Step 3: Calculate total
    var total = new Func<decimal>(() => CalculateTotal(order))
        .Metrika("Calculate Total", trackMemory: true, logger: _logger);

    // Step 4: Send notification
    await SendNotificationAsync(order)
        .MetrikaAsync("Send Notification", thresholdMs: 2000, logger: _logger);

    return new ProcessResult { Total = total, IsValid = isValid };
}
```

**Console Output:**

```
[METRIKA] [14:30:45] [INFO] Fetch Order duration: 245 ms
[METRIKA] [14:30:45] [INFO] Validate Order duration: 5 ms
[METRIKA] [14:30:45] [INFO] Calculate Total duration: 2 ms | Memory: +0.05 MB | GC: Gen0: 0, Gen1: 0, Gen2: 0
[METRIKA] [14:30:46] [INFO] Update Inventory duration: 320 ms
[METRIKA] [14:30:48] [INFO] Send Notification duration: 1850 ms
[METRIKA] [14:30:45] [INFO] Check Device Data Exists duration: 3 ms
[METRIKA] [14:30:45] [INFO] Count Device Data duration: 28 ms
[METRIKA] [14:30:45] [INFO] Fetch Device Data duration: 245 ms | Memory: +15.32 MB | GC: Gen0: 3, Gen1: 1, Gen2: 0
```
**Benefits:**
- ✅ Detected slow query (245ms, needs index)
- ✅ Identified memory usage (15MB for this dataset)
- ✅ Found 3 Gen0 GC collections (optimization opportunity)
---

### Example 3: Background Job with Memory Tracking

```csharp
public class DataSyncJob
{
    public async Task ExecuteAsync()
    {
        // Fetch from external API
        var externalData = await _httpClient
            .GetFromJsonAsync<List<Item>>("https://api.example.com/items")
            .MetrikaAsync("Fetch External Data", thresholdMs: 5000, trackMemory: true);

        // Transform data
        var transformed = new Func<List<Item>>(() => TransformData(externalData))
            .Metrika("Transform Data", trackMemory: true);

        // Bulk insert
        await _dbContext.Items
            .AddRangeAsync(transformed)
            .MetrikaAsync("Bulk Insert", thresholdMs: 3000);

        await _dbContext
            .SaveChangesAsync()
            .MetrikaAsync("Save Changes", thresholdMs: 5000, trackMemory: true);
    }
}
```

---

### Example 4: LINQ Query Optimization

```csharp
// Compare different query approaches
var approach1 = await _dbContext.Users
    .ToListAsync()
    .MetrikaAsync("Approach 1: Load All", trackMemory: true);

var approach2 = await _dbContext.Users
    .Where(u => u.IsActive)
    .ToListAsync()
    .MetrikaAsync("Approach 2: Filter in DB", trackMemory: true);

var approach3 = await _dbContext.Users
    .Where(u => u.IsActive)
    .Select(u => new { u.Id, u.Name })
    .ToListAsync()
    .MetrikaAsync("Approach 3: Filter and Project", trackMemory: true);
```

**Console Output:**

```
[METRIKA] [14:30:45] [INFO] Approach 1: Load All duration: 850 ms | Memory: +45.00 MB | GC: Gen0: 5, Gen1: 2, Gen2: 1
[METRIKA] [14:30:46] [INFO] Approach 2: Filter in DB duration: 320 ms | Memory: +12.50 MB | GC: Gen0: 2, Gen1: 0, Gen2: 0
[METRIKA] [14:30:46] [INFO] Approach 3: Filter and Project duration: 280 ms | Memory: +8.25 MB | GC: Gen0: 1, Gen1: 0, Gen2: 0
```

---

### Example 5: Localization

```csharp
// English
MetrikaCore.ConfigureLocalization(MetrikaLocalization.English);
new Func<int>(() => Calculate()).Metrika("Calculate", thresholdMs: 50);
// Output: [METRIKA] [14:30:45] [WARN] Calculate duration high: 100 ms (threshold: 50 ms)

// Turkish
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Turkish);
new Func<int>(() => Calculate()).Metrika("Hesapla", thresholdMs: 50);
// Output: [METRİKA] [14:30:45] [WARN] Hesapla süresi yüksek: 100 ms (eşik: 50 ms)

// Japanese
MetrikaCore.ConfigureLocalization(MetrikaLocalization.Japanese);
new Func<int>(() => Calculate()).Metrika("計算", thresholdMs: 50);
// Output: [メトリカ] [14:30:45] [WARN] 計算 処理時間が長い: 100 ミリ秒 (閾値: 50 ミリ秒)
```

---

### Example 6: Real-World Database Query Optimization
```csharp
public class DeviceDataService
{
    private readonly IRepository _repository;

    public async Task<List> GetDeviceDataAsync(
        int deviceId, 
        DateTime startDate, 
        DateTime endDate)
    {
        var queryable = await _repository.GetQueryableAsync();

        // Step 1: Check if any data exists (fastest check)
        var hasData = queryable
            .Where(x => x.DeviceId == deviceId)
            .AnyWithMetrika("Check Device Data Exists", thresholdMs: 50);

        if (!hasData)
        {
            return new List();
        }

        // Step 2: Get total count for pagination
        var totalCount = queryable
            .Where(x => x.DeviceId == deviceId && 
                       x.ReadDateTime >= startDate && 
                       x.ReadDateTime < endDate)
            .CountWithMetrika("Count Device Data", thresholdMs: 100);

        // Step 3: Fetch actual data
        var data = queryable
            .Where(x => x.DeviceId == deviceId && 
                       x.ReadDateTime >= startDate && 
                       x.ReadDateTime < endDate)
            .OrderBy(x => x.ReadDateTime)
            .ToListWithMetrika("Fetch Device Data", 
                thresholdMs: 500, 
                trackMemory: true);

        return data;
    }
}
```
---
## 🏗️ Project Structure

```
Metrika/
├── Metrika.Core/                    # Core library (50 KB)
│   ├── MetrikaCore.cs              # Main fluent API
│   ├── Abstractions/
│   │   └── IMetrikaLogger.cs       # Logger interface
│   └── Models/
│       ├── MetrikaMeasurementResult.cs
│       ├── MetrikaMemoryInfo.cs
│       ├── MetrikaLocalization.cs
│       └── MetrikaTimestampFormat.cs
├── Metrika.Core.Tests/             # Core tests (54 tests)
├── Metrika.Console/                # Console logger
│   ├── ConsoleMetrikaLogger.cs
│   └── MetrikaColorScheme.cs
├── Metrika.Console.Tests/          # Console tests (15 tests)
├── Metrika.Examples.ConsoleApp/    # Example application
└── assets/
    ├── metrika-icon.png            # Project icon
    └── screenshots/                # Screenshots
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test Metrika.Core.Tests
dotnet test Metrika.Console.Tests
```

**Test Coverage:**

- Metrika.Core: 54 tests ✅
- Metrika.Console: 15 tests ✅
- Total: 69 tests ✅

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'feat: Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🌟 Show Your Support

If you find this project helpful, please give it a ⭐️ on GitHub!

---

## 📞 Contact

- GitHub: [@alierguc1](https://github.com/alierguc1)
- Email: alierguc1@gmail.com

---

## 📋 Version History


### v1.4.0 (Current - November 2024)
- 🛡️ **New:** Robust exception handling in all measurement methods
- 🛡️ **New:** Exceptions logged with performance metrics before re-throwing
- 🛡️ **New:** Stopwatch and memory tracking complete even on failure
- 🛡️ **New:** `Exception` and `HasException` properties in `MetrikaMeasurementResult`
- 🎨 **Enhanced:** Console logger displays exceptions with red color and `[ERROR]` prefix
- 🧪 Comprehensive exception handling test coverage (6 new tests)
- 📝 Exception handling examples in console app

**Why v1.4.0 and not v1.3.2?**
Exception handling is a **new feature** (not a bug fix), so according to [Semantic Versioning](https://semver.org/), we increment the MINOR version (1.3.x → 1.4.0), not the PATCH version.

### v1.3.1  (November 2024)
- 📝 Documentation improvements
- 📚 Enhanced examples and guides

### v1.3.0 (November 2024)
- ✨ **New:** `SingleWithMetrika` - For queries expecting exactly one result
- ✨ **New:** `SingleOrDefaultWithMetrika` - For 0 or 1 expected results
- ✨ **New:** `LastWithMetrika` - Get last element from ordered queries
- ✨ **New:** `LastOrDefaultWithMetrika` - Get last element or null
- ✨ **New:** `LongCountWithMetrika` - Count large datasets (>2 billion records)
- 🧪 Comprehensive unit test coverage for new methods
- 📝 Enhanced XML documentation with detailed examples
- 📊 Real-world usage examples added

**Use Cases:**
- ID-based lookups with `Single`
- Email/username searches with `SingleOrDefault`
- Latest record queries with `Last`
- Big data counting with `LongCount`

### v1.2.0 (October 2024)
- ✨ Initial IQueryable extensions: `ToList`, `ToArray`, `First`, `FirstOrDefault`, `Count`, `Any`
- 🎨 Console logger with 4 customizable color schemes
- 📊 Memory tracking support with GC collection metrics
- 🌍 Multi-language localization (10+ languages)
- ⏰ Threshold-based warnings
- 🔧 Custom timestamp formats

### v1.0.0 (September 2024)
- 🎉 Initial release
- ⏱️ Basic performance measurement for sync/async operations
- 🔧 Fluent API design
- 📊 Custom logger support
- 🌍 English and Turkish localization
---




## 🎯 Roadmap

### Version 1.1 (Q1 2025)

- [ ] ASP.NET Core middleware integration
- [ ] SignalR logger for real-time monitoring
- [ ] Performance dashboard UI

### Version 1.2 (Q2 2025)

- [ ] Database logger (SQL Server, PostgreSQL)
- [ ] Elasticsearch integration
- [ ] Prometheus metrics exporter

### Version 2.0 (Q3 2025)

- [ ] Blazor component for visualization
- [ ] Advanced analytics and reporting
- [ ] Performance anomaly detection

---

<p align="center">
  <strong>Made with ❤️ by <a href="https://github.com/alierguc1">M. Ali Ergüç</a></strong>
</p>
