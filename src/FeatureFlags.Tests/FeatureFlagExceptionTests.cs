using System;
using FeatureFlags.Exceptions;
using Xunit;

namespace FeatureFlags.Tests
{
    public class FeatureFlagExceptionTests
    {
        [Fact]
        public void FeatureFlagException_MessageConstructor_SetsMessageAndErrorCodeNull()
        {
            // Arrange
            var message = "Test message";

            // Act
            var ex = new FeatureFlagException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.ErrorCode);
        }

        [Fact]
        public void FeatureFlagException_MessageAndErrorCodeConstructor_SetsBothProperties()
        {
            // Arrange
            var message = "Test message";
            var errorCode = "TEST_CODE";

            // Act
            var ex = new FeatureFlagException(message, errorCode);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal(errorCode, ex.ErrorCode);
        }

        [Fact]
        public void FeatureFlagNotFoundException_CreatesCorrectMessageAndErrorCode()
        {
            // Arrange
            var key = "nonexistent_flag";

            // Act
            var ex = new FeatureFlagNotFoundException(key);

            // Assert
            Assert.Equal($"Feature flag '{key}' not found.", ex.Message);
            Assert.Equal("FF_NOT_FOUND", ex.ErrorCode);
        }

        [Fact]
        public void InvalidFeatureFlagException_CreatesCorrectMessageAndErrorCode()
        {
            // Arrange
            var message = "Invalid configuration";

            // Act
            var ex = new InvalidFeatureFlagException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("FF_INVALID_CONFIG", ex.ErrorCode);
        }

        [Fact]
        public void RuleEvaluationException_WithInnerException_SetsInnerAndErrorCode()
        {
            // Arrange
            var message = "Rule evaluation failed";
            var inner = new InvalidOperationException("inner");

            // Act
            var ex = new RuleEvaluationException(message, inner);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("RULE_EVAL_ERROR", ex.ErrorCode);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void FeatureFlagDataException_WithInnerException_SetsInnerAndErrorCode()
        {
            // Arrange
            var message = "Database error";
            var inner = new Exception("inner");

            // Act
            var ex = new FeatureFlagDataException(message, inner);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("DATA_ERROR", ex.ErrorCode);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void FeatureFlagException_ErrorCodeCanBeModifiedAfterConstruction()
        {
            // Arrange
            var ex = new FeatureFlagException("Initial message");

            // Act
            ex.ErrorCode = "MODIFIED_CODE";

            // Assert
            Assert.Equal("MODIFIED_CODE", ex.ErrorCode);
        }

        [Fact]
        public void DerivedExceptions_AreAssignableToFeatureFlagException()
        {
            // Arrange
            var notFound = new FeatureFlagNotFoundException("key");
            var invalid = new InvalidFeatureFlagException("msg");
            var rule = new RuleEvaluationException("msg");
            var data = new FeatureFlagDataException("msg");

            // Act & Assert
            Assert.IsAssignableFrom<FeatureFlagException>(notFound);
            Assert.IsAssignableFrom<FeatureFlagException>(invalid);
            Assert.IsAssignableFrom<FeatureFlagException>(rule);
            Assert.IsAssignableFrom<FeatureFlagException>(data);
        }
    }
}
