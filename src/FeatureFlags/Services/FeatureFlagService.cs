#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Configuration;
using FeatureFlags.Enums;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for feature flag operations.
/// Coordinates evaluation, persistence, and audit logging of feature flags.
/// </summary>
public class FeatureFlagService : IFeatureFlagService {
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IRuleEvaluationService _ruleEvaluationService;
    private readonly IPercentageRolloutService _percentageRolloutService;
    private readonly IFlagEvaluationLogService _evaluationLogService;
    private readonly FeatureFlagOptions _options;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(
        IFeatureFlagRepository featureFlagRepository,
        IAuditLogRepository auditLogRepository,
        IRuleEvaluationService ruleEvaluationService,
        IPercentageRolloutService percentageRolloutService,
        IFlagEvaluationLogService evaluationLogService,
        IOptions<FeatureFlagOptions> options,
        ILogger<FeatureFlagService> logger)
    {
        _featureFlagRepository = featureFlagRepository;
        _auditLogRepository = auditLogRepository;
        _ruleEvaluationService = ruleEvaluationService;
        _percentageRolloutService = percentageRolloutService;
        _evaluationLogService = evaluationLogService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureFlagKey, UserContext userContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        if (!userContext.IsValid())
            throw new InvalidOperationException("User context is invalid");

        try
        {
            var featureFlag = await _featureFlagRepository.GetByKeyAsync(featureFlagKey);
            if (featureFlag is null)
            {
                _logger.LogWarning("Feature flag '{Key}' not found", featureFlagKey);
                // Log evaluation even if feature flag is not found, with result "Not Found"
                await LogAuditAsync(0, AuditAction.Evaluated, userContext.UserId, featureFlagKey, "Not Found", $"Feature flag '{featureFlagKey}' evaluated by '{userContext.UserId}' - Not Found");
                RecordEvaluationLog(featureFlagKey, userContext.UserId, false, "FlagNotFound");
                return false;
            }

            if (!featureFlag.IsEnabled)
            {
                // Log evaluation when feature flag is disabled
                await LogAuditAsync(featureFlag.Id, AuditAction.Evaluated, userContext.UserId, featureFlagKey, "Disabled", $"Feature flag '{featureFlagKey}' evaluated by '{userContext.UserId}' - Disabled");
                RecordEvaluationLog(featureFlagKey, userContext.UserId, false, "FlagDisabled");
                return false;
            }

            string reason = featureFlag.RolloutType switch
            {
                RolloutType.Percentage => "PercentageRollout",
                RolloutType.RulesBased => "RulesBased",
                RolloutType.ABTest => "ABTest",
                RolloutType.Full => "Full",
                RolloutType.None => "None",
                _ => "Unknown"
            };

            bool result = featureFlag.RolloutType switch
            {
                RolloutType.Percentage => await _percentageRolloutService.EvaluateAsync(featureFlag, userContext),
                RolloutType.RulesBased => await _ruleEvaluationService.EvaluateAsync(featureFlag, userContext),
                RolloutType.ABTest => await _ruleEvaluationService.EvaluateAsync(featureFlag, userContext),
                RolloutType.Full => true,
                RolloutType.None => false,
                _ => false
            };

            // Log successful evaluation
            await LogAuditAsync(featureFlag.Id, AuditAction.Evaluated, userContext.UserId, featureFlagKey, result.ToString(), $"Feature flag '{featureFlagKey}' evaluated by '{userContext.UserId}' - Result: {result}");
            RecordEvaluationLog(featureFlagKey, userContext.UserId, result, reason);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag '{Key}'", featureFlagKey);
            throw;
        }
    }

