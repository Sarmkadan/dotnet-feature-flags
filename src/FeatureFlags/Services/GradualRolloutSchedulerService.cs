#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Data;
using FeatureFlags.Enums;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for managing gradual feature flag rollouts with time-based scheduling.
/// Evaluates active rollout strategies and advances percentage allocations according to
/// configured start dates, end dates, and daily increment values.
/// </summary>
public sealed class GradualRolloutSchedulerService {
    private readonly FeatureFlagDbContext _context;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<GradualRolloutSchedulerService> _logger;

    public GradualRolloutSchedulerService(
        FeatureFlagDbContext context,
        IAuditLogRepository auditLogRepository,
        ILogger<GradualRolloutSchedulerService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<int> ProcessScheduledRolloutsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing scheduled gradual rollouts");

        var strategies = await _context.RolloutStrategies
            .Where(s => s.IsGradual && s.StartDate is not null)
            .Include(s => s.FeatureFlag)
            .ToListAsync(cancellationToken);

        var updatedCount = 0;

        foreach (var strategy in strategies)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (!strategy.IsActive() || strategy.FeatureFlag is null)
                continue;

            try
            {
                if (await ApplyRolloutProgressAsync(strategy, "scheduler", cancellationToken))
                    updatedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing rollout for feature flag {FlagId}", strategy.FeatureFlagId);
            }
        }

        _logger.LogInformation("Gradual rollout cycle complete: {Count} flags updated", updatedCount);
        return updatedCount;
    }

    /// <inheritdoc/>
    public async Task<RolloutScheduleStatus?> GetScheduleStatusAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("Feature flag id must be positive", nameof(featureFlagId));

        var strategy = await _context.RolloutStrategies
            .Where(s => s.FeatureFlagId == featureFlagId && s.IsGradual)
            .Include(s => s.FeatureFlag)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (strategy?.FeatureFlag is null)
            return null;

        var currentPct = strategy.GetCurrentPercentage();
        var targetPct = strategy.EndPercentage ?? 100;
        var isComplete = currentPct >= targetPct;

        var daysRemaining = 0;
        if (!isComplete && strategy.DailyIncrement is > 0)
            daysRemaining = (int)Math.Ceiling((double)(targetPct - currentPct) / strategy.DailyIncrement.Value);

        return new RolloutScheduleStatus
        {
            FeatureFlagId = featureFlagId,
            FeatureFlagKey = strategy.FeatureFlag.Key,
            CurrentPercentage = strategy.FeatureFlag.PercentageRollout ?? currentPct,
            TargetPercentage = targetPct,
            DailyIncrement = strategy.DailyIncrement,
            StartDate = strategy.StartDate,
            EndDate = strategy.EndDate,
            IsActive = strategy.IsActive(),
            IsComplete = isComplete,
            EstimatedDaysRemaining = daysRemaining
        };
    }

    /// <inheritdoc/>
    public async Task<bool> AdvanceRolloutAsync(int featureFlagId, string advancedBy, CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("Feature flag id must be positive", nameof(featureFlagId));

        if (string.IsNullOrWhiteSpace(advancedBy))
            throw new ArgumentException("AdvancedBy cannot be empty", nameof(advancedBy));

        var strategy = await _context.RolloutStrategies
            .Where(s => s.FeatureFlagId == featureFlagId && s.IsGradual)
            .Include(s => s.FeatureFlag)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (strategy?.FeatureFlag is null)
        {
            _logger.LogWarning("No gradual rollout strategy found for feature flag {FlagId}", featureFlagId);
            return false;
        }

        return await ApplyRolloutProgressAsync(strategy, advancedBy, cancellationToken);
    }

    private async Task<bool> ApplyRolloutProgressAsync(
        RolloutStrategy strategy,
        string modifiedBy,
        CancellationToken cancellationToken)
    {
        var flag = strategy.FeatureFlag!;
        var computedPct = strategy.GetCurrentPercentage();
        var previousPct = flag.PercentageRollout ?? 0;

        if (computedPct == previousPct)
            return false;

        var oldSnapshot = flag.GetSnapshot();
        flag.PercentageRollout = computedPct;
        flag.UpdatedAt = DateTime.UtcNow;
        flag.UpdatedBy = modifiedBy;
        strategy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Rollout advanced for '{Key}': {Prev}% → {Curr}%",
            flag.Key, previousPct, computedPct);

        await RecordAuditAsync(flag, modifiedBy, oldSnapshot);
        return true;
    }

    private async Task RecordAuditAsync(FeatureFlag flag, string changedBy, string oldSnapshot)
    {
        try
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                FeatureFlagId = flag.Id,
                Action = AuditAction.RolloutChanged,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                OldValue = oldSnapshot,
                NewValue = flag.GetSnapshot(),
                Description = $"Gradual rollout advanced to {flag.PercentageRollout}%"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record rollout audit for flag {Id}", flag.Id);
        }
    }
}
