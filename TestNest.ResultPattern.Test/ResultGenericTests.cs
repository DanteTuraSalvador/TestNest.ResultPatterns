using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;

namespace TestNest.ResultPattern.Test;

public class ResultGenericTests
{
    #region Success Tests

    [Fact]
    public void Success_WithValue_ReturnsSuccessResult()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_WithStringValue_ReturnsSuccessResult()
    {
        // Act
        var result = Result<string>.Success("Hello World");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello World", result.Value);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ResultException>(() => Result<string>.Success(null!));
    }

    [Fact]
    public void Success_WithComplexType_ReturnsSuccessResult()
    {
        // Arrange
        var testObject = new TestClass { Id = 1, Name = "Test" };

        // Act
        var result = Result<TestClass>.Success(testObject);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("Test", result.Value.Name);
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithSingleError_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error message");

        // Act
        var result = Result<int>.Failure(ErrorType.Validation, error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Single(result.Errors);
        Assert.Equal(default(int), result.Value);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ReturnsFailureResult()
    {
        // Arrange
        var errors = new[]
        {
            new Error("ERR001", "First error"),
            new Error("ERR002", "Second error"),
            new Error("ERR003", "Third error")
        };

        // Act
        var result = Result<int>.Failure(ErrorType.Validation, errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void Failure_WithStringOverload_ReturnsFailureResult()
    {
        // Act
        var result = Result<int>.Failure(ErrorType.NotFound, "ERR404", "Resource not found");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal("ERR404", result.Errors[0].Code);
    }

    [Fact]
    public void Failure_WithErrorTypeNone_ThrowsException()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");

        // Act & Assert
        Assert.Throws<ResultException>(() => Result<int>.Failure(ErrorType.None, error));
    }

    [Fact]
    public void Failure_WithEmptyErrorList_ThrowsException()
    {
        // Arrange
        var errors = Array.Empty<Error>();

        // Act & Assert
        Assert.Throws<ResultException>(() => Result<int>.Failure(ErrorType.Validation, errors));
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_OnSuccess_ExecutesBindFunction()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var boundResult = result.Bind(x => Result<int>.Success(x * 2));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal(20, boundResult.Value);
    }

    [Fact]
    public void Bind_OnFailure_SkipsBindFunction()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error");
        var bindExecuted = false;

        // Act
        var boundResult = result.Bind(x =>
        {
            bindExecuted = true;
            return Result<int>.Success(x * 2);
        });

        // Assert
        Assert.False(bindExecuted);
        Assert.False(boundResult.IsSuccess);
    }

    [Fact]
    public void Bind_ChainedOperations_PropagatesFirstError()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var finalResult = result
            .Bind(x => Result<int>.Success(x + 5))
            .Bind(x => Result<int>.Failure(ErrorType.Validation, "ERR001", "Mid-chain failure"))
            .Bind(x => Result<int>.Success(x * 2));

        // Assert
        Assert.False(finalResult.IsSuccess);
        Assert.Equal("ERR001", finalResult.Errors[0].Code);
    }

    [Fact]
    public void Bind_TransformToNewType_ReturnsNewType()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var stringResult = result.Bind(x => Result<string>.Success($"Value is {x}"));

        // Assert
        Assert.True(stringResult.IsSuccess);
        Assert.Equal("Value is 42", stringResult.Value);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(20, mappedResult.Value);
    }

    [Fact]
    public void Map_OnFailure_SkipsMapFunction()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error");
        var mapExecuted = false;

        // Act
        var mappedResult = result.Map(x =>
        {
            mapExecuted = true;
            return x * 2;
        });

