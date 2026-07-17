namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for <see cref="ApiResponse"/> and <see cref="ApiResponse{T}"/>.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Determines if an <see cref="ApiResponse"/> or <see cref="ApiResponse{T}"/> was successful.
    /// </summary>
    /// <param name="response">The response to check.</param>
    /// <returns>true if the response was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static bool IsSuccess(this ApiResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Success;
    }

    /// <summary>
    /// Determines if an <see cref="ApiResponse{T}"/> was successful and contains data.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="response">The response to check.</param>
    /// <returns>true if the response was successful and contains data; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static bool HasData<T>(this ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Success && response.Data is not null;
    }

    /// <summary>
    /// Gets a human-readable representation of an <see cref="ApiResponse"/> error.
    /// </summary>
    /// <param name="response">The response to get the error from.</param>
    /// <returns>A human-readable representation of the error; or null if there is no error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static string? GetErrorMessage(this ApiResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Error ?? response.Message;
    }

    /// <summary>
    /// Gets a human-readable representation of an <see cref="ApiResponse{T}"/> error.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="response">The response to get the error from.</param>
    /// <returns>A human-readable representation of the error; or null if there is no error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static string? GetErrorMessage<T>(this ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Error ?? response.Message;
    }
}