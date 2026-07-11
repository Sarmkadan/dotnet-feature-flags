#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FeatureFlags.Models;
using FeatureFlags.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Extension methods for <see cref="AuditController"/> providing additional convenience methods
/// for querying and analyzing audit logs.
/// </summary>
public static class AuditControllerExtensions
{
    /// <summary>
    /// Gets recent audit activity across all feature flags within a specified time window.
    /// </summary>
    /// <param name="controller">The <see cref="AuditController"/> instance</param>
    /// <param name="days">Number of days to look back (default: 7)</param>
    /// <param name="maxResults">Maximum number of results to return (default: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with recent audit activity</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="days"/> is less than 1 or <paramref name="maxResults"/> is less than 1</exception>
    public static async Task<IActionResult> GetRecentActivity(
        this AuditController controller,
        int days = 7,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResults);

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

            return logsResult switch
            {
                OkObjectResult okResult when okResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse
                    => controller.Ok(ApiResponse<object>.Ok(
                        new { RecentActivity = paginatedResponse.Data, TotalCount = paginatedResponse.Pagination?.TotalCount ?? 0 },
                        "Recent activity retrieved successfully")),
                _ => controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve recent activity"))
            };
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail($"Failed to retrieve recent activity: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets audit logs filtered by specific action type.
    /// </summary>
    /// <param name="controller">The <see cref="AuditController"/> instance</param>
    /// <param name="action">The audit action type to filter by</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with filtered audit logs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
    public static async Task<IActionResult> GetAuditLogsByAction(
        this AuditController controller,
        Enums.AuditAction action,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

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

            return allLogsResult switch
            {
                OkObjectResult okResult when okResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse
                    => controller.Ok(new PaginatedApiResponse<AuditLog>
                    {
                        Success = true,
                        Data = paginatedResponse.Data
                            .Where(log => log.Action == action)
                            .ToList(),
                        Pagination = new Models.PaginationInfo
                        {
                            PageNumber = validPage,
                            PageSize = validPageSize,
                            TotalCount = paginatedResponse.Data.Count(log => log.Action == action),
                            TotalPages = PaginationHelper.CreateMetadata(validPage, validPageSize, paginatedResponse.Data.Count(log => log.Action == action)).TotalPages
                        }
                    }),
                _ => controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve audit logs by action"))
            };
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail($"Failed to retrieve audit logs by action: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets audit logs with enhanced filtering capabilities including multiple criteria.
    /// </summary>
    /// <param name="controller">The <see cref="AuditController"/> instance</param>
    /// <param name="startDate">Start date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <param name="action">Optional action type to filter by</param>
    /// <param name="username">Optional username to filter by</param>
    /// <param name="flagId">Optional feature flag ID to filter by</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with filtered audit logs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="startDate"/> is after <paramref name="endDate"/></exception>
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
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfLessThan(endDate, startDate, nameof(endDate));

        try
        {
            var (validPage, validPageSize) = PaginationHelper.ValidateAndNormalizePaging(page, pageSize);

            // Get all logs in date range first
            var logsResult = await controller.GetAuditLogsByDateRange(
                startDate: startDate,
                endDate: endDate,
                page: 1,
                pageSize: 10000,
                cancellationToken: cancellationToken
            );

            return logsResult switch
            {
                OkObjectResult okResult when okResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse
                    => controller.Ok(new PaginatedApiResponse<AuditLog>
                    {
                        Success = true,
                        Data = ApplyFilters(paginatedResponse.Data.AsQueryable(), action, username, flagId)
                            .Skip((validPage - 1) * validPageSize)
                            .Take(validPageSize)
                            .ToList(),
                        Pagination = new Models.PaginationInfo
                        {
                            PageNumber = validPage,
                            PageSize = validPageSize,
                            TotalCount = ApplyFilters(paginatedResponse.Data.AsQueryable(), action, username, flagId).Count(),
                            TotalPages = PaginationHelper.CreateMetadata(validPage, validPageSize, ApplyFilters(paginatedResponse.Data.AsQueryable(), action, username, flagId).Count()).TotalPages
                        }
                    }),
                _ => controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve filtered audit logs"))
            };
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail($"Failed to retrieve filtered audit logs: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets the most recent changes across all feature flags.
    /// </summary>
    /// <param name="controller">The <see cref="AuditController"/> instance</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with recent changes</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
    public static async Task<IActionResult> GetMostRecentChanges(
        this AuditController controller,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResults);

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

            return recentLogsResult switch
            {
                OkObjectResult recentOkResult when recentOkResult.Value is PaginatedApiResponse<AuditLog> paginatedResponse
                    => controller.Ok(ApiResponse<object>.Ok(
                        paginatedResponse.Data
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
                            .ToList(),
                        "Most recent changes retrieved successfully")),
                _ => controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve most recent changes"))
            };
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail($"Failed to retrieve most recent changes: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets audit logs for a specific feature flag with enhanced change details.
    /// </summary>
    /// <param name="controller">The <see cref="AuditController"/> instance</param>
    /// <param name="featureFlagId">The feature flag ID to get history for</param>
    /// <param name="maxEntries">Maximum number of entries to return (default: 50)</param>
    /// <param name="includeDetailedChanges">Whether to include detailed change information (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result with enhanced change history</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="featureFlagId"/> is less than 1 or <paramref name="maxEntries"/> is less than 1</exception>
    public static async Task<IActionResult> GetEnhancedChangeHistory(
        this AuditController controller,
        int featureFlagId,
        int maxEntries = 50,
        bool includeDetailedChanges = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(featureFlagId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEntries);

        try
        {
            var historyResult = await controller.GetChangeHistory(
                featureFlagId: featureFlagId,
                maxEntries: maxEntries,
                cancellationToken: cancellationToken
            );

            return historyResult switch
            {
                OkObjectResult okResult when okResult.Value is ApiResponse<object> apiResponse
                    => includeDetailedChanges && apiResponse.Data is List<object> changeList && changeList.Count > 0
                        ? controller.Ok(ApiResponse<object>.Ok(
                            changeList.Select(change => new { Change = change, Timestamp = DateTime.UtcNow })
                                .ToList(),
                            $"Enhanced change history for flag {featureFlagId} retrieved successfully"))
                        : controller.Ok(apiResponse),
                _ => controller.StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve enhanced change history"))
            };
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, ApiResponse<object>.Fail($"Failed to retrieve enhanced change history: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets a human-readable description of a change (copied from AuditController for extension use)
    /// </summary>
    /// <param name="action">The audit action type</param>
    /// <param name="oldValue">The old value (if applicable)</param>
    /// <param name="newValue">The new value (if applicable)</param>
    /// <returns>Human-readable change description</returns>
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
    /// Applies multiple filters to a queryable collection of audit logs.
    /// </summary>
    /// <param name="query">The queryable collection to filter</param>
    /// <param name="action">Optional action type to filter by</param>
    /// <param name="username">Optional username to filter by</param>
    /// <param name="flagId">Optional feature flag ID to filter by</param>
    /// <returns>Filtered queryable collection</returns>
    private static IQueryable<AuditLog> ApplyFilters(
        IQueryable<AuditLog> query,
        Enums.AuditAction? action,
        string? username,
        int? flagId)
    {
        var filteredQuery = query;

        if (action.HasValue)
        {
            filteredQuery = filteredQuery.Where(log => log.Action == action.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            filteredQuery = filteredQuery.Where(log => log.ChangedBy.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        if (flagId.HasValue)
        {
            filteredQuery = filteredQuery.Where(log => log.FeatureFlagId == flagId.Value);
        }

        return filteredQuery;
    }
}