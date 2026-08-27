using System;
using System.Collections.Generic;
using FeatureFlags.Integration;
using Xunit;

namespace FeatureFlags.Tests
{
    /// <summary>
    /// Test class for validating Webhook objects.
    /// </summary>
    public class WebhookValidationTests
    {
        private static Webhook CreateValidWebhook()
        {
            var now = DateTime.UtcNow;
            return new Webhook
            {
                Url = "https://example.com/webhook",
                Description = "Test webhook",
                CreatedBy = "tester",
                MaxRetries = 3,
                RetryDelaySeconds = 5,
                CreatedAt = now,
                UpdatedAt = now,
                LastTriggeredAt = null,
                FeatureFlagKey = "feature-123",
                AuthorizationHeader = "Bearer token",
                Secret = "super-secret"
            };
        }

        /// <summary>
        /// Tests that a valid webhook returns an empty list of problems when Validate() is called.
        /// </summary>
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var webhook = CreateValidWebhook();

            // Act
            IReadOnlyList<string> problems = webhook.Validate();

            // Assert
            Assert.Empty(problems);
        }

        /// <summary>
        /// Tests that a valid webhook returns true when IsValid() is called.
        /// </summary>
        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var webhook = CreateValidWebhook();

            // Act
            bool isValid = webhook.IsValid();

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// Tests that a valid webhook does not throw when EnsureValid() is called.
        /// </summary>
        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var webhook = CreateValidWebhook();

            // Act & Assert
            var exception = Record.Exception(() => webhook.EnsureValid());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that calling Validate() on a null webhook throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Validate_Null_ThrowsArgumentNullException()
        {
            // Arrange
            Webhook? webhook = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => webhook!.Validate());
        }

        /// <summary>
        /// Tests that calling IsValid() on a null webhook throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void IsValid_Null_ThrowsArgumentNullException()
        {
            // Arrange
            Webhook? webhook = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => webhook!.IsValid());
        }

        /// <summary>
        /// Tests that calling EnsureValid() on an invalid webhook (with empty URL) throws an ArgumentException.
        /// </summary>
        [Fact]
        public void EnsureValid_Invalid_ThrowsArgumentException()
        {
            // Arrange
            var webhook = CreateValidWebhook();
            webhook.Url = ""; // invalid URL

            // Act
            var ex = Assert.Throws<ArgumentException>(() => webhook.EnsureValid());

            // Assert
            Assert.Contains("Url cannot be null or whitespace.", ex.Message);
        }

        /// <summary>
        /// Tests that calling IsValid() on an invalid webhook (with negative MaxRetries) returns false.
        /// </summary>
        [Fact]
        public void IsValid_Invalid_ReturnsFalse()
        {
            // Arrange
            var webhook = CreateValidWebhook();
            webhook.MaxRetries = -1; // invalid value

            // Act
            bool isValid = webhook.IsValid();

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// Tests that Validate() returns a problem when UpdatedAt is earlier than CreatedAt.
        /// </summary>
        [Fact]
        public void Validate_UpdatedAtEarlierThanCreatedAt_ReturnsProblem()
        {
            // Arrange
            var webhook = CreateValidWebhook();
            webhook.UpdatedAt = webhook.CreatedAt.AddMinutes(-1); // earlier than CreatedAt

            // Act
            var problems = webhook.Validate();

            // Assert
            Assert.Contains("UpdatedAt cannot be earlier than CreatedAt.", problems);
        }

        /// <summary>
        /// Tests that Validate() returns a problem when MaxRetries is negative.
        /// </summary>
        [Fact]
        public void Validate_NegativeMaxRetries_ReturnsProblem()
        {
            // Arrange
            var webhook = CreateValidWebhook();
            webhook.MaxRetries = -5;

            // Act
            var problems = webhook.Validate();

            // Assert
            Assert.Contains("MaxRetries cannot be negative.", problems);
        }
    }
}
