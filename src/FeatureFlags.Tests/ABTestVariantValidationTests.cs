using System;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests
{
    public class ABTestVariantValidationTests
    {
        private ABTestVariant CreateValidVariant()
        {
            return new ABTestVariant
            {
                Id = 1,
                FeatureFlagId = 1,
                VariantKey = "test-variant",
                DisplayName = "Test Variant",
                Description = "A test variant for unit testing",
                AllocationPercentage = 50,
                UserCount = 100,
                ConversionCount = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public void Validate_ValidVariant_ReturnsEmptyList()
        {
            // Arrange
            var variant = CreateValidVariant();

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_NullVariant_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ABTestVariantValidation.Validate(null));
        }

        [Fact]
        public void Validate_NegativeId_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.Id = -1;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("Id must be a non-negative integer.", result);
        }

        [Fact]
        public void Validate_ZeroId_IsValid()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.Id = 0; // Zero is allowed (non-negative)

            // Act
            var result = variant.Validate();

            // Assert
            Assert.DoesNotContain("Id must be a non-negative integer.", result);
        }

        [Fact]
        public void Validate_NonPositiveFeatureFlagId_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.FeatureFlagId = 0;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("FeatureFlagId must be a positive integer.", result);
        }

        [Fact]
        public void Validate_NegativeFeatureFlagId_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.FeatureFlagId = -5;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("FeatureFlagId must be a positive integer.", result);
        }

        [Fact]
        public void Validate_EmptyVariantKey_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.VariantKey = string.Empty;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("VariantKey is required and cannot be empty or whitespace.", result);
        }

        [Fact]
        public void Validate_WhitespaceVariantKey_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.VariantKey = "   ";

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("VariantKey is required and cannot be empty or whitespace.", result);
        }

        [Fact]
        public void Validate_LongVariantKey_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.VariantKey = new string('a', 101); // 101 characters

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("VariantKey must be 100 characters or less.", result);
        }

        [Fact]
        public void Validate_ValidVariantKey_IsValid()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.VariantKey = new string('a', 100); // Exactly 100 characters

            // Act
            var result = variant.Validate();

            // Assert
            Assert.DoesNotContain("VariantKey must be 100 characters or less.", result);
        }

        [Fact]
        public void Validate_EmptyDisplayName_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.DisplayName = string.Empty;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("DisplayName is required and cannot be empty or whitespace.", result);
        }

        [Fact]
        public void Validate_LongDisplayName_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.DisplayName = new string('a', 201); // 201 characters

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("DisplayName must be 200 characters or less.", result);
        }

        [Fact]
        public void Validate_EmptyDescription_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.Description = string.Empty;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("Description is required and cannot be empty or whitespace.", result);
        }

        [Fact]
        public void Validate_LongDescription_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.Description = new string('a', 1001); // 1001 characters

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("Description must be 1000 characters or less.", result);
        }

        [Fact]
        public void Validate_InvalidAllocationPercentage_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.AllocationPercentage = -5; // Below minimum

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("AllocationPercentage must be between 0 and 100 inclusive.", result);
        }

        [Fact]
        public void Validate_AllocationPercentageOver100_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.AllocationPercentage = 105; // Above maximum

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("AllocationPercentage must be between 0 and 100 inclusive.", result);
        }

        [Fact]
        public void Validate_ValidAllocationPercentage_IsValid()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.AllocationPercentage = 0; // Minimum valid

            // Act
            var result = variant.Validate();

            // Assert
            Assert.DoesNotContain("AllocationPercentage must be between 0 and 100 inclusive.", result);
        }

        [Fact]
        public void Validate_AllocationPercentageAt100_IsValid()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.AllocationPercentage = 100; // Maximum valid

            // Act
            var result = variant.Validate();

            // Assert
            Assert.DoesNotContain("AllocationPercentage must be between 0 and 100 inclusive.", result);
        }

        [Fact]
        public void Validate_NegativeUserCount_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.UserCount = -1;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("UserCount cannot be negative.", result);
        }

        [Fact]
        public void Validate_NegativeConversionCount_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.ConversionCount = -1;

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("ConversionCount cannot be negative.", result);
        }

        [Fact]
        public void Validate_ConversionCountExceedsUserCount_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.UserCount = 50;
            variant.ConversionCount = 60; // More conversions than users

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("ConversionCount cannot exceed UserCount.", result);
        }

        [Fact]
        public void Validate_DefaultCreatedAt_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.CreatedAt = default; // DateTime.MinValue

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("CreatedAt must be set to a valid DateTime.", result);
        }

        [Fact]
        public void Validate_NonUtcCreatedAt_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.CreatedAt = DateTime.Now; // Local time, not UTC

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("CreatedAt must be in UTC.", result);
        }

        [Fact]
        public void Validate_DefaultUpdatedAt_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.UpdatedAt = default; // DateTime.MinValue

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("UpdatedAt must be set to a valid DateTime.", result);
        }

        [Fact]
        public void Validate_NonUtcUpdatedAt_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.UpdatedAt = DateTime.Now; // Local time, not UTC

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("UpdatedAt must be in UTC.", result);
        }

        [Fact]
        public void Validate_UpdatedAtBeforeCreatedAt_ReturnsError()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.CreatedAt = DateTime.UtcNow;
            variant.UpdatedAt = DateTime.UtcNow.AddHours(-1); // One hour before CreatedAt

            // Act
            var result = variant.Validate();

            // Assert
            Assert.Contains("UpdatedAt must be equal to or after CreatedAt.", result);
        }

        [Fact]
        public void IsValid_ValidVariant_ReturnsTrue()
        {
            // Arrange
            var variant = CreateValidVariant();

            // Act
            var result = variant.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullVariant_ReturnsFalse()
        {
            // Act
            var result = ABTestVariantValidation.IsValid(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_InvalidVariant_ReturnsFalse()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.VariantKey = string.Empty; // Make it invalid

            // Act
            var result = variant.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_ValidVariant_DoesNotThrow()
        {
            // Arrange
            var variant = CreateValidVariant();

            // Act
            variant.EnsureValid();

            // Assert - no exception thrown
        }

        [Fact]
        public void EnsureValid_NullVariant_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ABTestVariantValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_InvalidVariant_ThrowsArgumentExceptionWithMessage()
        {
            // Arrange
            var variant = CreateValidVariant();
            variant.VariantKey = string.Empty; // Make it invalid

            // Act
            var exception = Assert.Throws<ArgumentException>(() => variant.EnsureValid());

            // Assert
            Assert.Contains("ABTestVariant validation failed", exception.Message);
            Assert.Contains("VariantKey is required and cannot be empty or whitespace.", exception.Message);
        }
    }
}