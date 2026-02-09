// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// API endpoints for accessing and analyzing audit logs of feature flag changes.
/// Provides comprehensive audit trail querying capabilities for compliance and analysis.
/// </summary>
[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditLogService auditLogService,
        IFeatureFlagService featureFlagService,
        ILogger<AuditController> logger)
    {
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all audit logs for a specific feature flag.
    /// </summary>
    [HttpGet("flags/{featureFlagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFlagAuditLog(int featureFlagId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var (validPage, validPageSize) = PaginationHelper.ValidateAndNormalizePaging(page, pageSize);

            var logs = await _auditLogService.GetAuditLogsPagedAsync(
                skip: (validPage - 1) * validPageSize,
                take: validPageSize);

            // Filter by feature flag ID
            var filteredLogs = logs.Where(l => l.FeatureFlagId == featureFlagId).ToList();

            var metadata = PaginationHelper.CreateMetadata(validPage, validPageSize, filteredLogs.Count);

            return Ok(new PaginatedApiResponse<AuditLog>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log for flag {FlagId}", featureFlagId);
            return StatusCode(500, ApiResponse<object>.Error("Failed to retrieve audit logs"));
        }
    }

    /// <summary>
    /// Gets audit logs filtered by user who made the change.
    /// </summary>
    [HttpGet("by-user/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogsByUser(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var (validPage, validPageSize) = PaginationHelper.ValidateAndNormalizePaging(page, pageSize);

            var logs = await _auditLogService.GetAuditLogsByUserAsync(username);
            var pagedLogs = PaginationHelper.PaginateInMemory(logs, validPage, validPageSize).ToList();

            var metadata = PaginationHelper.CreateMetadata(validPage, validPageSize, logs.Count);

            return Ok(new PaginatedApiResponse<AuditLog>
            {
                Success = true,
                Data = pagedLogs,
                Pagination = new Models.PaginationInfo
                {
                    PageNumber = validPage,
                    PageSize = validPageSize,
                    TotalCount = logs.Count,
                    TotalPages = metadata.TotalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {Username}", username);
            return StatusCode(500, ApiResponse<object>.Error("Failed to retrieve audit logs"));
        }
    }

    /// <summary>
    /// Gets audit logs within a date range.
    /// </summary>
    [HttpGet("by-date-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuditLogsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(ApiResponse<object>.Error("Start date must be before end date"));
            }

            var (validPage, validPageSize) = PaginationHelper.ValidateAndNormalizePaging(page, pageSize);

            var logs = await _auditLogService.GetChangeHistoryAsync(startDate, endDate);
            var pagedLogs = PaginationHelper.PaginateInMemory(logs, validPage, validPageSize).ToList();

            var metadata = PaginationHelper.CreateMetadata(validPage, validPageSize, logs.Count);

            return Ok(new PaginatedApiResponse<AuditLog>
            {
                Success = true,
                Data = pagedLogs,
                Pagination = new Models.PaginationInfo
                {
                    PageNumber = validPage,
                    PageSize = validPageSize,
                    TotalCount = logs.Count,
                    TotalPages = metadata.TotalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs by date range");
            return StatusCode(500, ApiResponse<object>.Error("Failed to retrieve audit logs"));
        }
    }

    /// <summary>
    /// Gets the change history for a specific feature flag showing before/after values.
    /// </summary>
    [HttpGet("history/{featureFlagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChangeHistory(int featureFlagId, [FromQuery] int maxEntries = 50)
    {
        try
        {
            var logs = await _auditLogService.GetAuditLogsAsync();
            var flagLogs = logs
                .Where(l => l.FeatureFlagId == featureFlagId)
                .OrderByDescending(l => l.Timestamp)
                .Take(maxEntries)
                .ToList();

            var history = flagLogs.Select(log => new
            {
                log.Id,
                log.Timestamp,
                log.Action,
                log.ChangedBy,
                log.OldValue,
                log.NewValue,
                Summary = GetChangeDescription(log.Action, log.OldValue, log.NewValue)
            }).ToList();

            return Ok(ApiResponse<object>.Ok(history, "Change history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving change history for flag {FlagId}", featureFlagId);
            return StatusCode(500, ApiResponse<object>.Error("Failed to retrieve change history"));
        }
    }

    /// <summary>
    /// Gets a summary of all audit activity.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditSummary([FromQuery] int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var logs = await _auditLogService.GetChangeHistoryAsync(cutoffDate, DateTime.UtcNow);

            var summary = new
            {
                TotalChanges = logs.Count,
                UniqueUsers = logs.Select(l => l.ChangedBy).Distinct().Count(),
                ChangesByAction = logs
                    .GroupBy(l => l.Action)
                    .Select(g => new { Action = g.Key, Count = g.Count() })
                    .ToList(),
                ChangesByFlag = logs
                    .GroupBy(l => l.FeatureFlagId)
                    .Select(g => new { FlagId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList(),
                MostActiveUsers = logs
                    .GroupBy(l => l.ChangedBy)
                    .Select(g => new { User = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList()
            };

            return Ok(ApiResponse<object>.Ok(summary, "Audit summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit summary");
            return StatusCode(500, ApiResponse<object>.Error("Failed to retrieve audit summary"));
        }
    }

    /// <summary>
    /// Exports audit logs to CSV format.
    /// </summary>
    [HttpGet("export/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAuditLogsCsv([FromQuery] int? featureFlagId = null, [FromQuery] int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var logs = await _auditLogService.GetChangeHistoryAsync(cutoffDate, DateTime.UtcNow);

            if (featureFlagId.HasValue)
            {
                logs = logs.Where(l => l.FeatureFlagId == featureFlagId.Value).ToList();
            }

            var csv = Formatters.CsvExporter.ExportAuditLogs(logs);
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs to CSV");
            return StatusCode(500, ApiResponse<object>.Error("Failed to export audit logs"));
        }
    }

    /// <summary>
    /// Gets a human-readable description of a change.
    /// </summary>
    private static string GetChangeDescription(Enums.AuditAction action, string? oldValue, string? newValue)
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
}
