using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions; // Ensure this is correct for Error and ErrorType

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

    // ========== Core Factory Methods ========== //
    public static Result Success() => new(true, ErrorType.None, Array.Empty<Error>());

    public static Result Failure(ErrorType errorType, Error error)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result));

        ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message);  // Validate error before creating Result

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
            ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message); // Validate errors before creating Result
        }

        return new Result(false, errorType, errorList);
    }

    // Convenience overload for string errors
    public static Result Failure(ErrorType errorType, string code, string message) =>
        Failure(errorType, new Error(code, message));

    // ========== Core Operations ========== //
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => !r.IsSuccess).ToList();
        return failures.Any()
            ? Failure(ErrorType.Aggregate, failures.SelectMany(r => r.Errors))
            : Success();
    }

    public Result<T> ToResult<T>(T value) =>
        IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(ErrorType, Errors);

    // ========== Fluent Chaining ========== //
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

    // ========== Async Support ========== //
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

    // ========== Pattern Matching ========== //
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

    // ========== Core Factory Methods ========== //
    public static Result<T> Success(T value) =>
        value is null
            ? throw ResultException.NullValue(typeof(T))
            : new Result<T>(true, ErrorType.None, value, Array.Empty<Error>());

    public static Result<T> Failure(ErrorType errorType, Error error)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result<T>));

        ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message);  // Validate error before creating Result<T>

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
            ResultException.ValidateErrorCodeAndMessage(error.Code, error.Message); // Validate errors before creating Result<T>
        }

        return new Result<T>(false, errorType, default, errorList);
    }

    // Convenience overload for string errors
    public static Result<T> Failure(ErrorType errorType, string code, string message) =>
        Failure(errorType, new Error(code, message));

    // ========== Fluent Chaining ========== //
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> bind) =>
        IsSuccess ? bind(Value!) : Result<TNew>.Failure(ErrorType, Errors);

    public Result<TNew> Map<TNew>(Func<T, TNew> map) =>
        IsSuccess ? Result<TNew>.Success(map(Value!)) : Result<TNew>.Failure(ErrorType, Errors);

    // ========== Async Support ========== //
    public async Task<Result<TNew>> BindAsync<TNew>(Func<T, Task<Result<TNew>>> bindAsync) =>
        IsSuccess ? await bindAsync(Value!) : Result<TNew>.Failure(ErrorType, Errors);

    public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapAsync) =>
        IsSuccess ? Result<TNew>.Success(await mapAsync(Value!)) : Result<TNew>.Failure(ErrorType, Errors);

    // ========== Utilities ========== //
    public T EnsureSuccess() =>
      IsSuccess ? Value! : throw ResultException.Failure(typeof(T), Errors.Select(e => e.Message));

    public bool TryGetValue(out T? value, out IReadOnlyList<Error> errors)
    {
        value = IsSuccess ? Value : default;
        errors = Errors;
        return IsSuccess;
    }

    // ========== Pattern Matching ========== //
    public void Deconstruct(out bool isSuccess, out T? value, out ErrorType errorType, out IReadOnlyList<Error> errors)
    {
        isSuccess = IsSuccess;
        value = Value;
        errorType = ErrorType;
        errors = Errors;
    }

    // ========== Implicit Conversion ========== //
    public static implicit operator Result<T>(T value) => Success(value);


    public Result ToResult()
    {
        return IsSuccess ? Result.Success() : Result.Failure(ErrorType, Errors);
    }
}

//public sealed class Result
//{
//    public bool IsSuccess { get; }
//    public ErrorType ErrorType { get; }
//    public IReadOnlyList<string> Errors { get; }

//    private Result(bool isSuccess, ErrorType errorType, IReadOnlyList<string> errors)
//    {
//        IsSuccess = isSuccess;
//        ErrorType = errorType;
//        Errors = errors ?? Array.Empty<string>();
//    }

//    public static Result Success() => new(true, ErrorType.None, Array.Empty<string>());

//    public static Result Failure(ErrorType errorType, string error)
//    {
//        if (errorType == ErrorType.None)
//            throw ResultException.InvalidErrorType(typeof(Result));

//        if (string.IsNullOrWhiteSpace(error))
//            throw ResultException.EmptyErrors(typeof(Result));

//        return new Result(false, errorType, new[] { error });
//    }

//    public static Result Failure(ErrorType errorType, IEnumerable<string> errors)
//    {
//        if (errorType == ErrorType.None)
//            throw ResultException.InvalidErrorType(typeof(Result));

//        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
//                        ?? throw new ArgumentNullException(nameof(errors));

