using System;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests
{
    public class ABTestVariantTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesWithDefaultValues()
        {
            // Arrange & Act
            var variant = new ABTestVariant();

            // Assert
            Assert.Equal(0, variant.Id);
            Assert.Equal(0, variant.FeatureFlagId);
            Assert.Empty(variant.VariantKey);
            Assert.Empty(variant.DisplayName);
            Assert.Empty(variant.Description);
            Assert.Equal(0, variant.AllocationPercentage);
            Assert.Equal(0, variant.UserCount);
            Assert.Equal(0, variant.ConversionCount);
            Assert.False(variant.IsControl);
            Assert.True(variant.CreatedAt <= DateTime.UtcNow);
            Assert.True(variant.UpdatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Id_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.Id = 42;

            // Assert
            Assert.Equal(42, variant.Id);
        }

        [Fact]
        public void FeatureFlagId_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.FeatureFlagId = 99;

            // Assert
            Assert.Equal(99, variant.FeatureFlagId);
        }

        [Fact]
        public void VariantKey_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.VariantKey = "new_variant";

            // Assert
            Assert.Equal("new_variant", variant.VariantKey);
        }

        [Fact]
        public void DisplayName_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.DisplayName = "New Display Name";

            // Assert
            Assert.Equal("New Display Name", variant.DisplayName);
        }

        [Fact]
        public void Description_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.Description = "Detailed description here";

            // Assert
            Assert.Equal("Detailed description here", variant.Description);
        }

        [Fact]
        public void AllocationPercentage_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.AllocationPercentage = 75;

            // Assert
            Assert.Equal(75, variant.AllocationPercentage);
        }

        [Fact]
        public void UserCount_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.UserCount = 5000;

            // Assert
            Assert.Equal(5000, variant.UserCount);
        }

        [Fact]
        public void ConversionCount_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.ConversionCount = 250;

            // Assert
            Assert.Equal(250, variant.ConversionCount);
        }

        [Fact]
        public void CreatedAt_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();
            var testDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            // Act
            variant.CreatedAt = testDate;

            // Assert
            Assert.Equal(testDate, variant.CreatedAt);
        }

        [Fact]
        public void UpdatedAt_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();
            var testDate = new DateTime(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc);

            // Act
            variant.UpdatedAt = testDate;

            // Assert
            Assert.Equal(testDate, variant.UpdatedAt);
        }

        [Fact]
        public void IsControl_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            var variant = new ABTestVariant();

            // Act
            variant.IsControl = true;

            // Assert
            Assert.True(variant.IsControl);
        }

        [Fact]
        public void GetConversionRate_WithZeroUserCount_ReturnsZero()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 0, ConversionCount = 100 };

            // Act
            var conversionRate = variant.GetConversionRate();

            // Assert
            Assert.Equal(0, conversionRate);
        }

        [Fact]
        public void GetConversionRate_WithEqualUserAndConversionCount_ReturnsOne()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 100, ConversionCount = 100 };

            // Act
            var conversionRate = variant.GetConversionRate();

            // Assert
            Assert.Equal(1.0, conversionRate);
        }

        [Fact]
        public void GetConversionRate_WithPartialConversion_ReturnsCorrectRate()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 400, ConversionCount = 80 };

            // Act
            var conversionRate = variant.GetConversionRate();

            // Assert
            Assert.Equal(0.2, conversionRate);
        }

        [Fact]
        public void RecordUserAssignment_IncrementsUserCount()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 100 };

            // Act
            variant.RecordUserAssignment();

            // Assert
            Assert.Equal(101, variant.UserCount);
        }

        [Fact]
        public void RecordConversion_IncrementsConversionCount()
        {
            // Arrange
            var variant = new ABTestVariant { ConversionCount = 50 };

            // Act
            variant.RecordConversion();

            // Assert
            Assert.Equal(51, variant.ConversionCount);
        }

        [Fact]
        public void IsValid_WithValidVariant_ReturnsTrue()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "valid_key", DisplayName = "Valid Display Name", AllocationPercentage = 50, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_WithEmptyVariantKey_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "", DisplayName = "Valid Display Name", AllocationPercentage = 50, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_WithWhitespaceVariantKey_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "   ", DisplayName = "Valid Display Name", AllocationPercentage = 50, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_WithEmptyDisplayName_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "valid_key", DisplayName = "", AllocationPercentage = 50, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_WithWhitespaceDisplayName_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "valid_key", DisplayName = "   ", AllocationPercentage = 50, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_WithNegativeAllocationPercentage_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "valid_key", DisplayName = "Valid Display Name", AllocationPercentage = -1, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_WithOverHundredAllocationPercentage_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "valid_key", DisplayName = "Valid Display Name", AllocationPercentage = 101, FeatureFlagId = 1 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_WithZeroFeatureFlagId_ReturnsFalse()
        {
            // Arrange
            var variant = new ABTestVariant { VariantKey = "valid_key", DisplayName = "Valid Display Name", AllocationPercentage = 50, FeatureFlagId = 0 };

            // Act
            var isValid = variant.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void GetStatisticalConfidence_WithVeryLowUserCount_ReturnsVeryLow()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 50 };

            // Act
            var confidence = variant.GetStatisticalConfidence();

            // Assert
            Assert.Equal("Very Low", confidence);
        }

        [Fact]
        public void GetStatisticalConfidence_WithLowUserCount_ReturnsLow()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 250 };

            // Act
            var confidence = variant.GetStatisticalConfidence();

            // Assert
            Assert.Equal("Low", confidence);
        }

        [Fact]
        public void GetStatisticalConfidence_WithMediumUserCount_ReturnsMedium()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 750 };

            // Act
            var confidence = variant.GetStatisticalConfidence();

            // Assert
            Assert.Equal("Medium", confidence);
        }

        [Fact]
        public void GetStatisticalConfidence_WithHighUserCount_ReturnsHigh()
        {
            // Arrange
            var variant = new ABTestVariant { UserCount = 2500 };

            // Act
            var confidence = variant.GetStatisticalConfidence();

            // Assert
            Assert.Equal("High", confidence);
        }

        [Fact]
        public void FeatureFlagNavigationProperty_IsInitiallyNull()
        {
            // Arrange & Act
            var variant = new ABTestVariant();

            // Assert
            Assert.Null(variant.FeatureFlag);
        }

        [Fact]
        public void FeatureFlagNavigationProperty_CanBeSet()
        {
            // Arrange
            var variant = new ABTestVariant();
            var featureFlag = new FeatureFlag();

            // Act
            variant.FeatureFlag = featureFlag;

            // Assert
            Assert.Same(featureFlag, variant.FeatureFlag);
        }
    }
}