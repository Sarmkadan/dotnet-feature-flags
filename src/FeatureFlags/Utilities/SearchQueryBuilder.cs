// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;

namespace FeatureFlags.Utilities;

/// <summary>
/// Fluent query builder for constructing complex feature flag searches with filtering and sorting.
/// Provides a chainable API for building search criteria without writing LINQ queries directly.
/// </summary>
public class FeatureFlagSearchBuilder
{
    private string? _keyFilter;
    private string? _nameFilter;
    private bool? _enabledFilter;
    private RolloutType? _rolloutTypeFilter;
    private string? _createdByFilter;
    private DateTime? _createdAfter;
    private DateTime? _createdBefore;
    private int _skip = 0;
    private int _take = 20;
    private string _sortBy = "Key";
    private bool _sortDescending = false;

    /// <summary>
    /// Filters by feature flag key (supports wildcards with *).
    /// </summary>
    public FeatureFlagSearchBuilder WithKeyContaining(string? key)
    {
        _keyFilter = key;
        return this;
    }

    /// <summary>
    /// Filters by feature flag display name (supports wildcards with *).
    /// </summary>
    public FeatureFlagSearchBuilder WithNameContaining(string? name)
    {
        _nameFilter = name;
        return this;
    }

    /// <summary>
    /// Filters by enabled/disabled status.
    /// </summary>
    public FeatureFlagSearchBuilder WithEnabledStatus(bool enabled)
    {
        _enabledFilter = enabled;
        return this;
    }

    /// <summary>
    /// Filters by rollout type.
    /// </summary>
    public FeatureFlagSearchBuilder WithRolloutType(RolloutType rolloutType)
    {
        _rolloutTypeFilter = rolloutType;
        return this;
    }

    /// <summary>
    /// Filters by who created the flag.
    /// </summary>
    public FeatureFlagSearchBuilder WithCreatedBy(string? userId)
    {
        _createdByFilter = userId;
        return this;
    }

    /// <summary>
    /// Filters by creation date range.
    /// </summary>
    public FeatureFlagSearchBuilder WithCreatedDateRange(DateTime? startDate, DateTime? endDate)
    {
        _createdAfter = startDate;
        _createdBefore = endDate;
        return this;
    }

    /// <summary>
    /// Sets pagination with skip and take.
    /// </summary>
    public FeatureFlagSearchBuilder WithPaging(int skip, int take)
    {
        _skip = Math.Max(0, skip);
        _take = Math.Clamp(take, 1, 1000);
        return this;
    }

