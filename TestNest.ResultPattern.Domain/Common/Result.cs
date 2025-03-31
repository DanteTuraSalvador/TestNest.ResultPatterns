using TestNest.ResultPattern.Domain.Exceptions;

namespace TestNest.ResultPattern.Domain.Common;

public sealed class Result
{
    public bool IsSuccess { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, ErrorType errorType, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result Success() => new(true, ErrorType.None, Array.Empty<string>());

    public static Result Failure(ErrorType errorType, string error)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result));

        if (string.IsNullOrWhiteSpace(error))
            throw ResultException.EmptyErrors(typeof(Result));

        return new Result(false, errorType, new[] { error });
    }

    public static Result Failure(ErrorType errorType, IEnumerable<string> errors)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result));

        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
                        ?? throw new ArgumentNullException(nameof(errors));

        if (!errorList.Any())
            throw ResultException.EmptyErrors(typeof(Result));

        return new Result(false, errorType, errorList);
    }

    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => !r.IsSuccess).ToList();
        return failures.Any()
            ? Failure(ErrorType.Aggregate, failures.SelectMany(r => r.Errors))
            : Success();
    }

    public Result<T> ToResult<T>(T value) =>
        IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(ErrorType, Errors);
}



public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, ErrorType errorType, T value, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Value = value;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result<T> Success(T value)
    {
        return value is null
            ? throw ResultException.NullValue(typeof(T))
            : new Result<T>(true, ErrorType.None, value, Array.Empty<string>());
    }

    public static Result<T> Failure(ErrorType errorType, string error)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result<T>));

        if (string.IsNullOrWhiteSpace(error))
            throw ResultException.EmptyErrors(typeof(Result<T>));

        return new Result<T>(false, errorType, default!, new[] { error });
    }

    public static Result<T> Failure(ErrorType errorType, IEnumerable<string> errors)
    {
        if (errorType == ErrorType.None)
            throw ResultException.InvalidErrorType(typeof(Result<T>));

        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
                        ?? throw new ArgumentNullException(nameof(errors));

        if (!errorList.Any())
            throw ResultException.EmptyErrors(typeof(Result<T>));

        return new Result<T>(false, errorType, default!, errorList);
    }

    public static Result<T> Combine(params Result<T>[] results)
    {
        var failures = results.Where(r => !r.IsSuccess).ToList();
        if (failures.Any())
            return Failure(ErrorType.Aggregate, failures.SelectMany(r => r.Errors));

        var validResult = results.FirstOrDefault(r => r.IsSuccess);
        return validResult ?? throw ResultException.NoValidResults(typeof(Result<T>));
    }

    public T EnsureSuccess() => IsSuccess ? Value : throw ResultException.Failure(typeof(T), Errors);

    public bool TryGetValue(out T value, out IReadOnlyList<string> errors)
    {
        value = IsSuccess ? Value : default!;
        errors = Errors;
        return IsSuccess;
    }
}
