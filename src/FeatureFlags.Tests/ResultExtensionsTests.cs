using System;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void Map_WithSuccessfulResult_TransformsData()
    {
        // Arrange
        var originalData = 10;
        var result = Result<int>.Success(originalData);
        var transform = new Func<int, string>(x => (x * 2).ToString());

        // Act
        var mappedResult = result.Map(transform);

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("20", mappedResult.Data);
        Assert.Null(mappedResult.Error);
        Assert.Null(mappedResult.ErrorCode);
    }

    [Fact]
    public void Map_WithFailedResult_ReturnsFailedResultWithSameError()
    {
        // Arrange
        var errorMessage = "Original error";
        var result = Result<int>.Failure(errorMessage);

        // Act
        var mappedResult = result.Map(x => x.ToString());

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Null(mappedResult.Data);
        Assert.Equal(errorMessage, mappedResult.Error);
        Assert.Null(mappedResult.ErrorCode);
    }

    [Fact]
    public void Map_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        Result<int> result = null;
        Func<int, string> transform = x => x.ToString();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.Map(result, transform));
    }

    [Fact]
    public void Map_WithNullTransform_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Success(10);
        Func<int, string> transform = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.Map(result, transform));
    }

    [Fact]
    public void OnSuccess_WithSuccessfulResult_ExecutesAction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var executed = false;
        void Action(int data) => executed = true;

        // Act
        var returnedResult = result.OnSuccess(Action);

        // Assert
        Assert.True(executed);
        Assert.Same(result, returnedResult);
    }

    [Fact]
    public void OnSuccess_WithFailedResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<int>.Failure("error");
        var executed = false;
        void Action(int data) => executed = true;

        // Act
        var returnedResult = result.OnSuccess(Action);

        // Assert
        Assert.False(executed);
        Assert.Same(result, returnedResult);
    }

    [Fact]
    public void OnSuccess_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        Result<int> result = null;
        Action<int> action = _ => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.OnSuccess(result, action));
    }

    [Fact]
    public void OnSuccess_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Success(42);
        Action<int> action = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.OnSuccess(result, action));
    }

    [Fact]
    public void OnFailure_WithFailedResult_ExecutesAction()
    {
        // Arrange
        var result = Result<int>.Failure("error message", 404);
        var executedAction = false;
        var capturedError = string.Empty;
        int? capturedErrorCode = null;
        void Action(string error, int? errorCode)
        {
            executedAction = true;
            capturedError = error ?? string.Empty;
            capturedErrorCode = errorCode;
        }

        // Act
        var returnedResult = result.OnFailure(Action);

        // Assert
        Assert.True(executedAction);
        Assert.Equal("error message", capturedError);
        Assert.Equal(404, capturedErrorCode);
        Assert.Same(result, returnedResult);
    }

    [Fact]
    public void OnFailure_WithSuccessfulResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var executed = false;
        void Action(string error, int? errorCode) => executed = true;

        // Act
        var returnedResult = result.OnFailure(Action);

        // Assert
        Assert.False(executed);
        Assert.Same(result, returnedResult);
    }

    [Fact]
    public void OnFailure_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        Result<int> result = null;
        Action<string?, int?> action = (_, __) => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.OnFailure(result, action));
    }

    [Fact]
    public void OnFailure_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Failure("error");
        Action<string?, int?> action = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.OnFailure(result, action));
    }

    [Fact]
    public void OnFailure_NonGeneric_WithFailedResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure("error message", 500);
        var executedAction = false;
        var capturedError = string.Empty;
        int? capturedErrorCode = null;
        void Action(string error, int? errorCode)
        {
            executedAction = true;
            capturedError = error ?? string.Empty;
            capturedErrorCode = errorCode;
        }

        // Act
        var returnedResult = result.OnFailure(Action);

        // Assert
        Assert.True(executedAction);
        Assert.Equal("error message", capturedError);
        Assert.Equal(500, capturedErrorCode);
        Assert.Same(result, returnedResult);
    }

    [Fact]
    public void OnFailure_NonGeneric_WithSuccessfulResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;
        void Action(string error, int? errorCode) => executed = true;

        // Act
        var returnedResult = result.OnFailure(Action);

        // Assert
        Assert.False(executed);
        Assert.Same(result, returnedResult);
    }

    [Fact]
    public void OnFailure_NonGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        Result result = null;
        Action<string?, int?> action = (_, __) => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.OnFailure(result, action));
    }

    [Fact]
    public void OnFailure_NonGeneric_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result.Failure("error");
        Action<string?, int?> action = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.OnFailure(result, action));
    }
}