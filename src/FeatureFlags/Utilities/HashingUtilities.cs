#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.Utilities;

/// <summary>
/// Utility class for various hashing operations used in feature flag evaluation and security.
/// Provides consistent hashing algorithms for stable rollout decisions and password hashing.
/// </summary>
public static class HashingUtilities
{
    /// <summary>
    /// Computes SHA-256 hash of input string and returns as hex-encoded lowercase string.
    /// Used for consistent hashing in rollout bucketing and data integrity verification.
    /// </summary>
    public static string ComputeSha256(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Computes SHA-512 hash for additional security in sensitive operations.
    /// </summary>
    public static string ComputeSha512(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var sha512 = SHA512.Create();
        var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Converts SHA-256 hash to a numeric value (0-99) for percentage rollout bucketing.
    /// Ensures same input always produces same output for consistent user experiences.
    /// </summary>
    public static int ComputeHashBucket(string input, int bucketSize = 100)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        if (bucketSize <= 0)
        {
            throw new ArgumentException("Bucket size must be greater than 0", nameof(bucketSize));
        }

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        // Take first 4 bytes and convert to unsigned int
        var hashValue = BitConverter.ToUInt32(hashBytes, 0);
        return (int)(hashValue % bucketSize);
    }

    /// <summary>
    /// Computes MD5 hash (useful for ETags and quick checksums, not for security).
    /// Warning: MD5 is cryptographically weak, use SHA-256 for security-sensitive operations.
    /// </summary>
    public static string ComputeMd5(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Computes 32-bit FNV-1a hash for faster but less collision-resistant hashing.
    /// Suitable for hash tables and quick lookups, not cryptographic use.
    /// </summary>
    public static uint ComputeFnv1aHash(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        const uint fnvPrime = 16777619;
        const uint offsetBasis = 2166136261;

        uint hash = offsetBasis;

        foreach (var byteValue in Encoding.UTF8.GetBytes(input))
        {
            hash ^= byteValue;
            hash *= fnvPrime;
        }

        return hash;
    }

    /// <summary>
    /// Hashes password using PBKDF2 with SHA-256, suitable for user credentials.
    /// Uses random salt to prevent rainbow table attacks.
    /// </summary>
    public static string HashPassword(string password, int iterations = 10000, int saltSize = 16, int hashSize = 20)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[saltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(hashSize);

        var hashBytes = new byte[saltSize + hashSize];
        Array.Copy(salt, 0, hashBytes, 0, saltSize);
        Array.Copy(hash, 0, hashBytes, saltSize, hashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against a PBKDF2 hash created by HashPassword.
    /// </summary>
    public static bool VerifyPassword(string password, string hash, int iterations = 10000, int saltSize = 16, int hashSize = 20)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            var hashBytes = Convert.FromBase64String(hash);

            if (hashBytes.Length != saltSize + hashSize)
            {
                return false;
            }

            var salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(hashSize);

            // Constant-time comparison to prevent timing attacks
            for (int i = 0; i < hashSize; i++)
            {
                if (hashBytes[saltSize + i] != computedHash[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a secure random hash suitable for tokens or session IDs.
    /// </summary>
    public static string GenerateSecureHash(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Computes an HMAC-SHA256 signature for the given payload using the provided secret.
    /// </summary>
    public static string ComputeHmacSha256(string payload, string secret)
    {
        if (string.IsNullOrEmpty(payload))
            throw new ArgumentException("Payload cannot be null or empty", nameof(payload));
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
