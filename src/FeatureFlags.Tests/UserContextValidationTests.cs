using System;
using System.Collections.Generic;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests
{
    public class UserContextValidationTests
    {
        private static UserContext CreateValidUserContext()
        {
            return new UserContext
            {
                UserId = "user-123",
                Email = "test@example.com",
                Country = "US",
                Tier = "premium",
                Region = "us-east",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CustomAttributes = new Dictionary<string, string> { { "key", "value" } }
            };
        }

        [Fact]
        public void Validate_ValidContext_ReturnsEmptyList()
        {
            var context = CreateValidUserContext();
            var errors = UserContextValidation.Validate(context);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_InvalidFields_ReturnsErrors()
        {
            var context = new UserContext
            {
                UserId = "", // Invalid
                Email = "invalid-email", // Invalid
                Country = "USA", // Invalid
                Tier = "invalid tier!" // Invalid
            };
            var errors = UserContextValidation.Validate(context);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("UserId"));
            Assert.Contains(errors, e => e.Contains("Email"));
            Assert.Contains(errors, e => e.Contains("Country"));
            Assert.Contains(errors, e => e.Contains("Tier"));
        }

        [Fact]
        public void IsValid_ValidContext_ReturnsTrue()
        {
            var context = CreateValidUserContext();
            Assert.True(UserContextValidation.IsValid(context));
        }

        [Fact]
        public void IsValid_InvalidContext_ReturnsFalse()
        {
            var context = new UserContext { UserId = "" };
            Assert.False(UserContextValidation.IsValid(context));
        }

        [Fact]
        public void EnsureValid_ValidContext_DoesNotThrow()
        {
            var context = CreateValidUserContext();
            var exception = Record.Exception(() => UserContextValidation.EnsureValid(context));
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_InvalidContext_ThrowsArgumentException()
        {
            var context = new UserContext { UserId = "" };
            Assert.Throws<ArgumentException>(() => UserContextValidation.EnsureValid(context));
        }

        [Fact]
        public void Validate_NullContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => UserContextValidation.Validate(null!));
        }
    }
}
