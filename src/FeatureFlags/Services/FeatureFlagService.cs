// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for feature flag operations.
/// Coordinates evaluation, persistence, and audit logging of feature flags.
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IRuleEvaluationService _ruleEvaluationService;
    private readonly IPercentageRolloutService _percentageRolloutService;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(
        IFeatureFlagRepository featureFlagRepository,
        IAuditLogRepository auditLogRepository,
        IRuleEvaluationService ruleEvaluationService,
        IPercentageRolloutService percentageRolloutService,
        ILogger<FeatureFlagService> logger)
    {
        _featureFlagRepository = featureFlagRepository;
        _auditLogRepository = auditLogRepository;
        _ruleEvaluationService = ruleEvaluationService;
        _percentageRolloutService = percentageRolloutService;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureFlagKey, UserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        if (!userContext.IsValid())
            throw new InvalidOperationException("User context is invalid");

        try
        {
            var featureFlag = await _featureFlagRepository.GetByKeyAsync(featureFlagKey);
            if (featureFlag == null)
            {
                _logger.LogWarning("Feature flag '{Key}' not found", featureFlagKey);
                return false;
            }

            if (!featureFlag.IsEnabled)
                return false;

            return featureFlag.RolloutType switch
            {
                RolloutType.Percentage => await _percentageRolloutService.EvaluateAsync(featureFlag, userContext),
                RolloutType.RulesBased => await _ruleEvaluationService.EvaluateAsync(featureFlag, userContext),
                RolloutType.ABTest => await _ruleEvaluationService.EvaluateAsync(featureFlag, userContext),
                RolloutType.Full => true,
                RolloutType.None => false,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag '{Key}'", featureFlagKey);
            throw;
        }
    }

    public async Task<FeatureFlag?> GetFeatureFlagAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        return await _featureFlagRepository.GetByIdAsync(id);
    }

    public async Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return await _featureFlagRepository.GetByKeyAsync(key);
    }

    public async Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync()
    {
        return await _featureFlagRepository.GetAllAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> GetEnabledFeatureFlagsAsync()
    {
        return await _featureFlagRepository.GetEnabledAsync();
    }

    public async Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag, string createdBy)
    {
        if (featureFlag == null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        if (await _featureFlagRepository.KeyExistsAsync(featureFlag.Key))
            throw new InvalidOperationException($"Feature flag with key '{featureFlag.Key}' already exists");

        featureFlag.CreatedBy = createdBy;
        featureFlag.UpdatedBy = createdBy;
        featureFlag.CreatedAt = DateTime.UtcNow;
        featureFlag.UpdatedAt = DateTime.UtcNow;

        var created = await _featureFlagRepository.AddAsync(featureFlag);

        await LogAuditAsync(created.Id, AuditAction.Created, createdBy, string.Empty, created.GetSnapshot(), $"Feature flag '{created.Key}' created");

        _logger.LogInformation("Feature flag '{Key}' created by {User}", created.Key, createdBy);
        return created;
    }

    public async Task UpdateFeatureFlagAsync(FeatureFlag featureFlag, string updatedBy)
    {
        if (featureFlag == null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be empty", nameof(updatedBy));

        var existing = await _featureFlagRepository.GetByIdAsync(featureFlag.Id);
        if (existing == null)
            throw new FeatureFlagNotFoundException(featureFlag.Key);

        var oldSnapshot = existing.GetSnapshot();
        featureFlag.UpdatedBy = updatedBy;
        featureFlag.UpdatedAt = DateTime.UtcNow;

        await _featureFlagRepository.UpdateAsync(featureFlag);

        await LogAuditAsync(featureFlag.Id, AuditAction.Updated, updatedBy, oldSnapshot, featureFlag.GetSnapshot(), "Feature flag updated");

        _logger.LogInformation("Feature flag '{Key}' updated by {User}", featureFlag.Key, updatedBy);
    }

    public async Task DeleteFeatureFlagAsync(int id, string deletedBy)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be empty", nameof(deletedBy));

        var existing = await _featureFlagRepository.GetByIdAsync(id);
        if (existing == null)
            throw new FeatureFlagNotFoundException(id.ToString());

        var snapshot = existing.GetSnapshot();
        await _featureFlagRepository.DeleteAsync(id);

        await LogAuditAsync(id, AuditAction.Deleted, deletedBy, snapshot, string.Empty, "Feature flag deleted");

        _logger.LogInformation("Feature flag with id {Id} deleted by {User}", id, deletedBy);
    }

    public async Task EnableFeatureFlagAsync(int id, string modifiedBy)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag == null)
            throw new FeatureFlagNotFoundException(id.ToString());

        if (featureFlag.IsEnabled)
            return;

        featureFlag.IsEnabled = true;
        featureFlag.UpdatedBy = modifiedBy;
        featureFlag.UpdatedAt = DateTime.UtcNow;

        await _featureFlagRepository.UpdateAsync(featureFlag);
        await LogAuditAsync(id, AuditAction.Enabled, modifiedBy, "false", "true", $"Feature flag '{featureFlag.Key}' enabled");

        _logger.LogInformation("Feature flag '{Key}' enabled by {User}", featureFlag.Key, modifiedBy);
    }

    public async Task DisableFeatureFlagAsync(int id, string modifiedBy)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag == null)
            throw new FeatureFlagNotFoundException(id.ToString());

        if (!featureFlag.IsEnabled)
            return;

        featureFlag.IsEnabled = false;
        featureFlag.UpdatedBy = modifiedBy;
        featureFlag.UpdatedAt = DateTime.UtcNow;

        await _featureFlagRepository.UpdateAsync(featureFlag);
        await LogAuditAsync(id, AuditAction.Disabled, modifiedBy, "true", "false", $"Feature flag '{featureFlag.Key}' disabled");

        _logger.LogInformation("Feature flag '{Key}' disabled by {User}", featureFlag.Key, modifiedBy);
    }

    public async Task<string?> GetVariantAsync(string featureFlagKey, UserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (userContext == null || !userContext.IsValid())
            throw new InvalidOperationException("User context is invalid");

        var featureFlag = await _featureFlagRepository.GetWithVariantsAsync(await GetIdByKeyAsync(featureFlagKey));
        if (featureFlag == null || !featureFlag.IsEnabled)
            return null;

        if (featureFlag.RolloutType != RolloutType.ABTest)
            return null;

        var hash = userContext.GetConsistentHash(featureFlagKey);
        var current = 0;

        foreach (var variant in featureFlag.Variants.OrderBy(v => v.Id))
        {
            current += variant.AllocationPercentage;
            if (hash < current)
            {
                variant.RecordUserAssignment();
                await _featureFlagRepository.SaveChangesAsync();
                return variant.VariantKey;
            }
        }

        return featureFlag.Variants.FirstOrDefault()?.VariantKey;
    }

    public async Task<IEnumerable<FeatureFlag>> SearchFeatureFlagsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllFeatureFlagsAsync();

        return await _featureFlagRepository.SearchAsync(searchTerm);
    }

    private async Task<int> GetIdByKeyAsync(string key)
    {
        var flag = await _featureFlagRepository.GetByKeyAsync(key);
        if (flag == null)
            throw new FeatureFlagNotFoundException(key);

        return flag.Id;
    }

    private async Task LogAuditAsync(int featureFlagId, AuditAction action, string changedBy, string oldValue, string newValue, string description)
    {
        try
        {
            var auditLog = new AuditLog
            {
                FeatureFlagId = featureFlagId,
                Action = action,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                OldValue = oldValue,
                NewValue = newValue,
                Description = description
            };

            await _auditLogRepository.AddAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry for feature flag {Id}", featureFlagId);
        }
    }
}
