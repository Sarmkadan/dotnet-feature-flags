// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Generic API response wrapper for consistent API response format.
/// Used across all endpoints to provide uniform response structure to clients.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public ApiMetadata? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed response with error message.
    /// </summary>
    public static ApiResponse<T> Error(string error, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = data,
            Error = error,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a response from a Result<T>.
    /// </summary>
    public static ApiResponse<T> FromResult(Result<T> result, string? defaultError = null)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return Error(result.Error ?? defaultError ?? "Operation failed");
    }
}

/// <summary>
/// Non-generic response for operations that don't return data.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public ApiMetadata? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message ?? "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    public static ApiResponse Error(string error)
    {
        return new ApiResponse
        {
            Success = false,
            Error = error,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Metadata included in API responses for additional context.
/// </summary>
public class ApiMetadata
{
    public string? RequestId { get; set; }
    public long? ExecutionTimeMs { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public int? TotalCount { get; set; }
    public Dictionary<string, object>? CustomData { get; set; }
}

/// <summary>
/// Paginated API response wrapper for list endpoints.
/// </summary>
public class PaginatedApiResponse<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful paginated response.
    /// </summary>
    public static PaginatedApiResponse<T> Ok(List<T> data, int pageNumber, int pageSize, int totalCount)
    {
        return new PaginatedApiResponse<T>
        {
            Success = true,
            Data = data,
            Pagination = new PaginationInfo
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            },
            Message = "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed paginated response.
    /// </summary>
    public static PaginatedApiResponse<T> Error(string error)
    {
        return new PaginatedApiResponse<T>
        {
            Success = false,
            Message = error,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Pagination information included in paginated responses.
/// </summary>
public class PaginationInfo
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
