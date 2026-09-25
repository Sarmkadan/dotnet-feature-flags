#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.Exceptions;
using Xunit;

namespace FeatureFlags.Tests.Exceptions
{
    public class ConfigurationExceptionValidationTests
    {
        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            ConfigurationException value = null!;
            Assert.Throws<ArgumentNullException>(() => value.Validate());
        }

        [Fact]
        public void Validate_ValidMessage_NoInnerException_ReturnsEmptyList()
        {
            var ex = new ConfigurationException("Valid message");
            var result = ex.Validate();
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_NullMessage_ThrowsInConstructor()
        {
            // Cannot create ConfigurationException with null message - constructor throws
            Assert.Throws<ArgumentNullException>(() => new ConfigurationException(null!));
        }

        [Fact]
        public void Validate_EmptyMessage_ReturnsErrorAboutMessage()
        {
            var ex = new ConfigurationException(string.Empty);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_WhitespaceMessage_ReturnsErrorAboutMessage()
        {
            var ex = new ConfigurationException("   ");
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_ValidMessageWithValidInnerException_ReturnsEmptyList()
        {
            var inner = new ConfigurationException("Valid inner");
            var ex = new ConfigurationException("Valid outer", inner);
            var result = ex.Validate();
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_ValidMessageWithInvalidInnerException_ReturnsInnerExceptionError()
        {
            var inner = new ConfigurationException(string.Empty);
            var ex = new ConfigurationException("Valid outer", inner);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Inner exception: Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_InvalidMessageWithValidInnerException_ReturnsOuterExceptionError()
        {
            var inner = new ConfigurationException("Valid inner");
            var ex = new ConfigurationException(string.Empty, inner);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_InvalidMessageWithInvalidInnerException_ReturnsBothErrors()
        {
            var inner = new ConfigurationException(string.Empty);
            var ex = new ConfigurationException(string.Empty, inner);
            var result = ex.Validate();
            Assert.Equal(2, result.Count);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
            Assert.Contains("Inner exception: Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_ValidMessageWithNonConfigurationExceptionInner_ReturnsEmptyList()
        {
            var inner = new ArgumentException("Inner exception");
            var ex = new ConfigurationException("Valid outer", inner);
            var result = ex.Validate();
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_InvalidMessageWithNonConfigurationExceptionInner_ReturnsOuterExceptionError()
        {
            var inner = new ArgumentException("Inner exception");
            var ex = new ConfigurationException(string.Empty, inner);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_ValidMessageWithConfigurationExceptionInnerThatHasNonConfigurationExceptionInner_ReturnsEmptyList()
        {
            var innerInner = new ArgumentException("Deep inner");
            var inner = new ConfigurationException("Valid middle", innerInner);
            var ex = new ConfigurationException("Valid outer", inner);
            var result = ex.Validate();
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_ValidMessageWithConfigurationExceptionInnerThatHasInvalidMessageAndNonConfigurationExceptionInner_ReturnsInnerExceptionError()
        {
            var innerInner = new ArgumentException("Deep inner");
            var inner = new ConfigurationException(string.Empty, innerInner);
            var ex = new ConfigurationException("Valid outer", inner);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Inner exception: Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void IsValid_NullInput_ReturnsTrue()
        {
            ConfigurationException value = null!;
            Assert.True(value.IsValid());
        }

        [Fact]
        public void IsValid_ValidMessage_ReturnsTrue()
        {
            var ex = new ConfigurationException("Valid");
            Assert.True(ex.IsValid());
        }

        [Fact]
        public void IsValid_InvalidMessage_ReturnsFalse()
        {
            var ex = new ConfigurationException(string.Empty);
            Assert.False(ex.IsValid());
        }

        [Fact]
        public void IsValid_WhitespaceMessage_ReturnsFalse()
        {
            var ex = new ConfigurationException("   ");
            Assert.False(ex.IsValid());
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            ConfigurationException value = null!;
            Assert.Throws<ArgumentNullException>(() => value.EnsureValid());
        }

        [Fact]
        public void EnsureValid_ValidMessage_DoesNotThrow()
        {
            var ex = new ConfigurationException("Valid");
            ex.EnsureValid(); // Should not throw
        }

        [Fact]
        public void EnsureValid_InvalidMessage_ThrowsArgumentException()
        {
            var ex = new ConfigurationException(string.Empty);
            var exAssert = Assert.Throws<ArgumentException>(() => ex.EnsureValid());
            Assert.Contains("ConfigurationException validation failed", exAssert.Message);
            Assert.Contains("Message must not be null, empty, or whitespace.", exAssert.Message);
        }

        [Fact]
        public void EnsureValid_WhitespaceMessage_ThrowsArgumentException()
        {
            var ex = new ConfigurationException("   ");
            var exAssert = Assert.Throws<ArgumentException>(() => ex.EnsureValid());
            Assert.Contains("ConfigurationException validation failed", exAssert.Message);
            Assert.Contains("Message must not be null, empty, or whitespace.", exAssert.Message);
        }

        [Fact]
        public void Validate_DatabaseConfigurationException_BehavesSameAsBase()
        {
            var ex = new DatabaseConfigurationException(string.Empty);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_HttpClientConfigurationException_BehavesSameAsBase()
        {
            var ex = new HttpClientConfigurationException(string.Empty);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_InnerException_DatabaseConfigurationException_Recurses()
        {
            var inner = new DatabaseConfigurationException(string.Empty);
            var ex = new ConfigurationException("Outer", inner);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Inner exception: Message must not be null, empty, or whitespace.", result);
        }

        [Fact]
        public void Validate_InnerException_HttpClientConfigurationException_Recurses()
        {
            var inner = new HttpClientConfigurationException(string.Empty);
            var ex = new ConfigurationException("Outer", inner);
            var result = ex.Validate();
            Assert.Single(result);
            Assert.Contains("Inner exception: Message must not be null, empty, or whitespace.", result);
        }
    }
}