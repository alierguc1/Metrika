
# Metrika

High-performance measurement library for .NET applications.

## Installation
```bash
dotnet add package Metrika.Core
dotnet add package Metrika.Console
```

## Quick Start
```csharp
using Metrika.Core.Extensions;

var result = await GetDataAsync()
    .MetrikaAsync("Get Data");
```