//        if (!errorList.Any())
//            throw ResultException.EmptyErrors(typeof(Result));

//        return new Result(false, errorType, errorList);
//    }

//    public static Result Combine(params Result[] results)
//    {
//        var failures = results.Where(r => !r.IsSuccess).ToList();
//        return failures.Any()
//            ? Failure(ErrorType.Aggregate, failures.SelectMany(r => r.Errors))
//            : Success();
//    }

//    public Result<T> ToResult<T>(T value) =>
//        IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(ErrorType, Errors);

//    // Bind method inside Result class (as before)
//    public static Result<TNew> Bind<T, TNew>(Result<T> result, Func<T, Result<TNew>> bind)
//    {
//        if (result == null) throw new ArgumentNullException(nameof(result));
//        if (bind == null) throw new ArgumentNullException(nameof(bind));

//        if (result.IsSuccess)
//        {
//            return bind(result.Value);
//        }

//        return Result<TNew>.Failure(result.ErrorType, result.Errors);
//    }

//    // Map method inside Result class
//    public static Result<TNew> Map<T, TNew>(Result<T> result, Func<T, TNew> map)
//    {
//        if (result == null) throw new ArgumentNullException(nameof(result));
//        if (map == null) throw new ArgumentNullException(nameof(map));

//        if (result.IsSuccess)
//        {
//            // Transform the value if successful
//            return Result<TNew>.Success(map(result.Value));
//        }

//        // If the result is not successful, return the failure as it is
//        return Result<TNew>.Failure(result.ErrorType, result.Errors);
//    }
//}

//public sealed class Result<T>
//{
//    public bool IsSuccess { get; }
//    public T Value { get; }
//    public ErrorType ErrorType { get; }
//    public IReadOnlyList<string> Errors { get; }

//    private Result(bool isSuccess, ErrorType errorType, T value, IReadOnlyList<string> errors)
//    {
//        IsSuccess = isSuccess;
//        ErrorType = errorType;
//        Value = value;
//        Errors = errors ?? Array.Empty<string>();
//    }

//    public static Result<T> Success(T value)
//    {
//        return value is null
//            ? throw ResultException.NullValue(typeof(T))
//            : new Result<T>(true, ErrorType.None, value, Array.Empty<string>());
//    }

//    public static Result<T> Failure(ErrorType errorType, string error)
//    {
//        if (errorType == ErrorType.None)
//            throw ResultException.InvalidErrorType(typeof(Result<T>));

//        if (string.IsNullOrWhiteSpace(error))
//            throw ResultException.EmptyErrors(typeof(Result<T>));

//        return new Result<T>(false, errorType, default!, new[] { error });
//    }

//    public static Result<T> Failure(ErrorType errorType, IEnumerable<string> errors)
//    {
//        if (errorType == ErrorType.None)
//            throw ResultException.InvalidErrorType(typeof(Result<T>));

//        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
//                        ?? throw new ArgumentNullException(nameof(errors));

//        if (!errorList.Any())
//            throw ResultException.EmptyErrors(typeof(Result<T>));

//        return new Result<T>(false, errorType, default!, errorList);
//    }

//    public static Result<T> Combine(params Result<T>[] results)
//    {
//        var failures = results.Where(r => !r.IsSuccess).ToList();
//        if (failures.Any())
//            return Failure(ErrorType.Aggregate, failures.SelectMany(r => r.Errors));

//        var validResult = results.FirstOrDefault(r => r.IsSuccess);
//        return validResult ?? throw ResultException.NoValidResults(typeof(Result<T>));
//    }

//    public T EnsureSuccess() => IsSuccess ? Value : throw ResultException.Failure(typeof(T), Errors);

//    public bool TryGetValue(out T value, out IReadOnlyList<string> errors)
//    {
//        value = IsSuccess ? Value : default!;
//        errors = Errors;
//        return IsSuccess;
//    }

//    // Bind method: Allows chaining of operations returning Result<TNew>
//    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> bind)
//    {
//        if (bind == null)
//            throw new ArgumentNullException(nameof(bind));

//        if (IsSuccess)
//        {
//            return bind(Value);
//        }

//        return Result<TNew>.Failure(ErrorType, Errors);
//    }

//    // Map method: Allows transforming a value from T to TNew if successful
//    public Result<TNew> Map<TNew>(Func<T, TNew> map)
//    {
//        if (map == null)
//            throw new ArgumentNullException(nameof(map));

//        if (IsSuccess)
//        {
//            return Result<TNew>.Success(map(Value));
//        }

//        return Result<TNew>.Failure(ErrorType, Errors);
//    }
//}