    /// <summary>
    /// Sets pagination with page number and size.
    /// </summary>
    public FeatureFlagSearchBuilder WithPage(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 1000);
        _skip = (pageNumber - 1) * pageSize;
        _take = pageSize;
        return this;
    }

    /// <summary>
    /// Sorts by specified field.
    /// </summary>
    public FeatureFlagSearchBuilder SortBy(string fieldName, bool descending = false)
    {
        _sortBy = fieldName;
        _sortDescending = descending;
        return this;
    }

    /// <summary>
    /// Builds an IQueryable for the current search criteria.
    /// </summary>
    public IQueryable<FeatureFlag> Build(IQueryable<FeatureFlag> source)
    {
        // Apply filters
        if (!string.IsNullOrEmpty(_keyFilter))
        {
            source = source.Where(f => f.Key.Contains(_keyFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(_nameFilter))
        {
            source = source.Where(f => f.DisplayName.Contains(_nameFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (_enabledFilter.HasValue)
        {
            source = source.Where(f => f.IsEnabled == _enabledFilter.Value);
        }

        if (_rolloutTypeFilter.HasValue)
        {
            source = source.Where(f => f.RolloutType == _rolloutTypeFilter.Value);
        }

        if (!string.IsNullOrEmpty(_createdByFilter))
        {
            source = source.Where(f => f.CreatedBy == _createdByFilter);
        }

        if (_createdAfter.HasValue)
        {
            source = source.Where(f => f.CreatedAt >= _createdAfter.Value);
        }

        if (_createdBefore.HasValue)
        {
            source = source.Where(f => f.CreatedAt <= _createdBefore.Value);
        }

        // Apply sorting
        source = _sortBy.ToLower() switch
        {
            "key" => _sortDescending ? source.OrderByDescending(f => f.Key) : source.OrderBy(f => f.Key),
            "name" => _sortDescending ? source.OrderByDescending(f => f.DisplayName) : source.OrderBy(f => f.DisplayName),
            "enabled" => _sortDescending ? source.OrderByDescending(f => f.IsEnabled) : source.OrderBy(f => f.IsEnabled),
            "created" => _sortDescending ? source.OrderByDescending(f => f.CreatedAt) : source.OrderBy(f => f.CreatedAt),
            "updated" => _sortDescending ? source.OrderByDescending(f => f.UpdatedAt) : source.OrderBy(f => f.UpdatedAt),
            _ => source.OrderBy(f => f.Key)
        };

        // Apply paging
        source = source.Skip(_skip).Take(_take);

        return source;
    }

    /// <summary>
    /// Executes the search on an in-memory collection.
    /// </summary>
    public List<FeatureFlag> Execute(IEnumerable<FeatureFlag> source)
    {
        var query = source.AsQueryable();
        return Build(query).ToList();
    }

    /// <summary>
    /// Gets a summary of the current search criteria for debugging.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(_keyFilter))
            parts.Add($"Key contains '{_keyFilter}'");
        if (!string.IsNullOrEmpty(_nameFilter))
            parts.Add($"Name contains '{_nameFilter}'");
        if (_enabledFilter.HasValue)
            parts.Add($"IsEnabled = {_enabledFilter.Value}");
        if (_rolloutTypeFilter.HasValue)
            parts.Add($"RolloutType = {_rolloutTypeFilter.Value}");
        if (!string.IsNullOrEmpty(_createdByFilter))
            parts.Add($"CreatedBy = '{_createdByFilter}'");
        if (_createdAfter.HasValue)
            parts.Add($"CreatedAfter = {_createdAfter:O}");
        if (_createdBefore.HasValue)
            parts.Add($"CreatedBefore = {_createdBefore:O}");

        parts.Add($"Sort: {_sortBy} {(_sortDescending ? "DESC" : "ASC")}");
        parts.Add($"Paging: Skip {_skip}, Take {_take}");

        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Simple helper for building common search queries.
/// </summary>
public static class SearchQueryPresets
{
    /// <summary>
    /// Creates a query to find all enabled flags.
    /// </summary>
    public static FeatureFlagSearchBuilder AllEnabled()
    {
        return new FeatureFlagSearchBuilder().WithEnabledStatus(true);
    }

    /// <summary>
    /// Creates a query to find all disabled flags.
    /// </summary>
    public static FeatureFlagSearchBuilder AllDisabled()
    {
        return new FeatureFlagSearchBuilder().WithEnabledStatus(false);
    }

    /// <summary>
    /// Creates a query to find flags modified in the last N days.
    /// </summary>
    public static FeatureFlagSearchBuilder ModifiedInLastDays(int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return new FeatureFlagSearchBuilder().WithCreatedDateRange(startDate, null);
    }

    /// <summary>
    /// Creates a query to find all percentage rollout flags with a specific threshold.
    /// </summary>
    public static FeatureFlagSearchBuilder AllPercentageRollouts(int minPercentage = 0, int maxPercentage = 100)
    {
        return new FeatureFlagSearchBuilder().WithRolloutType(RolloutType.Percentage);
    }

    /// <summary>
    /// Creates a query to find all A/B test flags.
    /// </summary>
    public static FeatureFlagSearchBuilder AllABTests()
    {
        return new FeatureFlagSearchBuilder().WithRolloutType(RolloutType.ABTest);
    }

    /// <summary>
    /// Creates a query to find all rules-based flags.
    /// </summary>
    public static FeatureFlagSearchBuilder AllRulesBased()
    {
        return new FeatureFlagSearchBuilder().WithRolloutType(RolloutType.RulesBased);
    }
}
