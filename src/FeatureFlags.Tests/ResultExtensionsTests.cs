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
        var result = Result<int>.Success(10);

        // Act
        var mappedResult = result.Map(x => (x * 2).ToString());

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
        var result = Result<int>.Failure("error");

        // Act
        var mappedResult = result.Map(x => x.ToString());

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Null(mappedResult.Data);
        Assert.Equal("error", mappedResult.Error);
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

        // Act
        var returnedResult = result.OnSuccess(data => executed = true);

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

        // Act
        var returnedResult = result.OnSuccess(data => executed = true);

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
        var executed = false;
        string? capturedError = null;
        int? capturedErrorCode = null;

        // Act
        var returnedResult = result.OnFailure((error, errorCode) =>
        {
            executed = true;
            capturedError = error;
            capturedErrorCode = errorCode;
        });

        // Assert
        Assert.True(executed);
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

        // Act
        var returnedResult = result.OnFailure((error, errorCode) => executed = true);

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
        var executed = false;
        string? capturedError = null;
        int? capturedErrorCode = null;

        // Act
        var returnedResult = result.OnFailure((error, errorCode) =>
        {
            executed = true;
            capturedError = error;
            capturedErrorCode = errorCode;
        });

        // Assert
        Assert.True(executed);
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

        // Act
        var returnedResult = result.OnFailure((error, errorCode) => executed = true);

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