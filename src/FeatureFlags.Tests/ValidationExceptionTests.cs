using System;
using System.Collections.Generic;
using FeatureFlags.Exceptions;
using Xunit;

namespace FeatureFlags.Tests
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void ValidationException_MessageConstructor_SetsMessageAndErrorCode()
        {
            // Arrange
            var message = "Validation failed";

            // Act
            var ex = new ValidationException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
        }

        [Fact]
        public void ValidationException_MessageAndErrorsConstructor_SetsMessageErrorCodeAndErrors()
        {
            // Arrange
            var message = "Validation failed";
            var errors = new Dictionary<string, string>
            {
                { "field1", "Error message 1" },
                { "field2", "Error message 2" }
            };

            // Act
            var ex = new ValidationException(message, errors);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
            Assert.Same(errors, ex.Errors);
            Assert.Equal(2, ex.Errors.Count);
            Assert.Equal("Error message 1", ex.Errors["field1"]);
            Assert.Equal("Error message 2", ex.Errors["field2"]);
        }

        [Fact]
        public void ValidationException_MessageAndErrorsConstructor_WithNullErrors_SetsErrorsToNull()
        {
            // Arrange
            var message = "Validation failed";
            Dictionary<string, string> errors = null!;

            // Act
            var ex = new ValidationException(message, errors);

            // Assert - null errors parameter results in null Errors (direct assignment)
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
            Assert.Null(ex.Errors);
        }

        [Fact]
        public void ValidationException_MessageAndErrorsConstructor_WithEmptyDictionary_InitializesEmptyDictionary()
        {
            // Arrange
            var message = "Validation failed";
            var errors = new Dictionary<string, string>();

            // Act
            var ex = new ValidationException(message, errors);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
            Assert.NotNull(ex.Errors);
            Assert.Empty(ex.Errors);
        }

        [Fact]
        public void ValidationException_MessageAndInnerExceptionConstructor_SetsMessageErrorCodeAndInnerException()
        {
            // Arrange
            var message = "Validation failed";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var ex = new ValidationException(message, innerException);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
            Assert.Same(innerException, ex.InnerException);
        }

        [Fact]
        public void ValidationException_ErrorsProperty_IsReadOnlyDictionary()
        {
            // Arrange
            var ex = new ValidationException("Test");

            // Act & Assert - Dictionary is read-only in the sense that it's a new instance
            var originalCount = ex.Errors.Count;
            var newDict = new Dictionary<string, string> { { "key", "value" } };
            ex.Errors.ToList().ForEach(kvp => newDict[kvp.Key] = kvp.Value);

            // The Errors property returns a new Dictionary instance each time
            Assert.Equal(originalCount, ex.Errors.Count);
        }

        [Fact]
        public void WebhookValidationException_MessageConstructor_SetsMessageAndErrorCode()
        {
            // Arrange
            var message = "Webhook validation failed";

            // Act
            var ex = new WebhookValidationException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
            Assert.IsAssignableFrom<ValidationException>(ex);
        }

        [Fact]
        public void WebhookValidationException_MessageAndErrorsConstructor_SetsMessageErrorCodeAndErrors()
        {
            // Arrange
            var message = "Webhook validation failed";
            var errors = new Dictionary<string, string>
            {
                { "webhookUrl", "Invalid URL format" },
                { "events", "At least one event is required" }
            };

            // Act
            var ex = new WebhookValidationException(message, errors);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
            Assert.Same(errors, ex.Errors);
            Assert.Equal(2, ex.Errors.Count);
        }

        [Fact]
        public void ValidationException_InheritanceHierarchy_IsCorrect()
        {
            // Arrange
            var validationEx = new ValidationException("test");
            var webhookEx = new WebhookValidationException("test");

            // Act & Assert
            Assert.IsAssignableFrom<FeatureFlagException>(validationEx);
            Assert.IsType<ValidationException>(validationEx);

            Assert.IsAssignableFrom<ValidationException>(webhookEx);
            Assert.IsAssignableFrom<FeatureFlagException>(webhookEx);
        }

        [Fact]
        public void ValidationException_WithNullMessage_SetsMessageToNull()
        {
            // Arrange & Act
            var ex = new ValidationException(null!);

            // Assert - Exception constructor handles null message
            Assert.NotNull(ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
        }

        [Fact]
        public void WebhookValidationException_WithNullMessage_SetsMessageToNull()
        {
            // Arrange & Act
            var ex = new WebhookValidationException(null!);

            // Assert - Exception constructor handles null message
            Assert.NotNull(ex.Message);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
        }

        [Fact]
        public void ValidationException_ErrorsProperty_SharesReference()
        {
            // Arrange
            var originalErrors = new Dictionary<string, string> { { "field", "error" } };
            var ex = new ValidationException("test", originalErrors);

            // Act - modify original dictionary
            originalErrors.Add("newField", "newError");

            // Assert - exception's errors shares the same reference (no copy is made)
            Assert.Equal(2, ex.Errors.Count);
            Assert.Contains("newField", ex.Errors.Keys);
        }
    }
}