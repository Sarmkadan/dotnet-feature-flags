using System;
using System.Collections.Generic;
using FeatureFlags.Integration;
using Xunit;

namespace FeatureFlags.Tests
{
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

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var webhook = CreateValidWebhook();

            // Act & Assert
            var exception = Record.Exception(() => webhook.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_Null_ThrowsArgumentNullException()
        {
            // Arrange
            Webhook? webhook = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => webhook!.Validate());
        }

        [Fact]
        public void IsValid_Null_ThrowsArgumentNullException()
        {
            // Arrange
            Webhook? webhook = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => webhook!.IsValid());
        }

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
