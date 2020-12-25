#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Extension methods for AuditController providing additional convenience methods
/// for querying and analyzing audit logs.
/// </summary>
public static class AuditControllerExtensions
{
    /// <summary>
    /// Gets recent audit activity across all feature flags within a specified time window.
    /// </summary>
    /// <param name="controller">The AuditController instance</param>
    /// <param name="days">Number of days to look back (default: 7)</param>
    /// <param name="maxResults">Maximum number of results to return (default: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with recent audit activity</returns>
    public static async Task<IActionResult> GetRecentActivity(
        this AuditController controller,
        int days = 7,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var logsResult = await controller.GetAuditLogsByDateRange(
                startDate: cutoffDate,
                endDate: DateTime.UtcNow,
                page: 1,
                pageSize: maxResults,
                cancellationToken: cancellationToken
            );

            if (logsResult is OkObjectResult okResult && okResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse)
            {
                return controller.Ok(ApiResponse<object>.Ok(
                    new { RecentActivity = paginatedResponse.Data,
                          TotalCount = paginatedResponse.Pagination?.TotalCount ?? 0 },
                    "Recent activity retrieved successfully"
                ));
            }

            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve recent activity"));
        }
        catch (Exception)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve recent activity"));
        }
    }

    /// <summary>
    /// Gets audit logs filtered by specific action type.
    /// </summary>
    /// <param name="controller">The AuditController instance</param>
    /// <param name="action">The audit action type to filter by</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with filtered audit logs</returns>
    public static async Task<IActionResult> GetAuditLogsByAction(
        this AuditController controller,
        Enums.AuditAction action,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (validPage, validPageSize) = PaginationHelper.ValidateAndNormalizePaging(page, pageSize);

            // Get all logs in a reasonable date range and filter by action type
            var cutoffDate = DateTime.UtcNow.AddDays(-90); // Last 90 days
            var allLogsResult = await controller.GetAuditLogsByDateRange(
                startDate: cutoffDate,
                endDate: DateTime.UtcNow,
                page: 1,
                pageSize: 10000, // Large enough to get all recent logs
                cancellationToken: cancellationToken
            );

            if (allLogsResult is OkObjectResult okResult && okResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse)
            {
                var filteredLogs = paginatedResponse.Data
                    .Where(log => log.Action == action)
                    .ToList();

                var metadata = PaginationHelper.CreateMetadata(validPage, validPageSize, filteredLogs.Count);

                return controller.Ok(new PaginatedApiResponse<AuditLog>
                {
                    Success = true,
                    Data = filteredLogs,
                    Pagination = new Models.PaginationInfo
                    {
                        PageNumber = validPage,
                        PageSize = validPageSize,
                        TotalCount = filteredLogs.Count,
                        TotalPages = metadata.TotalPages
                    }
                });
            }

            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve audit logs by action"));
        }
        catch (Exception)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve audit logs by action"));
        }
    }

    /// <summary>
    /// Gets audit logs with enhanced filtering capabilities including multiple criteria.
    /// </summary>
    /// <param name="controller">The AuditController instance</param>
    /// <param name="startDate">Start date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <param name="action">Optional action type to filter by</param>
    /// <param name="username">Optional username to filter by</param>
    /// <param name="flagId">Optional feature flag ID to filter by</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with filtered audit logs</returns>
    public static async Task<IActionResult> GetFilteredAuditLogs(
        this AuditController controller,
        DateTime startDate,
        DateTime endDate,
        Enums.AuditAction? action = null,
        string? username = null,
        int? flagId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate > endDate)
            {
                return controller.BadRequest(ApiResponse<object>.Fail("Start date must be before end date"));
            }

            var (validPage, validPageSize) = PaginationHelper.ValidateAndNormalizePaging(page, pageSize);

            // Get all logs in date range first
            var logsResult = await controller.GetAuditLogsByDateRange(
                startDate: startDate,
                endDate: endDate,
                page: 1,
                pageSize: 10000,
                cancellationToken: cancellationToken
            );

            if (logsResult is OkObjectResult okResult && okResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse)
            {
                var filteredLogs = paginatedResponse.Data.AsQueryable();

                // Apply additional filters
                if (action.HasValue)
                {
                    filteredLogs = filteredLogs.Where(log => log.Action == action.Value);
                }

                if (!string.IsNullOrWhiteSpace(username))
                {
                    filteredLogs = filteredLogs.Where(log => log.ChangedBy.Equals(username, StringComparison.OrdinalIgnoreCase));
                }

                if (flagId.HasValue)
                {
                    filteredLogs = filteredLogs.Where(log => log.FeatureFlagId == flagId.Value);
                }

                var pagedLogs = PaginationHelper.PaginateInMemory(filteredLogs, validPage, validPageSize).ToList();
                var metadata = PaginationHelper.CreateMetadata(validPage, validPageSize, filteredLogs.Count());

                return controller.Ok(new PaginatedApiResponse<AuditLog>
                {
                    Success = true,
                    Data = pagedLogs,
                    Pagination = new Models.PaginationInfo
                    {
                        PageNumber = validPage,
                        PageSize = validPageSize,
                        TotalCount = filteredLogs.Count(),
                        TotalPages = metadata.TotalPages
                    }
                });
            }

            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve filtered audit logs"));
        }
        catch (Exception)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve filtered audit logs"));
        }
    }

    /// <summary>
    /// Gets the most recent changes across all feature flags.
    /// </summary>
    /// <param name="controller">The AuditController instance</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with recent changes</returns>
    public static async Task<IActionResult> GetMostRecentChanges(
        this AuditController controller,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var recentLogsResult = await controller.GetAuditLogsByDateRange(
                startDate: cutoffDate,
                endDate: DateTime.UtcNow,
                page: 1,
                pageSize: maxResults * 2,
                cancellationToken: cancellationToken
            );

            if (recentLogsResult is OkObjectResult recentOkResult && recentOkResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse)
            {
                var recentLogs = paginatedResponse.Data;

                // Get the most recent changes across all flags
                var mostRecent = recentLogs
                    .OrderByDescending(log => log.ChangedAt)
                    .Take(maxResults)
                    .Select(log => new
                    {
                        log.Id,
                        log.FeatureFlagId,
                        log.Action,
                        log.ChangedBy,
                        log.ChangedAt,
                        ChangeDescription = GetChangeDescription(log.Action, log.OldValue, log.NewValue)
                    })
                    .ToList();

                return controller.Ok(ApiResponse<object>.Ok(
                    mostRecent,
                    "Most recent changes retrieved successfully"
                ));
            }

            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve most recent changes"));
        }
        catch (Exception)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve most recent changes"));
        }
    }

    /// <summary>
    /// Gets a human-readable description of a change (copied from AuditController for extension use)
    /// </summary>
    private static string GetChangeDescription(
        Enums.AuditAction action,
        string? oldValue,
        string? newValue)
    {
        return action switch
        {
            Enums.AuditAction.Created => "Feature flag created",
            Enums.AuditAction.Updated => $"Updated from '{oldValue}' to '{newValue}'",
            Enums.AuditAction.Enabled => "Feature flag enabled",
            Enums.AuditAction.Disabled => "Feature flag disabled",
            Enums.AuditAction.RolloutChanged => $"Rollout changed from {oldValue}% to {newValue}%",
            Enums.AuditAction.RuleAdded => "Rule added",
            Enums.AuditAction.RuleRemoved => "Rule removed",
            Enums.AuditAction.VariantUpdated => "Variant updated",
            Enums.AuditAction.Deleted => "Feature flag deleted",
            _ => "Unknown change"
        };
    }

    /// <summary>
    /// Gets audit logs for a specific feature flag with enhanced change details.
    /// </summary>
    /// <param name="controller">The AuditController instance</param>
    /// <param name="featureFlagId">The feature flag ID to get history for</param>
    /// <param name="maxEntries">Maximum number of entries to return (default: 50)</param>
    /// <param name="includeDetailedChanges">Whether to include detailed change information (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with enhanced change history</returns>
    public static async Task<IActionResult> GetEnhancedChangeHistory(
        this AuditController controller,
        int featureFlagId,
        int maxEntries = 50,
        bool includeDetailedChanges = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var historyResult = await controller.GetChangeHistory(
                featureFlagId: featureFlagId,
                maxEntries: maxEntries,
                cancellationToken: cancellationToken
            );

            if (historyResult is OkObjectResult okResult && okResult.Value is ApiResponse<object> apiResponse)
            {
                if (includeDetailedChanges && apiResponse.Data is List<object> changeList && changeList.Count > 0)
                {
                    // Enhance the change data with additional context
                    var enhancedChanges = changeList.Select(change => new
                    {
                        Change = change,
                        Timestamp = DateTime.UtcNow
                    }).ToList();

                    return controller.Ok(ApiResponse<object>.Ok(
                        enhancedChanges,
                        $"Enhanced change history for flag {featureFlagId} retrieved successfully"
                    ));
                }

                return controller.Ok(apiResponse);
            }

            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve enhanced change history"));
        }
        catch (Exception)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve enhanced change history"));
        }
    }
}