# 🚀 Result Pattern

[![Azure DevOps Board](https://img.shields.io/badge/Azure%20DevOps-Board-0078D7?logo=azure-devops)](https://dev.azure.com/tengtium-io/ResultPatterns/_boards/board/t/ResultPatterns%20Team/Stories)

A robust implementation of the **Result pattern** in C# for functional error handling. Instead of throwing exceptions, operations return `Result<T>` types that explicitly communicate success or failure, enabling more expressive error handling and better control flow.

## 📑 Table of Contents

- [Features](#-features)
- [Getting Started](#-getting-started)
- [Core Components](#-core-components)
- [Usage Examples](#-usage-examples)
- [Value Object Examples](#-value-object-examples)
- [Related Projects](#-related-projects)
- [License](#-license)

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔗 **Fluent Chaining** | Chain multiple operations with `Bind` and `Map` methods |
| ⚡ **Async Support** | Built-in `BindAsync` and `MapAsync` for async operations |
| ⚠️ **Rich Error Handling** | Structured errors with codes, messages, and error types |
| 🛡️ **Immutability** | Result objects are immutable once created |
| 🎯 **Pattern Matching** | Deconstruction support for C# pattern matching |
| 🔄 **Error Aggregation** | Combine multiple results and aggregate errors |

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 or later

### Installation

Add a project reference to `TestNest.ResultPattern.Domain`:

```xml
<ProjectReference Include="..\TestNest.ResultPattern.Domain\TestNest.ResultPattern.Domain.csproj" />
```

### Quick Start

```csharp
using TestNest.ResultPattern.Domain.Common;

// Success case
var success = Result<int>.Success(42);
Console.WriteLine(success.Value); // 42

// Failure case
var failure = Result<int>.Failure(ErrorType.Validation, "ERR001", "Value must be positive");
if (!failure.IsSuccess)
{
    Console.WriteLine(failure.Errors[0].Message); // "Value must be positive"
}

// Fluent chaining
var result = Result<int>.Success(10)
    .Bind(x => Result<int>.Success(x * 2))
    .Map(x => $"Result: {x}");
// result.Value == "Result: 20"
```

## 📦 Core Components

### Error Record

```csharp
public record Error(string Code, string Message);
```

A simple record type for storing error information with a code and message.

### ErrorType Enumeration

```csharp
public enum ErrorType
{
    None,           // No error (success state)
    Validation,     // Input validation errors
    NotFound,       // Resource not found
    Unauthorized,   // Authentication/authorization failures
    Conflict,       // Conflicting operations
    Internal,       // Internal/unexpected errors
    Aggregate,      // Combined errors from multiple results
    Invalid         // Invalid operation or state
}
```

### Result Class (Non-Generic)

Used when an operation has no return value but can succeed or fail.

```csharp
public sealed class Result
{
    public bool IsSuccess { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<Error> Errors { get; }

    // Factory Methods
    public static Result Success();
    public static Result Failure(ErrorType errorType, Error error);
    public static Result Failure(ErrorType errorType, string code, string message);
    public static Result Combine(params Result[] results);

    // Fluent Methods
    public Result Bind(Func<Result> bind);
    public Result<T> Bind<T>(Func<Result<T>> bind);
    public Result Map(Action map);
    public Result<T> Map<T>(Func<T> map);

    // Async Methods
    public Task<Result> BindAsync(Func<Task<Result>> bindAsync);
    public Task<Result<T>> BindAsync<T>(Func<Task<Result<T>>> bindAsync);
    public Task<Result> MapAsync(Func<Task> mapAsync);
    public Task<Result<T>> MapAsync<T>(Func<Task<T>> mapAsync);

    // Conversion
    public Result<T> ToResult<T>(T value);

    // Pattern Matching
    public void Deconstruct(out bool isSuccess, out ErrorType errorType, out IReadOnlyList<Error> errors);
}
```

### Result&lt;T&gt; Class (Generic)

Used when an operation returns a value on success.

```csharp
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<Error> Errors { get; }

    // Factory Methods
    public static Result<T> Success(T value);
    public static Result<T> Failure(ErrorType errorType, Error error);
    public static Result<T> Failure(ErrorType errorType, string code, string message);

    // Fluent Methods
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> bind);
    public Result<TNew> Map<TNew>(Func<T, TNew> map);

    // Async Methods
    public Task<Result<TNew>> BindAsync<TNew>(Func<T, Task<Result<TNew>>> bindAsync);
    public Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapAsync);

    // Utilities
    public T EnsureSuccess();  // Throws if failure
    public bool TryGetValue(out T? value, out IReadOnlyList<Error> errors);
    public Result ToResult();  // Convert to non-generic Result

    // Implicit Conversion
    public static implicit operator Result<T>(T value);

    // Pattern Matching
    public void Deconstruct(out bool isSuccess, out T? value, out ErrorType errorType, out IReadOnlyList<Error> errors);
}
```

## 📌 Usage Examples

### Basic Success and Failure

```csharp
// Non-generic Result
var success = Result.Success();
var failure = Result.Failure(ErrorType.Validation, "ERR001", "Invalid input");

// Generic Result<T>
var valueSuccess = Result<int>.Success(42);
var valueFailure = Result<int>.Failure(ErrorType.NotFound, "ERR404", "Resource not found");

// Implicit conversion (creates success result)
Result<string> implicitResult = "Hello World";
```

### Fluent Chaining with Bind and Map

```csharp
// Bind: Chain operations that return Result<T>
var result = Result<int>.Success(10)
    .Bind(x => Result<int>.Success(x + 5))      // 15
    .Bind(x => Result<int>.Success(x * 2));     // 30

// Map: Transform the value directly
var transformed = Result<int>.Success(42)
    .Map(x => x.ToString())                      // "42"
    .Map(s => $"Value: {s}");                   // "Value: 42"

// Combining Bind and Map
var combined = Result<int>.Success(100)
    .Bind(x => x > 0
        ? Result<int>.Success(x)
        : Result<int>.Failure(ErrorType.Validation, "ERR", "Must be positive"))
    .Map(x => x * 2);
```

### Error Propagation

```csharp
// Errors propagate through the chain automatically
var result = Result<int>.Success(10)
    .Bind(x => Result<int>.Success(x + 5))
    .Bind(x => Result<int>.Failure(ErrorType.Validation, "ERR", "Something went wrong"))
    .Bind(x => Result<int>.Success(x * 2))  // This is skipped
    .Map(x => x.ToString());                 // This is also skipped

// result.IsSuccess == false
// result.Errors contains the error from the failed Bind
```

### Combining Multiple Results

```csharp
var result1 = Result.Success();
var result2 = Result.Failure(ErrorType.Validation, "ERR001", "First error");
var result3 = Result.Failure(ErrorType.Validation, "ERR002", "Second error");

var combined = Result.Combine(result1, result2, result3);
// combined.IsSuccess == false
// combined.ErrorType == ErrorType.Aggregate
// combined.Errors.Count == 2
```

### Async Operations

```csharp
var result = await Result<int>.Success(10)
    .BindAsync(async x =>
    {
        await Task.Delay(100);
        return Result<int>.Success(x * 2);
    })
    .MapAsync(async x =>
    {
        await Task.Delay(100);
        return $"Result: {x}";
    });
```

### Pattern Matching with Deconstruction

```csharp
var result = Result<int>.Success(42);

var (isSuccess, value, errorType, errors) = result;

if (isSuccess)
{
    Console.WriteLine($"Success: {value}");
}
else
{
    Console.WriteLine($"Failed with {errors.Count} errors");
}
```

### Error Handling

```csharp
var result = Result<int>.Failure(ErrorType.Validation, "ERR003", "Value out of range");

// Check and handle errors
if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"[{error.Code}] {error.Message}");
    }
}

// Or use TryGetValue
if (result.TryGetValue(out var value, out var errors))
{
    Console.WriteLine($"Got value: {value}");
}
else
{
    Console.WriteLine($"Failed: {errors[0].Message}");
}

// Or throw on failure
try
{
    var value = result.EnsureSuccess();
}
catch (ResultException ex)
{
    Console.WriteLine($"Operation failed: {ex.Message}");
}
```

## 📦 Value Object Examples

### Price Value Object

```csharp
public sealed class Price : ValueObject
{
    public decimal StandardPrice { get; }
    public decimal PeakPrice { get; }
    public Currency Currency => Currency.PHP;

    public static Price Empty => new Price();
    public static Price Zero => new Price(0, 0);

    public static Result<Price> Create(decimal standardPrice, decimal peakPrice)
    {
        var errors = new List<Error>();

        if (standardPrice < 0)
            errors.Add(new Error("NegativeStandardPrice", "Standard price cannot be negative"));

        if (peakPrice < 0)
            errors.Add(new Error("NegativePeakPrice", "Peak price cannot be negative"));

        if (standardPrice >= 0 && peakPrice >= 0 && peakPrice < standardPrice)
            errors.Add(new Error("PeakBelowStandard", "Peak price must be >= standard price"));

        return errors.Any()
            ? Result<Price>.Failure(ErrorType.Validation, errors)
            : Result<Price>.Success(new Price(standardPrice, peakPrice));
    }

    public Result<Price> WithStandardPrice(decimal newPrice) => Create(newPrice, PeakPrice);
    public Result<Price> WithPeakPrice(decimal newPrice) => Create(StandardPrice, newPrice);
}

// Usage
var priceResult = Price.Create(100m, 150m);
if (priceResult.IsSuccess)
{
    Console.WriteLine(priceResult.Value); // ₱100.00 / ₱150.00 (Peak)
}
```

### AccommodationPrice Value Object (Composition)

```csharp
public sealed class AccommodationPrice : ValueObject
{
    public Price Price { get; }
    public decimal CleaningFee { get; }

    public static Result<AccommodationPrice> Create(decimal standardPrice, decimal peakPrice, decimal cleaningFee)
        => Price.Create(standardPrice, peakPrice)
            .Bind(price => Create(price, cleaningFee));

    public static Result<AccommodationPrice> Create(Price price, decimal cleaningFee)
    {
        if (price == null || price == Price.Empty)
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, "NullPrice", "Price is required");

        if (cleaningFee < 0)
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, "NegativeFee", "Cleaning fee cannot be negative");

        return Result<AccommodationPrice>.Success(new AccommodationPrice(price, cleaningFee));
    }
}

// Usage with chaining
var result = AccommodationPrice.Create(100m, 150m, 25m);
// Validation errors from Price.Create automatically propagate
```

### EstablishmentAccommodation Entity

```csharp
public sealed class EstablishmentAccommodation
{
    public AccommodationPrice Price { get; }

    public static Result<EstablishmentAccommodation> Create(AccommodationPrice price)
    {
        if (price == AccommodationPrice.Empty)
            return Result<EstablishmentAccommodation>.Failure(
                ErrorType.Validation, "InvalidPrice", "Valid accommodation price is required");

        return Result<EstablishmentAccommodation>.Success(new EstablishmentAccommodation(price));
    }

    public Result<EstablishmentAccommodation> UpdatePrice(AccommodationPrice newPrice)
        => Create(newPrice);
}

// Full validation chain
var result = AccommodationPrice.Create(-100m, 150m, 25m)
    .Bind(price => EstablishmentAccommodation.Create(price));
// result.IsSuccess == false
// result.Errors contains "NegativeStandardPrice" from Price validation
```

## 🔗 Related Projects

- [TestNest.StronglyTypeId](https://github.com/DanteTuraSalvador/TestNest.StronglyTypeId) - Strongly typed ID implementations
- [TestNest.ValueObjects](https://github.com/DanteTuraSalvador/TestNest.ValueObjects) - Value Object pattern implementations
- [TestNest.SmartEnums](https://github.com/DanteTuraSalvador/TestNest.SmartEnums) - Smart Enum pattern with state machine

## 📜 License

This project is open-source and free to use.
