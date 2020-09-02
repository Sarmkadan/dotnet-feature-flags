// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Utilities;

/// <summary>
/// Helper class for pagination calculations and metadata generation.
/// Provides utilities for offset/limit calculations and page information.
/// </summary>
public static class PaginationHelper
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 1000;
    public const int MinPageSize = 1;

    /// <summary>
    /// Validates and normalizes page number and size parameters.
    /// Ensures values are within acceptable ranges to prevent abuse.
    /// </summary>
    public static (int pageNumber, int pageSize) ValidateAndNormalizePaging(int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);

        return (pageNumber, pageSize);
    }

    /// <summary>
    /// Calculates offset from page number and size for database skip operations.
    /// </summary>
    public static int CalculateOffset(int pageNumber, int pageSize)
    {
        var (validPageNumber, validPageSize) = ValidateAndNormalizePaging(pageNumber, pageSize);
        return (validPageNumber - 1) * validPageSize;
    }

    /// <summary>
    /// Calculates pagination metadata for API responses.
    /// </summary>
    public static PaginationMetadata CreateMetadata(int pageNumber, int pageSize, int totalCount)
    {
        var (validPageNumber, validPageSize) = ValidateAndNormalizePaging(pageNumber, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalCount / validPageSize);

        return new PaginationMetadata
        {
            PageNumber = validPageNumber,
            PageSize = validPageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = validPageNumber < totalPages,
            HasPreviousPage = validPageNumber > 1
        };
    }

    /// <summary>
    /// Paginates an enumerable collection in-memory (use for small datasets).
    /// For large datasets, use database-level pagination instead.
    /// </summary>
    public static IEnumerable<T> PaginateInMemory<T>(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var (validPageNumber, validPageSize) = ValidateAndNormalizePaging(pageNumber, pageSize);
        return source.Skip((validPageNumber - 1) * validPageSize).Take(validPageSize);
    }

    /// <summary>
    /// Gets the range of item numbers for the current page (e.g., "1-20 of 150").
    /// </summary>
    public static string GetItemRange(int pageNumber, int pageSize, int totalCount)
    {
        var (validPageNumber, validPageSize) = ValidateAndNormalizePaging(pageNumber, pageSize);
        var startItem = Math.Max(1, (validPageNumber - 1) * validPageSize + 1);
        var endItem = Math.Min(validPageNumber * validPageSize, totalCount);

        if (startItem > totalCount)
        {
            return $"0 of {totalCount}";
        }

        return $"{startItem}-{endItem} of {totalCount}";
    }
}

/// <summary>
/// Pagination metadata to include in API responses.
/// </summary>
public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public string ItemRange => PaginationHelper.GetItemRange(PageNumber, PageSize, TotalCount);
}

/// <summary>
/// Generic paginated response wrapper for consistent API responses.
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
}
