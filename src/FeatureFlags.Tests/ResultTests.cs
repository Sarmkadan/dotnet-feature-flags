using System;
using System.Threading.Tasks;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Success_WithData_CreatesSuccessfulResultWithData()
        {
            // Arrange
            var testData = "test value";

            // Act
            var result = Result<string>.Success(testData);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(testData, result.Data);
            Assert.Null(result.Error);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public void Success_WithNullData_CreatesSuccessfulResultWithNullData()
        {
            // Act
            var result = Result<string>.Success(null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Null(result.Error);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public void Failure_WithError_CreatesFailedResultWithError()
        {
            // Arrange
            var errorMessage = "Something went wrong";

            // Act
            var result = Result<string>.Failure(errorMessage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.Error);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public void Failure_WithErrorAndErrorCode_CreatesFailedResultWithBoth()
        {
            // Arrange
            var errorMessage = "Database error";
            var errorCode = 500;

            // Act
            var result = Result<string>.Failure(errorMessage, errorCode);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.Error);
            Assert.Equal(errorCode, result.ErrorCode);
        }

        [Fact]
        public void FromException_WithException_CreatesFailedResultWithExceptionMessage()
        {
            // Arrange
            var exception = new InvalidOperationException("Operation failed");

            // Act
            var result = Result<int>.FromException(exception);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(0, result.Data);
            Assert.Equal(exception.Message, result.Error);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task Try_WithSuccessfulOperation_ReturnsSuccessfulResult()
        {
            // Arrange
            var expectedValue = 42;
            Task<int> Operation() => Task.FromResult(expectedValue);

            // Act
            var result = await Result<int>.Try(Operation);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, result.Data);
            Assert.Null(result.Error);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task Try_WithFailingOperation_ReturnsFailedResult()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Operation failed");
            Task<int> Operation() => throw expectedException;

            // Act
            var result = await Result<int>.Try(Operation);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(0, result.Data);
            Assert.Equal(expectedException.Message, result.Error);
            Assert.Null(result.ErrorCode);
        }

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
        public void Map_WithTransformThrowingException_ReturnsFailedResult()
        {
            // Arrange
            var result = Result<int>.Success(10);
            var transform = new Func<int, string>(x => throw new InvalidOperationException("Transform failed"));

            // Act
            var mappedResult = result.Map(transform);

            // Assert
            Assert.False(mappedResult.IsSuccess);
            Assert.Null(mappedResult.Data);
            Assert.Equal("Transform failed", mappedResult.Error);
            Assert.Null(mappedResult.ErrorCode);
        }

        [Fact]
        public async Task BindAsync_WithSuccessfulResult_ChainsOperations()
        {
            // Arrange
            var originalData = 5;
            var result = Result<int>.Success(originalData);
            Task<Result<string>> Operation(int x) => Task.FromResult(Result<string>.Success((x * 3).ToString()));

            // Act
            var boundResult = await result.BindAsync(Operation);

            // Assert
            Assert.True(boundResult.IsSuccess);
            Assert.Equal("15", boundResult.Data);
            Assert.Null(boundResult.Error);
            Assert.Null(boundResult.ErrorCode);
        }

        [Fact]
        public async Task BindAsync_WithFailedResult_ReturnsFailedResultWithSameError()
        {
            // Arrange
            var errorMessage = "Original error";
            var result = Result<int>.Failure(errorMessage);

            // Act
            var boundResult = await result.BindAsync(async x => Result<string>.Success("ignored"));

            // Assert
            Assert.False(boundResult.IsSuccess);
            Assert.Null(boundResult.Data);
            Assert.Equal(errorMessage, boundResult.Error);
            Assert.Null(boundResult.ErrorCode);
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
            Assert.Same(result, returnedResult); // Should return same instance
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
            Assert.Same(result, returnedResult); // Should return same instance
        }

        [Fact]
        public void OnFailure_WithFailedResult_ExecutesAction()
        {
            // Arrange
            var result = Result<int>.Failure("error message");
            var executedAction = false;
            var capturedError = string.Empty;
            void Action(string error)
            {
                executedAction = true;
                capturedError = error;
            }

            // Act
            var returnedResult = result.OnFailure(Action);

            // Assert
            Assert.True(executedAction);
            Assert.Equal("error message", capturedError);
            Assert.Same(result, returnedResult); // Should return same instance
        }

        [Fact]
        public void OnFailure_WithSuccessfulResult_DoesNotExecuteAction()
        {
            // Arrange
            var result = Result<int>.Success(42);
            var executed = false;
            void Action(string error) => executed = true;

            // Act
            var returnedResult = result.OnFailure(Action);

            // Assert
            Assert.False(executed);
            Assert.Same(result, returnedResult); // Should return same instance
        }

        [Fact]
        public void GetOrThrow_WithSuccessfulResult_ReturnsData()
        {
            // Arrange
            var expectedData = "test data";
            var result = Result<string>.Success(expectedData);

            // Act
            var actualData = result.GetOrThrow();

            // Assert
            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public async Task GetOrThrow_WithFailedResult_ThrowsInvalidOperationException()
        {
            // Arrange
            var result = Result<string>.Failure("error message");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Task.FromResult(result.GetOrThrow()));
            Assert.Equal("error message", exception.Message);
        }

        [Fact]
        public void GetOrDefault_WithSuccessfulResult_ReturnsData()
        {
            // Arrange
            var result = Result<int>.Success(42);

            // Act
            var actual = result.GetOrDefault(0);

            // Assert
            Assert.Equal(42, actual);
        }

        [Fact]
        public void GetOrDefault_WithFailedResult_ReturnsDefaultValue()
        {
            // Arrange
            var result = Result<int>.Failure("error");

            // Act
            var actual = result.GetOrDefault(100);

            // Assert
            Assert.Equal(100, actual);
        }

        [Fact]
        public void GetOrDefault_WithNullData_ReturnsNull()
        {
            // Arrange
            var result = Result<string>.Success(null);

            // Act
            var actual = result.GetOrDefault("default");

            // Assert
            Assert.Null(actual);
        }
    }
}