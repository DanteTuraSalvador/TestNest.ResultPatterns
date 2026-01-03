using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;

namespace TestNest.ResultPattern.Test;

public class ResultTests
{
    #region Success Tests

    [Fact]
    public void Success_ReturnsSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithSingleError_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error message");

        // Act
        var result = Result.Failure(ErrorType.Validation, error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Single(result.Errors);
        Assert.Equal("ERR001", result.Errors[0].Code);
        Assert.Equal("Test error message", result.Errors[0].Message);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ReturnsFailureResult()
    {
        // Arrange
        var errors = new[]
        {
            new Error("ERR001", "First error"),
            new Error("ERR002", "Second error")
        };

        // Act
        var result = Result.Failure(ErrorType.Validation, errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Failure_WithStringOverload_ReturnsFailureResult()
    {
        // Act
        var result = Result.Failure(ErrorType.NotFound, "ERR404", "Resource not found");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Single(result.Errors);
        Assert.Equal("ERR404", result.Errors[0].Code);
    }

    [Fact]
    public void Failure_WithErrorTypeNone_ThrowsException()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");

        // Act & Assert
        Assert.Throws<ResultException>(() => Result.Failure(ErrorType.None, error));
    }

    #endregion

    #region Combine Tests

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_WithOneFailure_ReturnsFailure()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Failure(ErrorType.Validation, "ERR001", "Error occurred");
        var result3 = Result.Success();

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        Assert.False(combined.IsSuccess);
        Assert.Equal(ErrorType.Aggregate, combined.ErrorType);
    }

    [Fact]
    public void Combine_WithMultipleFailures_AggregatesAllErrors()
    {
        // Arrange
        var result1 = Result.Failure(ErrorType.Validation, "ERR001", "First error");
        var result2 = Result.Failure(ErrorType.Validation, "ERR002", "Second error");

        // Act
        var combined = Result.Combine(result1, result2);

        // Assert
        Assert.False(combined.IsSuccess);
        Assert.Equal(2, combined.Errors.Count);
    }

    #endregion

    #region ToResult<T> Tests

    [Fact]
    public void ToResult_FromSuccess_ReturnsSuccessWithValue()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var typedResult = result.ToResult(42);

        // Assert
        Assert.True(typedResult.IsSuccess);
        Assert.Equal(42, typedResult.Value);
    }

    [Fact]
    public void ToResult_FromFailure_ReturnsFailureWithErrors()
    {
        // Arrange
        var result = Result.Failure(ErrorType.Validation, "ERR001", "Error message");

        // Act
        var typedResult = result.ToResult(42);

        // Assert
        Assert.False(typedResult.IsSuccess);
        Assert.Equal(ErrorType.Validation, typedResult.ErrorType);
        Assert.Single(typedResult.Errors);
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_OnSuccess_ExecutesBindFunction()
    {
        // Arrange
        var result = Result.Success();
        var bindExecuted = false;

        // Act
        var boundResult = result.Bind(() =>
        {
            bindExecuted = true;
            return Result.Success();
        });

        // Assert
        Assert.True(bindExecuted);
        Assert.True(boundResult.IsSuccess);
    }

    [Fact]
    public void Bind_OnFailure_SkipsBindFunction()
    {
        // Arrange
        var result = Result.Failure(ErrorType.Validation, "ERR001", "Error");
        var bindExecuted = false;

        // Act
        var boundResult = result.Bind(() =>
        {
            bindExecuted = true;
            return Result.Success();
        });

        // Assert
        Assert.False(bindExecuted);
        Assert.False(boundResult.IsSuccess);
    }

    [Fact]
    public void Bind_ToTypedResult_OnSuccess_ReturnsTypedResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var typedResult = result.Bind(() => Result<int>.Success(42));

        // Assert
        Assert.True(typedResult.IsSuccess);
        Assert.Equal(42, typedResult.Value);
    }

    [Fact]
    public void Bind_ToTypedResult_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result.Failure(ErrorType.Validation, "ERR001", "Error");

        // Act
        var typedResult = result.Bind(() => Result<int>.Success(42));

        // Assert
        Assert.False(typedResult.IsSuccess);
        Assert.Equal(ErrorType.Validation, typedResult.ErrorType);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnSuccess_ExecutesMapAction()
    {
        // Arrange
        var result = Result.Success();
        var mapExecuted = false;

        // Act
        var mappedResult = result.Map(() => { mapExecuted = true; });

        // Assert
        Assert.True(mapExecuted);
        Assert.True(mappedResult.IsSuccess);
    }

    [Fact]
    public void Map_OnFailure_SkipsMapAction()
    {
        // Arrange
        var result = Result.Failure(ErrorType.Validation, "ERR001", "Error");
        var mapExecuted = false;

        // Act
        var mappedResult = result.Map(() => { mapExecuted = true; });

        // Assert
        Assert.False(mapExecuted);
        Assert.False(mappedResult.IsSuccess);
    }

    [Fact]
    public void Map_ToTypedResult_OnSuccess_ReturnsTypedResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var typedResult = result.Map(() => 42);

        // Assert
        Assert.True(typedResult.IsSuccess);
        Assert.Equal(42, typedResult.Value);
    }

    #endregion

    #region Async Tests

    [Fact]
    public async Task BindAsync_OnSuccess_ExecutesAsyncFunction()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var boundResult = await result.BindAsync(async () =>
        {
            await Task.Delay(1);
            return Result.Success();
        });

        // Assert
        Assert.True(boundResult.IsSuccess);
    }

    [Fact]
    public async Task BindAsync_OnFailure_SkipsAsyncFunction()
    {
        // Arrange
        var result = Result.Failure(ErrorType.Validation, "ERR001", "Error");
        var executed = false;

        // Act
        var boundResult = await result.BindAsync(async () =>
        {
            executed = true;
            await Task.Delay(1);
            return Result.Success();
        });

        // Assert
        Assert.False(executed);
        Assert.False(boundResult.IsSuccess);
    }

    [Fact]
    public async Task MapAsync_OnSuccess_ExecutesAsyncFunction()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var mappedResult = await result.MapAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(42, mappedResult.Value);
    }

    #endregion

    #region Deconstruct Tests

    [Fact]
    public void Deconstruct_Success_ReturnsCorrectValues()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var (isSuccess, errorType, errors) = result;

        // Assert
        Assert.True(isSuccess);
        Assert.Equal(ErrorType.None, errorType);
        Assert.Empty(errors);
    }

    [Fact]
    public void Deconstruct_Failure_ReturnsCorrectValues()
    {
        // Arrange
        var result = Result.Failure(ErrorType.Validation, "ERR001", "Error message");

        // Act
        var (isSuccess, errorType, errors) = result;

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(ErrorType.Validation, errorType);
        Assert.Single(errors);
    }

    #endregion
}