    public async Task<FeatureFlag?> GetFeatureFlagAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        return await _featureFlagRepository.GetByIdAsync(id);
    }

    public async Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key, CancellationToken cancellationToken = default)
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

    public async Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag, string createdBy, CancellationToken cancellationToken = default)
    {
        if (featureFlag is null)
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

    public async Task UpdateFeatureFlagAsync(FeatureFlag featureFlag, string updatedBy, CancellationToken cancellationToken = default)
    {
        if (featureFlag is null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be empty", nameof(updatedBy));

        var existing = await _featureFlagRepository.GetByIdAsync(featureFlag.Id);
        if (existing is null)
            throw new FeatureFlagNotFoundException(featureFlag.Key);

        var oldSnapshot = existing.GetSnapshot();
        featureFlag.UpdatedBy = updatedBy;
        featureFlag.UpdatedAt = DateTime.UtcNow;

        await _featureFlagRepository.UpdateAsync(featureFlag);

        await LogAuditAsync(featureFlag.Id, AuditAction.Updated, updatedBy, oldSnapshot, featureFlag.GetSnapshot(), "Feature flag updated");

        _logger.LogInformation("Feature flag '{Key}' updated by {User}", featureFlag.Key, updatedBy);
    }

    public async Task DeleteFeatureFlagAsync(int id, string deletedBy, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be empty", nameof(deletedBy));

        var existing = await _featureFlagRepository.GetByIdAsync(id);
        if (existing is null)
            throw new FeatureFlagNotFoundException(id.ToString());

        var snapshot = existing.GetSnapshot();
        await _featureFlagRepository.DeleteAsync(id);

        await LogAuditAsync(id, AuditAction.Deleted, deletedBy, snapshot, string.Empty, "Feature flag deleted");

        _logger.LogInformation("Feature flag with id {Id} deleted by {User}", id, deletedBy);
    }

    public async Task EnableFeatureFlagAsync(int id, string modifiedBy, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag is null)
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

    public async Task DisableFeatureFlagAsync(int id, string modifiedBy, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be > 0", nameof(id));

        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag is null)
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

    public async Task<string?> GetVariantAsync(string featureFlagKey, UserContext userContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (userContext is null || !userContext.IsValid())
            throw new InvalidOperationException("User context is invalid");

        var featureFlag = await _featureFlagRepository.GetWithVariantsAsync(await GetIdByKeyAsync(featureFlagKey));
        if (featureFlag is null || !featureFlag.IsEnabled)
        {
            await LogAuditAsync(featureFlag?.Id ?? 0, AuditAction.Evaluated, userContext.UserId, featureFlagKey, "Not Enabled/Found", $"Feature flag '{featureFlagKey}' evaluated by '{userContext.UserId}' for variant - Not Enabled/Found");
            return null;
        }

        if (featureFlag.RolloutType != RolloutType.ABTest)
        {
            await LogAuditAsync(featureFlag.Id, AuditAction.Evaluated, userContext.UserId, featureFlagKey, "Not ABTest", $"Feature flag '{featureFlagKey}' evaluated by '{userContext.UserId}' for variant - Not ABTest Type");
            return null;
        }

        var hash = userContext.GetConsistentHash(featureFlagKey);
        var current = 0;
        string? selectedVariantKey = null;

        foreach (var variant in featureFlag.Variants.OrderBy(v => v.Id))
        {
            current += variant.AllocationPercentage;
            if (hash < current)
            {
                variant.RecordUserAssignment();
                await _featureFlagRepository.SaveChangesAsync();
                selectedVariantKey = variant.VariantKey;
                break;
            }
        }
        
        if (selectedVariantKey == null) {
            selectedVariantKey = featureFlag.Variants.FirstOrDefault()?.VariantKey;
        }

        await LogAuditAsync(featureFlag.Id, AuditAction.Evaluated, userContext.UserId, featureFlagKey, selectedVariantKey ?? "No Variant", $"Feature flag '{featureFlagKey}' evaluated by '{userContext.UserId}' for variant - Result: {selectedVariantKey ?? "No Variant"}");
        return selectedVariantKey;

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
        if (flag is null)
            throw new FeatureFlagNotFoundException(key);

        return flag.Id;
    }

    private void RecordEvaluationLog(string flagName, string userId, bool result, string reason)
    {
        if (!_options.EnableAuditLog)
            return;

        _evaluationLogService.Log(new FlagEvaluationLog
        {
            FlagName = flagName,
            UserId = userId,
            Result = result,
            Timestamp = DateTime.UtcNow,
            Reason = reason
        });
    }

    private async Task LogAuditAsync(int featureFlagId, AuditAction action, string changedBy, string oldValue, string newValue, string description)
    {
        try
        {
            var finalChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "Anonymous" : changedBy;

            // Handle evaluation logs specifically if no description is provided, using featureFlagKey and result
            if (action == AuditAction.Evaluated && string.IsNullOrWhiteSpace(description))
            {
                description = $"Feature flag '{oldValue}' evaluated to '{newValue}' by '{finalChangedBy}'";
            }

            var auditLog = new AuditLog
            {
                FeatureFlagId = featureFlagId,
                Action = action,
                ChangedBy = finalChangedBy,
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

    Task<bool> IFeatureFlagService.IsEnabledAsync(string featureFlagKey, UserContext userContext) => IsEnabledAsync(featureFlagKey, userContext);
    Task<FeatureFlag?> IFeatureFlagService.GetFeatureFlagAsync(int id) => GetFeatureFlagAsync(id);
    Task<FeatureFlag?> IFeatureFlagService.GetFeatureFlagByKeyAsync(string key) => GetFeatureFlagByKeyAsync(key);
    Task<FeatureFlag> IFeatureFlagService.CreateFeatureFlagAsync(FeatureFlag featureFlag, string createdBy) => CreateFeatureFlagAsync(featureFlag, createdBy);
    Task IFeatureFlagService.UpdateFeatureFlagAsync(FeatureFlag featureFlag, string updatedBy) => UpdateFeatureFlagAsync(featureFlag, updatedBy);
    Task IFeatureFlagService.DeleteFeatureFlagAsync(int id, string deletedBy) => DeleteFeatureFlagAsync(id, deletedBy);
    Task IFeatureFlagService.EnableFeatureFlagAsync(int id, string modifiedBy) => EnableFeatureFlagAsync(id, modifiedBy);
    Task IFeatureFlagService.DisableFeatureFlagAsync(int id, string modifiedBy) => DisableFeatureFlagAsync(id, modifiedBy);
    Task<string?> IFeatureFlagService.GetVariantAsync(string featureFlagKey, UserContext userContext) => GetVariantAsync(featureFlagKey, userContext);
}
