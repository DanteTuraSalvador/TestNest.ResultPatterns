# 🚀 Result Pattern

[![Azure DevOps Board](https://img.shields.io/badge/Azure%20DevOps-Board-0078D7?logo=azure-devops)](https://dev.azure.com/tengtium-io/ResultPatterns/_boards/board/t/ResultPatterns%20Team/Stories)

This repository contains an implementation of the **Result pattern** in C#. The Result pattern is a way to handle operations that may fail by returning a `Result<T>` type rather than throwing exceptions. It allows for more expressive error handling and better control over flow without relying on traditional exception handling.

## This implementation includes:

- ✅ **Result class**: Used to wrap success and failure outcomes of operations.
- ✅ **Result<T> class**: A generic version that holds a value and errors for specific operations.
- ✅ **Error class**: Represents detailed error information.
- ✅ **Bind and Map methods**: For fluent chaining and handling asynchronous operations.

## Features

- 🔗 **Fluent Chaining**: Easily chain multiple operations while handling errors cleanly.
- ⚡ **Async Support**: Built-in support for asynchronous operations.
- ⚠️ **Error Handling**: Encapsulates errors with error codes and messages, providing detailed context.
- 🛡️ **Immutability**: Result objects are immutable once created.

## 📌 Core Implementation

### 🔹 ValueObject Base Class
```csharp
using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions; 

public sealed class Result
{
    public bool IsSuccess { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<Error> Errors { get; }

    private Result(bool isSuccess, ErrorType errorType, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Errors = errors ?? Array.Empty<Error>();
    }

    
    public static Result Success() => new(true, ErrorType.None, Array.Empty<Error>());

    public static Result Failure(ErrorType errorType, Error error)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result));

        ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message);  

        return new Result(false, errorType, new[] { error });
    }

    public static Result Failure(ErrorType errorType, IEnumerable<Error> errors)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result));

        var errorList = errors?.Where(e => e != null).ToList()
                        ?? throw new ArgumentNullException(nameof(errors));

        if (!errorList.Any())
            throw ResultException.EmptyErrors(typeof(Result));

        foreach (var error in errorList)
        {
            ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message); 
        }

        return new Result(false, errorType, errorList);
    }

    
    public static Result Failure(ErrorType errorType, string code, string message) =>
        Failure(errorType, new Error(code, message));

    
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => !r.IsSuccess).ToList();
        return failures.Any()
            ? Failure(ErrorType.Aggregate, failures.SelectMany(r => r.Errors))
            : Success();
    }

    public Result<T> ToResult<T>(T value) =>
        IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(ErrorType, Errors);

    public Result Bind(Func<Result> bind) =>
        IsSuccess ? bind() : this;

    public Result<T> Bind<T>(Func<Result<T>> bind) =>
        IsSuccess ? bind() : Result<T>.Failure(ErrorType, Errors);

    public Result Map(Action map)
    {
        if (IsSuccess) map();
        return this;
    }

    public Result<T> Map<T>(Func<T> map) =>
        IsSuccess ? Result<T>.Success(map()) : Result<T>.Failure(ErrorType, Errors);

    public async Task<Result> BindAsync(Func<Task<Result>> bindAsync) =>
        IsSuccess ? await bindAsync() : this;

    public async Task<Result<T>> BindAsync<T>(Func<Task<Result<T>>> bindAsync) =>
        IsSuccess ? await bindAsync() : Result<T>.Failure(ErrorType, Errors);

    public async Task<Result> MapAsync(Func<Task> mapAsync)
    {
        if (IsSuccess) await mapAsync();
        return this;
    }

    public async Task<Result<T>> MapAsync<T>(Func<Task<T>> mapAsync) =>
        IsSuccess ? Result<T>.Success(await mapAsync()) : Result<T>.Failure(ErrorType, Errors);

    public void Deconstruct(out bool isSuccess, out ErrorType errorType, out IReadOnlyList<Error> errors)
    {
        isSuccess = IsSuccess;
        errorType = ErrorType;
        errors = Errors;
    }
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<Error> Errors { get; }

    private Result(bool isSuccess, ErrorType errorType, T? value, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Value = value;
        Errors = errors ?? Array.Empty<Error>();
    }

    public static Result<T> Success(T value) =>
        value is null
            ? throw ResultException.NullValue(typeof(T))
            : new Result<T>(true, ErrorType.None, value, Array.Empty<Error>());

    public static Result<T> Failure(ErrorType errorType, Error error)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result<T>));

        ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message);  

        return new Result<T>(false, errorType, default, new[] { error });
    }

    public static Result<T> Failure(ErrorType errorType, IEnumerable<Error> errors)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result<T>));

        var errorList = errors?.Where(e => e != null).ToList()
                        ?? throw new ArgumentNullException(nameof(errors));

        if (!errorList.Any())
            throw ResultException.EmptyErrors(typeof(Result<T>));

        foreach (var error in errorList)
        {
            ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message); 
        }

        return new Result<T>(false, errorType, default, errorList);
    }

    
    public static Result<T> Failure(ErrorType errorType, string code, string message) =>
        Failure(errorType, new Error(code, message));

    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> bind) =>
        IsSuccess ? bind(Value!) : Result<TNew>.Failure(ErrorType, Errors);

    public Result<TNew> Map<TNew>(Func<T, TNew> map) =>
        IsSuccess ? Result<TNew>.Success(map(Value!)) : Result<TNew>.Failure(ErrorType, Errors);

    public async Task<Result<TNew>> BindAsync<TNew>(Func<T, Task<Result<TNew>>> bindAsync) =>
        IsSuccess ? await bindAsync(Value!) : Result<TNew>.Failure(ErrorType, Errors);

    public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapAsync) =>
        IsSuccess ? Result<TNew>.Success(await mapAsync(Value!)) : Result<TNew>.Failure(ErrorType, Errors);

    public T EnsureSuccess() =>
      IsSuccess ? Value! : throw ResultException.Failure(typeof(T), Errors.Select(e => e.Message));

    public bool TryGetValue(out T? value, out IReadOnlyList<Error> errors)
    {
        value = IsSuccess ? Value : default;
        errors = Errors;
        return IsSuccess;
    }

    public void Deconstruct(out bool isSuccess, out T? value, out ErrorType errorType, out IReadOnlyList<Error> errors)
    {
        isSuccess = IsSuccess;
        value = Value;
        errorType = ErrorType;
        errors = Errors;
    }

    public static implicit operator Result<T>(T value) => Success(value);


    public Result ToResult()
    {
        return IsSuccess ? Result.Success() : Result.Failure(ErrorType, Errors);
    }
}

```
## 📌 Usage Examples
### ✅ Result Class
The Result class is used when there is no return value but you still want to communicate whether an operation succeeded or failed. It also carries any relevant error information in case of failure.
```csharp
var result = Result.Success(); // Indicates success
var errorResult = Result.Failure(ErrorType.Validation, "ERR001", "Invalid input"); // Indicates failure with error
```

### ✅ Result<T> Class
The Result<T> class is a generic version that wraps a successful result with a value of type T. If the operation fails, it contains error information, similar to the Result class.
```csharp
var result = Result<int>.Success(42); // Success with value
var errorResult = Result<int>.Failure(ErrorType.Validation, "ERR002", "Invalid data"); // Failure with error
```

### ✅ Fluent Chaining with Bind and Map
You can chain operations using Bind and Map methods, which allow you to transform or pass the result through multiple stages. If any stage fails, the chain stops immediately, and the failure result is returned.
```csharp
var result = Result<int>.Success(42)
    .Bind(x => Result<int>.Success(x + 1)) // Adds 1 to the result
    .Map(x => x * 2); // Multiplies the result by 2
```
### ✅ Error Handling
The error handling is done using the Error class, which stores error codes and messages. You can check for errors using IsSuccess and handle them accordingly.
```csharp
var result = Result<int>.Failure(ErrorType.Validation, "ERR003", "Out of bounds");
if (!result.IsSuccess)
{
    Console.WriteLine($"Error: {result.Errors.First().Message}");
}
```

## 📌 Implementing Value Objects and Concrete Class
### ✅ Price Value Object 
```csharp
public sealed class Price : ValueObject
{
    private static readonly Lazy<Price> _lazyEmpty = new(() => new Price());
    private static readonly Lazy<Price> _lazyZero = new(() => new Price(0, 0)); // ✅ Fix Zero Initialization

    public static Price Empty => _lazyEmpty.Value;
    public static Price Zero => _lazyZero.Value;

    public decimal StandardPrice { get; }
    public decimal PeakPrice { get; }
    public Currency Currency => Currency.PHP; // Fixed to PHP

    private Price() => (StandardPrice, PeakPrice) = (0, 0);
    private Price(decimal standardPrice, decimal peakPrice)  => (StandardPrice, PeakPrice) = (standardPrice, peakPrice);

    public static Result<Price> Create(decimal standardPrice, decimal peakPrice)
    {
        var errors = new List<Error>();

        if (standardPrice < 0)
        {
            var exception = PriceException.NegativeStandardPrice();
            errors.Add(new Error(exception.Code.ToString(), exception.Message));
        }

        if (peakPrice < 0)
        {
            var exception = PriceException.NegativePeakPrice();
            errors.Add(new Error(exception.Code.ToString(), exception.Message));
        }

        if (standardPrice >= 0 && peakPrice >= 0 && peakPrice < standardPrice)
        {
            var exception = PriceException.PeakBelowStandard();
            errors.Add(new Error(exception.Code.ToString(), exception.Message));
        }

        if (errors.Any())
        {
            return Result<Price>.Failure(ErrorType.Validation, errors);
        }

        return Result<Price>.Success(new Price(standardPrice, peakPrice));
    }

    public Result<Price> WithStandardPrice(decimal newStandardPrice) => Create(newStandardPrice, PeakPrice);
    public Result<Price> WithPeakPrice(decimal newPeakPrice) => Create(StandardPrice, newPeakPrice);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return StandardPrice;
        yield return PeakPrice;
        yield return Currency;
    }

    public override string ToString() => $"{Currency.Symbol}{StandardPrice:F2} / {Currency.Symbol}{PeakPrice:F2} (Peak)";
}
```
### ✅ AccommodationPrice Value Object
```csharp
public sealed class AccommodationPrice : ValueObject
{
    public Price Price { get; }
    public decimal CleaningFee { get; }

    private static readonly Lazy<AccommodationPrice> _empty = new(() => new AccommodationPrice(Price.Empty, 0));
    private static readonly Lazy<AccommodationPrice> _zero = new(() => new AccommodationPrice(Price.Zero, 0));

    public static AccommodationPrice Empty => _empty.Value;
    public static AccommodationPrice Zero => _zero.Value;

    private AccommodationPrice() => (Price, CleaningFee) = (Price.Empty, 0);

    private AccommodationPrice(Price price, decimal cleaningFee)
        => (Price, CleaningFee) = (price, cleaningFee);

    public static Result<AccommodationPrice> Create(decimal standardPrice, decimal peakPrice, decimal cleaningFee)
        => Price.Create(standardPrice, peakPrice).Bind(price => Create(price, cleaningFee));

    public static Result<AccommodationPrice> Create(Price? price, decimal cleaningFee)
    {
        var errors = new List<Error>();
        
        if (price == null)
        {
            var exception = AccommodationPriceException.NullPrice();
            errors.Add(new Error(exception.Code.ToString(), exception.Message.ToString()));
        }
        if (price == Price.Empty)
        {
            var exception = AccommodationPriceException.NullPrice();
            errors.Add(new Error(exception.Code.ToString(), exception.Message.ToString()));
        }
        if (cleaningFee < 0)
        {
            var exception = AccommodationPriceException.NegativeCleaningFee();
            errors.Add(new Error(exception.Code.ToString(), exception.Message.ToString()));
        }

        if (errors.Any())
        {
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, errors);
        }

        return Result<AccommodationPrice>.Success(new AccommodationPrice(price!, cleaningFee));
    }

    public static Result<AccommodationPrice> Create(Result<Price> priceResult, decimal cleaningFee)
        => !priceResult.IsSuccess ? Result<AccommodationPrice>.Failure(priceResult.ErrorType, priceResult.Errors)
            : Create(priceResult.Value!, cleaningFee);

    public Result<AccommodationPrice> WithPrice(Price newPrice)
    {
        if (this == Empty)
        {
            var exception = AccommodationPriceException.CannotModifyEmpty();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        if (newPrice is null)
        {
            var exception = AccommodationPriceException.NullPrice();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        return Create(newPrice, CleaningFee);
    }

    public Result<AccommodationPrice> WithPrice(Result<Price> newPriceResult)
        => newPriceResult.IsSuccess ? Create(newPriceResult.Value!, CleaningFee)
            : Result<AccommodationPrice>.Failure(newPriceResult.ErrorType, newPriceResult.Errors);

    public Result<AccommodationPrice> WithCleaningFee(decimal newCleaningFee)
    {
        if (this == Empty)
        {
            var exception = AccommodationPriceException.CannotModifyEmpty();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        if (newCleaningFee < 0)
        {
            var exception = AccommodationPriceException.NegativeCleaningFee();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        return Create(Price, newCleaningFee);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Price;
        yield return CleaningFee;
    }

    public override string ToString() =>
        $"{Price} + Cleaning Fee: {Price.Currency.Symbol}{CleaningFee:F2}";
}
```
### ✅ EstablishmentAccomodationPrice Concrete Class
```csharp
public sealed class EstablishmentAccommodation
{
    public AccommodationPrice Price { get; }

    private static readonly Lazy<EstablishmentAccommodation> _empty = new(() => new EstablishmentAccommodation());
    public static EstablishmentAccommodation Empty => _empty.Value;

    private EstablishmentAccommodation() => Price = AccommodationPrice.Empty;

    private EstablishmentAccommodation(AccommodationPrice price) => Price = price;

    public static Result<EstablishmentAccommodation> Create(Result<AccommodationPrice> priceResult)
    {
        if (priceResult.IsSuccess)
        {
            return Result<EstablishmentAccommodation>
                .Success(new EstablishmentAccommodation(priceResult.Value!));
        }

        return Result<EstablishmentAccommodation>
            .Failure(priceResult.ErrorType, priceResult.Errors);
    }


    public static Result<EstablishmentAccommodation> Create(AccommodationPrice price)
    {
        if (price == AccommodationPrice.Empty)
        {
            return Result<EstablishmentAccommodation>
                .Failure(ErrorType.Validation, 
                    new Error(EstablishmentAccommodationException.InvalidAccommodationPrice().Code.ToString(),
                              EstablishmentAccommodationException.InvalidAccommodationPrice().Message));
        }

        return Result<EstablishmentAccommodation>
            .Success(new EstablishmentAccommodation(price));
    }


    public Result<EstablishmentAccommodation> UpdatePrice(AccommodationPrice newPrice)
    {
        if (newPrice == AccommodationPrice.Empty)
        {
            return Result<EstablishmentAccommodation>
                .Failure(ErrorType.Validation, 
                    new Error(EstablishmentAccommodationException.InvalidAccommodationPrice().Code.ToString(),
                              EstablishmentAccommodationException.InvalidAccommodationPrice().Message));
        }

        return Result<EstablishmentAccommodation>
            .Success(new EstablishmentAccommodation(newPrice));
    }


    public Result<EstablishmentAccommodation> UpdatePrice(Result<AccommodationPrice> newPriceResult)
    {
        if (newPriceResult.IsSuccess)
        {
            return Result<EstablishmentAccommodation>
                .Success(new EstablishmentAccommodation(newPriceResult.Value!));
        }

        return Result<EstablishmentAccommodation>
            .Failure(newPriceResult.ErrorType, newPriceResult.Errors);
    }
}
```
## 🔗 Related Projects

- [TestNest.StronglyTypeId](https://github.com/DanteTuraSalvador/TestNest.StronglyTypeId) - Strongly typed ID implementations
- [TestNest.ValueObjects](https://github.com/DanteTuraSalvador/TestNest.ValueObjects) - Value Object pattern implementations
- [TestNest.SmartEnums](https://github.com/DanteTuraSalvador/TestNest.SmartEnums) - Smart Enum pattern with state machine

## 📜 License

This project is open-source and free to use.