        // Assert
        Assert.False(mapExecuted);
        Assert.False(mappedResult.IsSuccess);
    }

    [Fact]
    public void Map_TransformToNewType_ReturnsNewType()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var stringResult = result.Map(x => $"Number: {x}");

        // Assert
        Assert.True(stringResult.IsSuccess);
        Assert.Equal("Number: 42", stringResult.Value);
    }

    [Fact]
    public void Map_ChainedTransformations_TransformsCorrectly()
    {
        // Arrange
        var result = Result<int>.Success(5);

        // Act
        var finalResult = result
            .Map(x => x * 2)
            .Map(x => x + 3)
            .Map(x => x.ToString());

        // Assert
        Assert.True(finalResult.IsSuccess);
        Assert.Equal("13", finalResult.Value);
    }

    #endregion

    #region Async Tests

    [Fact]
    public async Task BindAsync_OnSuccess_ExecutesAsyncFunction()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var boundResult = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int>.Success(x * 2);
        });

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal(20, boundResult.Value);
    }

    [Fact]
    public async Task BindAsync_OnFailure_SkipsAsyncFunction()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error");
        var executed = false;

        // Act
        var boundResult = await result.BindAsync(async x =>
        {
            executed = true;
            await Task.Delay(1);
            return Result<int>.Success(x * 2);
        });

        // Assert
        Assert.False(executed);
        Assert.False(boundResult.IsSuccess);
    }

    [Fact]
    public async Task MapAsync_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var mappedResult = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(20, mappedResult.Value);
    }

    [Fact]
    public async Task MapAsync_OnFailure_SkipsAsyncFunction()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error");
        var executed = false;

        // Act
        var mappedResult = await result.MapAsync(async x =>
        {
            executed = true;
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        Assert.False(executed);
        Assert.False(mappedResult.IsSuccess);
    }

    #endregion

    #region EnsureSuccess Tests

    [Fact]
    public void EnsureSuccess_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.EnsureSuccess();

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void EnsureSuccess_OnFailure_ThrowsException()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error message");

        // Act & Assert
        Assert.Throws<ResultException>(() => result.EnsureSuccess());
    }

    #endregion

    #region TryGetValue Tests

    [Fact]
    public void TryGetValue_OnSuccess_ReturnsTrueAndValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var success = result.TryGetValue(out var value, out var errors);

        // Assert
        Assert.True(success);
        Assert.Equal(42, value);
        Assert.Empty(errors);
    }

    [Fact]
    public void TryGetValue_OnFailure_ReturnsFalseAndErrors()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error message");

        // Act
        var success = result.TryGetValue(out var value, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(default(int), value);
        Assert.Single(errors);
    }

    #endregion

    #region Deconstruct Tests

    [Fact]
    public void Deconstruct_Success_ReturnsCorrectValues()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var (isSuccess, value, errorType, errors) = result;

        // Assert
        Assert.True(isSuccess);
        Assert.Equal(42, value);
        Assert.Equal(ErrorType.None, errorType);
        Assert.Empty(errors);
    }

    [Fact]
    public void Deconstruct_Failure_ReturnsCorrectValues()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error message");

        // Act
        var (isSuccess, value, errorType, errors) = result;

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(default(int), value);
        Assert.Equal(ErrorType.Validation, errorType);
        Assert.Single(errors);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesSuccessResult()
    {
        // Act
        Result<string> result = "Hello";

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello", result.Value);
    }

    #endregion

    #region ToResult Tests

    [Fact]
    public void ToResult_FromSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var nonGenericResult = result.ToResult();

        // Assert
        Assert.True(nonGenericResult.IsSuccess);
        Assert.Equal(ErrorType.None, nonGenericResult.ErrorType);
    }

    [Fact]
    public void ToResult_FromFailure_ReturnsFailureResult()
    {
        // Arrange
        var result = Result<int>.Failure(ErrorType.Validation, "ERR001", "Error message");

        // Act
        var nonGenericResult = result.ToResult();

        // Assert
        Assert.False(nonGenericResult.IsSuccess);
        Assert.Equal(ErrorType.Validation, nonGenericResult.ErrorType);
        Assert.Single(nonGenericResult.Errors);
    }

    #endregion

    #region ErrorType Coverage Tests

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Internal)]
    [InlineData(ErrorType.Aggregate)]
    [InlineData(ErrorType.Invalid)]
    public void Failure_WithAllErrorTypes_WorksCorrectly(ErrorType errorType)
    {
        // Act
        var result = Result<int>.Failure(errorType, "ERR001", "Error message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorType, result.ErrorType);
    }

    #endregion

    #region Helper Classes

    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
