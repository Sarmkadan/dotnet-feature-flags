using System;
using System.Text;
using FeatureFlags.Utilities;
using Xunit;

namespace FeatureFlags.Tests;

public class HashingUtilitiesTests
{
    // Test data
    private const string TestInput = "hello world";
    private const string EmptyInput = "";
    private const string NullInput = null!;
    private const string WhitespaceInput = "   ";
    private const string SingleCharInput = "a";
    private const string LongInput = "This is a very long string that should test the hashing algorithms with substantial input data to ensure they work correctly with larger strings";

    [Fact]
    public void ComputeSha256_WithValidInput_ReturnsCorrectHash()
    {
        // Act
        var result = HashingUtilities.ComputeSha256(TestInput);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.All(result, c => Assert.Contains(c, "0123456789abcdef"));
        Assert.Equal("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", result);
    }

    [Fact]
    public void ComputeSha256_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeSha256(NullInput));
    }

    [Fact]
    public void ComputeSha256_WithEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeSha256(EmptyInput));
    }

    [Fact]
    public void ComputeSha256_WithWhitespaceInput_ReturnsCorrectHash()
    {
        // Act
        var result = HashingUtilities.ComputeSha256(WhitespaceInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("e1822db470e60d090affd0959d3b03e2f6a4b967fa34ea1570f4f65b78e6cf53", result);
    }

    [Fact]
    public void ComputeSha512_WithValidInput_ReturnsCorrectHash()
    {
        // Act
        var result = HashingUtilities.ComputeSha512(TestInput);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.All(result, c => Assert.Contains(c, "0123456789abcdef"));
        Assert.Equal("9b71d224bd62f3785d96d46ad3ea3d73319bfbc2890caadaed2d8e88ca055532e60a8a90965408e8d90d3247ab11050a031f0f8e8f0b9b6ec63e800b84ecef8", result);
    }

    [Fact]
    public void ComputeSha512_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeSha512(NullInput));
    }

    [Fact]
    public void ComputeSha512_WithEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeSha512(EmptyInput));
    }

    [Fact]
    public void ComputeHashBucket_WithValidInput_ReturnsValueInRange()
    {
        // Act
        var result = HashingUtilities.ComputeHashBucket(TestInput, 100);

        // Assert
        Assert.InRange(result, 0, 99);
    }

    [Fact]
    public void ComputeHashBucket_WithSameInput_ReturnsSameResult()
    {
        // Act
        var result1 = HashingUtilities.ComputeHashBucket(TestInput, 50);
        var result2 = HashingUtilities.ComputeHashBucket(TestInput, 50);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ComputeHashBucket_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHashBucket(NullInput, 100));
    }

    [Fact]
    public void ComputeHashBucket_WithEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHashBucket(EmptyInput, 100));
    }

    [Fact]
    public void ComputeHashBucket_WithInvalidBucketSize_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHashBucket(TestInput, 0));
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHashBucket(TestInput, -1));
    }

    [Fact]
    public void ComputeHashBucket_WithDefaultBucketSize_UsesDefaultValue()
    {
        // Act
        var result = HashingUtilities.ComputeHashBucket(TestInput);

        // Assert
        Assert.InRange(result, 0, 99); // DefaultBucketSize is 100
    }

    [Fact]
    public void ComputeMd5_WithValidInput_ReturnsCorrectHash()
    {
        // Act
        var result = HashingUtilities.ComputeMd5(TestInput);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.All(result, c => Assert.Contains(c, "0123456789abcdef"));
        Assert.Equal("5eb63bbbe01eeed093cb22bb8f5acdc3", result);
    }

    [Fact]
    public void ComputeMd5_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeMd5(NullInput));
    }

    [Fact]
    public void ComputeMd5_WithEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeMd5(EmptyInput));
    }

    [Fact]
    public void ComputeFnv1aHash_WithValidInput_ReturnsCorrectHash()
    {
        // Act
        var result = HashingUtilities.ComputeFnv1aHash(TestInput);

        // Assert
        Assert.Equal(1640531527u, result);
    }

    [Fact]
    public void ComputeFnv1aHash_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeFnv1aHash(NullInput));
    }

    [Fact]
    public void ComputeFnv1aHash_WithEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeFnv1aHash(EmptyInput));
    }

    [Fact]
    public void ComputeFnv1aHash_WithSameInput_ReturnsSameResult()
    {
        // Act
        var result1 = HashingUtilities.ComputeFnv1aHash(TestInput);
        var result2 = HashingUtilities.ComputeFnv1aHash(TestInput);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void HashPassword_WithValidInput_ReturnsHashedPassword()
    {
        // Act
        var result = HashingUtilities.HashPassword("password123");

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        // Should be base64 encoded string (salt + hash)
        Assert.Contains("=", result); // Base64 strings often end with =
    }

    [Fact]
    public void HashPassword_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.HashPassword(NullInput));
    }

    [Fact]
    public void HashPassword_WithEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.HashPassword(EmptyInput));
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "securePassword456!";
        var hash = HashingUtilities.HashPassword(password);

        // Act
        var result = HashingUtilities.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "securePassword456!";
        var wrongPassword = "wrongPassword";
        var hash = HashingUtilities.HashPassword(password);

        // Act
        var result = HashingUtilities.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var password = "securePassword456!";
        var hash = HashingUtilities.HashPassword(password);

        // Act
        var result = HashingUtilities.VerifyPassword(NullInput, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ReturnsFalse()
    {
        // Arrange
        var password = "securePassword456!";

        // Act
        var result = HashingUtilities.VerifyPassword(password, NullInput);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ReturnsFalse()
    {
        // Arrange
        var password = "securePassword456!";

        // Act
        var result = HashingUtilities.VerifyPassword(password, EmptyInput);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateSecureHash_WithDefaultLength_ReturnsBase64String()
    {
        // Act
        var result = HashingUtilities.GenerateSecureHash();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        // Default length is 32 bytes, which base64 encodes to 44 characters
        Assert.Equal(44, result.Length);
    }

    [Fact]
    public void GenerateSecureHash_WithSpecifiedLength_ReturnsCorrectLength()
    {
        // Act
        var result = HashingUtilities.GenerateSecureHash(16);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        // 16 bytes base64 encoded is 24 characters
        Assert.Equal(24, result.Length);
    }

    [Fact]
    public void GenerateSecureHash_WithZeroLength_ReturnsEmptyString()
    {
        // Act
        var result = HashingUtilities.GenerateSecureHash(0);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComputeHmacSha256_WithValidInputs_ReturnsCorrectHash()
    {
        // Act
        var result = HashingUtilities.ComputeHmacSha256(TestInput, "secret");

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.All(result, c => Assert.Contains(c, "0123456789abcdef"));
        Assert.Equal("8030e7cb3fd002f2d046c577a9823d4ef1d3470ebisba6a418000f8f2422dc87", result.ToLower());
    }

    [Fact]
    public void ComputeHmacSha256_WithNullPayload_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHmacSha256(NullInput, "secret"));
    }

    [Fact]
    public void ComputeHmacSha256_WithEmptyPayload_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHmacSha256(EmptyInput, "secret"));
    }

    [Fact]
    public void ComputeHmacSha256_WithNullSecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHmacSha256(TestInput, NullInput));
    }

    [Fact]
    public void ComputeHmacSha256_WithEmptySecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HashingUtilities.ComputeHmacSha256(TestInput, EmptyInput));
    }

    [Fact]
    public void ComputeHmacSha256_WithSameInputs_ReturnsSameResult()
    {
        // Act
        var result1 = HashingUtilities.ComputeHmacSha256(TestInput, "secret");
        var result2 = HashingUtilities.ComputeHmacSha256(TestInput, "secret");

        // Assert
        Assert.Equal(result1, result2);
    }
}