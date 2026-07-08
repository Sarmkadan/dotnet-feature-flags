using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FeatureFlags.Enums;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using FeatureFlags.Services;
using FeatureFlags.Caching;
using System.Collections.Concurrent;

namespace dotnet_feature_flags.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0, warmupCount: 3, iterationCount: 10)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class FeatureFlagsBenchmarks
{
    private PercentageRolloutService _percentageRolloutService;
    private RuleEvaluationService _ruleEvaluationService;
    private FeatureFlagService _featureFlagService;
    private UserContext _userContext;
    private FeatureFlag _percentageFlag;
    private FeatureFlag _ruleFlag;
    private FeatureFlag _abTestFlag;
    private ICacheService _cacheService;
    private MockFeatureFlagRepository _repository;

    private MockAuditLogRepository _auditLogRepository;
    private MockFlagEvaluationLogService _flagEvaluationLogService;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize services
        _repository = new MockFeatureFlagRepository();
        _percentageRolloutService = new PercentageRolloutService(null);
        _ruleEvaluationService = new RuleEvaluationService(_repository, null);
        _cacheService = new MockCacheService();
        _auditLogRepository = new MockAuditLogRepository();
        _flagEvaluationLogService = new MockFlagEvaluationLogService();

        _featureFlagService = new FeatureFlagService(
            _repository,
            _auditLogRepository,
            _ruleEvaluationService,
            _percentageRolloutService,
            _flagEvaluationLogService,
            Microsoft.Extensions.Options.Options.Create(new FeatureFlags.Configuration.FeatureFlagOptions()),
            null!
        );

        // Create test user context
        _userContext = new UserContext
        {
            UserId = "user12345",
            Email = "user@example.com",
            Tier = "premium",
            Country = "US",
            Region = "north-america"
        };

        // Create percentage rollout flag (25% enabled)
        _percentageFlag = new FeatureFlag
        {
            Key = "new-dashboard",
            DisplayName = "New Dashboard",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 25,
            IsEnabled = true
        };

        // Create rule-based flag with multiple conditions
        _ruleFlag = new FeatureFlag
        {
            Key = "premium-feature",
            DisplayName = "Premium Feature",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new List<Rule>
            {
                new Rule
                {
                    Name = "Premium Users",
                    Priority = 1,
                    ConditionLogic = "AND",
                    IsActive = true,
                    Conditions = new List<Condition>
                    {
                        new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals , ExpectedValue = "premium", IsActive = true },
                        new Condition { AttributeName = "country", Operator = ConditionOperator.In , ExpectedValue = "US,CA,UK,DE", IsActive = true }
                    }
                },
                new Rule
                {
                    Name = "Beta Testers",
                    Priority = 2,
                    ConditionLogic = "AND",
                    IsActive = true,
                    Conditions = new List<Condition>
                    {
                        new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals , ExpectedValue = "beta", IsActive = true },
                        new Condition { AttributeName = "tags", Operator = ConditionOperator.Contains , ExpectedValue = "beta-tester", IsActive = true }
                    }
                }
            }
        };

        // Create A/B test flag
        _abTestFlag = new FeatureFlag
        {
            Key = "checkout-redesign",
            DisplayName = "Checkout Redesign",
            RolloutType = RolloutType.ABTest,
            IsEnabled = true,
            Variants = new List<ABTestVariant>
            {
                new ABTestVariant { DisplayName = "Control", AllocationPercentage = 50, VariantKey = "control-variant" },
                new ABTestVariant { DisplayName = "Treatment", AllocationPercentage = 50, VariantKey = "treatment-variant" }
            }
        };
    }

    [BenchmarkCategory("User Context")]
    [Benchmark(Baseline = true)]
    public int GetConsistentHash()
    {
        return _userContext.GetConsistentHash("test-feature-key");
    }

    [BenchmarkCategory("Percentage Rollout")]
    [Benchmark]
    public bool PercentageRolloutEvaluation()
    {
        return _percentageRolloutService.EvaluateAsync(_percentageFlag, _userContext).Result;
    }

    [BenchmarkCategory("Percentage Rollout")]
    [Benchmark]
    public bool PercentageRolloutEvaluation_100()
    {
        var flag = new FeatureFlag
        {
            Key = "full-rollout",
            PercentageRollout = 100,
            IsEnabled = true
        };
        return _percentageRolloutService.EvaluateAsync(flag, _userContext).Result;
    }

    [BenchmarkCategory("Percentage Rollout")]
    [Benchmark]
    public bool PercentageRolloutEvaluation_0()
    {
        var flag = new FeatureFlag
        {
            Key = "no-rollout",
            PercentageRollout = 0,
            IsEnabled = true
        };
        return _percentageRolloutService.EvaluateAsync(flag, _userContext).Result;
    }

    [BenchmarkCategory("Rule Evaluation")]
    [Benchmark]
    public bool RuleBasedEvaluation_Match()
    {
        return _ruleEvaluationService.EvaluateAsync(_ruleFlag, _userContext).Result;
    }

    [BenchmarkCategory("Rule Evaluation")]
    [Benchmark]
    public bool RuleBasedEvaluation_NoMatch()
    {
        var context = new UserContext
        {
            UserId = "user999",
            Email = "user@example.com",
            Tier = "free",
            Country = "RU"
        };
        return _ruleEvaluationService.EvaluateAsync(_ruleFlag, context).Result;
    }

    [BenchmarkCategory("Rule Evaluation")]
    [Benchmark]
    public bool RuleBasedEvaluation_SingleCondition()
    {
        var simpleFlag = new FeatureFlag
        {
            Key = "simple-rule",
            DisplayName = "Simple Rule",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new List<Rule>
            {
                new Rule
                {
                    Name = "Tier Check",
                    Priority = 1,
                    ConditionLogic = "AND",
                    IsActive = true,
                    Conditions = new List<Condition>
                    {
                        new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals , ExpectedValue = "premium", IsActive = true }
                    }
                }
            }
        };
        return _ruleEvaluationService.EvaluateAsync(simpleFlag, _userContext).Result;
    }

    [BenchmarkCategory("A/B Test")]
    [Benchmark]
    public string? ABTestVariantAssignment()
    {
        return _featureFlagService.GetVariantAsync(_abTestFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("A/B Test")]
    [Benchmark]
    public string? ABTestVariantAssignment_MultipleVariants()
    {
        var multiVariantFlag = new FeatureFlag
        {
            Key = "multi-variant-test",
            DisplayName = "Multi Variant Test",
            RolloutType = RolloutType.ABTest,
            IsEnabled = true,
            Variants = new List<ABTestVariant>
            {
                new ABTestVariant { DisplayName = "Variant A", AllocationPercentage = 30, VariantKey = "variant-a" },
                new ABTestVariant { DisplayName = "Variant B", AllocationPercentage = 30, VariantKey = "variant-b" },
                new ABTestVariant { DisplayName = "Variant C", AllocationPercentage = 40, VariantKey = "variant-c" }
            }
        };
        return _featureFlagService.GetVariantAsync(multiVariantFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Full Evaluation")]
    [Benchmark]
    public bool FullFeatureFlagEvaluation_Percentage()
    {
        var service = new FeatureFlagService(
            new MockFeatureFlagRepository(),
            new MockAuditLogRepository(),
            _ruleEvaluationService,
            _percentageRolloutService,
            new MockFlagEvaluationLogService(),
            null,
            null
        );
        return service.IsEnabledAsync(_percentageFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Full Evaluation")]
    [Benchmark]
    public bool FullFeatureFlagEvaluation_RuleBased()
    {
        var service = new FeatureFlagService(
            new MockFeatureFlagRepository(),
            new MockAuditLogRepository(),
            _ruleEvaluationService,
            _percentageRolloutService,
            new MockFlagEvaluationLogService(),
            null,
            null
        );
        return service.IsEnabledAsync(_ruleFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Full Evaluation")]
    [Benchmark]
    public bool FullFeatureFlagEvaluation_ABTest()
    {
        var service = new FeatureFlagService(
            new MockFeatureFlagRepository(),
            new MockAuditLogRepository(),
            _ruleEvaluationService,
            _percentageRolloutService,
            new MockFlagEvaluationLogService(),
            null,
            null
        );
        return service.IsEnabledAsync(_abTestFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Complex Scenarios")]
    [Benchmark]
    public bool ComplexRuleEvaluation_ManyConditions()
    {
        var complexFlag = new FeatureFlag
        {
            Key = "complex-conditions",
            DisplayName = "Complex Conditions",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new List<Rule>
            {
                new Rule
                {
                    Name = "Complex Rule",
                    Priority = 1,
                    ConditionLogic = "AND",
                    IsActive = true,
                    Conditions = new List<Condition>
                    {
                        new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals , ExpectedValue = "premium", IsActive = true },
                        new Condition { AttributeName = "country", Operator = ConditionOperator.In , ExpectedValue = "US,CA,UK,DE,FR,ES", IsActive = true },
                        new Condition { AttributeName = "subscription_age_days", Operator = ConditionOperator.GreaterThan , ExpectedValue = "90", IsActive = true },
                        new Condition { AttributeName = "plan_type", Operator = ConditionOperator.Equals , ExpectedValue = "enterprise", IsActive = true },
                        new Condition { AttributeName = "feature_access", Operator = ConditionOperator.Contains , ExpectedValue = "new-ui", IsActive = true }
                    }
                }
            }
        };

        var complexContext = new UserContext
        {
            UserId = "user-enterprise-001",
            Email = "enterprise@company.com",
            Tier = "premium",
            Country = "US",
            Region = "north-america"
        };
        complexContext.SetCustomAttribute("subscription_age_days", "180");
        complexContext.SetCustomAttribute("plan_type", "enterprise");
        complexContext.SetCustomAttribute("feature_access", "new-ui,analytics,dashboard");

        return _ruleEvaluationService.EvaluateAsync(complexFlag, complexContext).Result;
    }

    [BenchmarkCategory("Complex Scenarios")]
    [Benchmark]
    public bool ComplexRuleEvaluation_ORLogic()
    {
        var orFlag = new FeatureFlag
        {
            Key = "or-logic",
            DisplayName = "OR Logic",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new List<Rule>
            {
                new Rule
                {
                    Name = "Free Tier OR Beta",
                    Priority = 1,
                    ConditionLogic = "OR",
                    IsActive = true,
                    Conditions = new List<Condition>
                    {
                        new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals , ExpectedValue = "free", IsActive = true },
                        new Condition { AttributeName = "tags", Operator = ConditionOperator.Contains , ExpectedValue = "beta-tester", IsActive = true }
                    }
                }
            }
        };

        var orContext = new UserContext
        {
            UserId = "user-free-tier",
            Email = "free@example.com",
            Tier = "free"
        };

        return _ruleEvaluationService.EvaluateAsync(orFlag, orContext).Result;
    }

    [BenchmarkCategory("Cache Performance")]
    [Benchmark]
    public bool PercentageRollout_WithCache_Hit()
    {
        _cacheService.Set("new-dashboard:user12345", true.ToString());
        return _featureFlagService.IsEnabledAsync(_percentageFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Cache Performance")]
    [Benchmark]
    public bool PercentageRollout_WithCache_Miss()
    {
        return _featureFlagService.IsEnabledAsync(_percentageFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Cache Performance")]
    [Benchmark]
    public bool RuleBased_WithCache_Hit()
    {
        _cacheService.Set("premium-feature:user12345", true.ToString());
        return _featureFlagService.IsEnabledAsync(_ruleFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Cache Performance")]
    [Benchmark]
    public string ABTest_WithCache_Hit()
    {
        _cacheService.Set("checkout-redesign:user12345", "Control");
        return _featureFlagService.GetVariantAsync(_abTestFlag.Key, _userContext).Result!;
    }

    [BenchmarkCategory("Cache Performance")]
    [Benchmark]
    public bool FullEvaluation_WithCache_Miss()
    {
        return _featureFlagService.IsEnabledAsync(_percentageFlag.Key, _userContext).Result;
    }

    [BenchmarkCategory("Concurrent Access")]
    [Benchmark]
    public bool ConcurrentPercentageEvaluations()
    {
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 100; i++)
        {
            var user = new UserContext
            {
                UserId = $"user{i:D5}",
                Email = $"user{i}@example.com",
                Tier = "standard"
            };
            tasks.Add(_percentageRolloutService.EvaluateAsync(_percentageFlag, user));
        }
        return Task.WhenAll(tasks).Result.All(x => x == true);
    }

    [BenchmarkCategory("Concurrent Access")]
    [Benchmark]
    public bool ConcurrentRuleEvaluations()
    {
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 50; i++)
        {
            var user = new UserContext
            {
                UserId = $"user{i:D5}",
                Email = $"user{i}@example.com",
                Tier = "premium",
                Country = "US"
            };
            tasks.Add(_ruleEvaluationService.EvaluateAsync(_ruleFlag, user));
        }
        return Task.WhenAll(tasks).Result.All(x => x == true);
    }

    [BenchmarkCategory("Memory Allocations")]
    [Benchmark]
    public int GetConsistentHash_MultipleKeys()
    {
        int total = 0;
        for (int i = 0; i < 1000; i++)
        {
            total += _userContext.GetConsistentHash($"test-feature-{i}");
        }
        return total;
    }

    [BenchmarkCategory("Memory Allocations")]
    [Benchmark]
    public bool RuleEvaluation_ManyConditions()
    {
        var complexFlag = new FeatureFlag
        {
            Key = "complex-conditions",
            DisplayName = "Complex Conditions",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new List<Rule>
            {
                new Rule
                {
                    Name = "Complex Rule",
                    Priority = 1,
                    ConditionLogic = "AND",
                    IsActive = true,
                    Conditions = new List<Condition>
                    {
                        new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals , ExpectedValue = "premium", IsActive = true },
                        new Condition { AttributeName = "country", Operator = ConditionOperator.In , ExpectedValue = "US,CA,UK,DE,FR,ES" },
                        new Condition { AttributeName = "subscription_age_days", Operator = ConditionOperator.GreaterThan , ExpectedValue = "90" },
                        new Condition { AttributeName = "plan_type", Operator = ConditionOperator.Equals , ExpectedValue = "enterprise" },
                        new Condition { AttributeName = "feature_access", Operator = ConditionOperator.Contains , ExpectedValue = "new-ui" }
                    }
                }
            }
        };

        var complexContext = new UserContext
        {
            UserId = "user-enterprise-001",
            Email = "enterprise@company.com",
            Tier = "premium",
            Country = "US",
            Region = "north-america"
        };
        complexContext.SetCustomAttribute("subscription_age_days", "180");
        complexContext.SetCustomAttribute("plan_type", "enterprise");
        complexContext.SetCustomAttribute("feature_access", "new-ui,analytics,dashboard");

        return _ruleEvaluationService.EvaluateAsync(complexFlag, complexContext).Result;
    }

    [BenchmarkCategory("Configuration Changes")]
    [Benchmark]
    public bool PercentageFlag_ZeroToHundred()
    {
        var flag = new FeatureFlag
        {
            Key = "dynamic-percentage",
            DisplayName = "Dynamic Percentage",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 0,
            IsEnabled = true
        };

        // Evaluate at 0%
        var result1 = _percentageRolloutService.EvaluateAsync(flag, _userContext).Result;

        // Update to 100%
        flag.PercentageRollout = 100;

        // Evaluate at 100%
        var result2 = _percentageRolloutService.EvaluateAsync(flag, _userContext).Result;

        return result1 == false && result2 == true;
    }

    [BenchmarkCategory("Edge Cases")]
    [Benchmark]
    public bool PercentageRollout_EdgeCases()
    {
        bool result = true;

        // Test 0%
        var flag0 = new FeatureFlag { Key = "zero", PercentageRollout = 0, IsEnabled = true };
        result &= _percentageRolloutService.EvaluateAsync(flag0, _userContext).Result == false;

        // Test 1%
        var flag1 = new FeatureFlag { Key = "one", PercentageRollout = 1, IsEnabled = true };
        result &= _percentageRolloutService.EvaluateAsync(flag1, _userContext).Result == true;

        // Test 50%
        var flag50 = new FeatureFlag { Key = "fifty", PercentageRollout = 50, IsEnabled = true };
        int trueCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var user = new UserContext { UserId = $"user{i}" };
            if (_percentageRolloutService.EvaluateAsync(flag50, user).Result)
                trueCount++;
        }
        result &= Math.Abs(trueCount - 50) < 20; // Allow some variance

        // Test 100%
        var flag100 = new FeatureFlag { Key = "hundred", PercentageRollout = 100, IsEnabled = true };
        result &= _percentageRolloutService.EvaluateAsync(flag100, _userContext).Result == true;

        return result;
    }

    [BenchmarkCategory("Variant Distribution")]
    [Benchmark]
    public string? ABTest_VariantDistribution()
    {
        var flag = new FeatureFlag
        {
            Key = "distribution-test",
            DisplayName = "Distribution Test",
            RolloutType = RolloutType.ABTest,
            IsEnabled = true,
            Variants = new List<ABTestVariant>
            {
                new ABTestVariant { DisplayName = "A", AllocationPercentage = 10, VariantKey = "a" },
                new ABTestVariant { DisplayName = "B", AllocationPercentage = 20, VariantKey = "b" },
                new ABTestVariant { DisplayName = "C", AllocationPercentage = 30, VariantKey = "c" },
                new ABTestVariant { DisplayName = "D", AllocationPercentage = 40, VariantKey = "d" }
            }
        };

        var results = new Dictionary<string, int>();
        for (int i = 0; i < 1000; i++)
        {
            var user = new UserContext { UserId = $"user{i}" };
            var variant = _featureFlagService.GetVariantAsync(flag.Key, user).Result;
            if (variant != null)
                results[variant] = results.GetValueOrDefault(variant) + 1;
        }

        return $"A:{results.GetValueOrDefault("a")},B:{results.GetValueOrDefault("b")},C:{results.GetValueOrDefault("c")},D:{results.GetValueOrDefault("d")}";
    }
}

// Mock implementations for benchmarking
internal class MockCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(Get<T>(key));
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        _cache.AddOrUpdate(key, value!, (_, _) => value!);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        Set(key, value, ttl);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        Clear();
        return Task.CompletedTask;
    }

    Task ICacheService.RemoveAsync(string key) => RemoveAsync(key);
    Task ICacheService.ClearAsync() => ClearAsync();
}

internal class MockFeatureFlagRepository : IFeatureFlagRepository
{
    public Task<FeatureFlag?> GetByKeyAsync(string key) => Task.FromResult<FeatureFlag?>(null);
    public Task<FeatureFlag?> GetByIdAsync(int id) => Task.FromResult<FeatureFlag?>(null);
    public Task<IEnumerable<FeatureFlag>> GetAllAsync() => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<IEnumerable<FeatureFlag>> GetEnabledAsync() => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string createdBy) => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTime dateTime) => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<int> GetTotalCountAsync() => Task.FromResult(0);
    public Task<IEnumerable<FeatureFlag>> GetPagedAsync(int pageNumber, int pageSize) => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<bool> KeyExistsAsync(string key) => Task.FromResult(false);
    public Task<FeatureFlag> AddAsync(FeatureFlag entity) => Task.FromResult(new FeatureFlag());
    public Task UpdateAsync(FeatureFlag entity) => Task.CompletedTask;
    public Task DeleteAsync(int id) => Task.CompletedTask;
    public Task<bool> ExistsAsync(int id) => Task.FromResult(false);
    public Task SaveChangesAsync() => Task.CompletedTask;
    public Task<FeatureFlag?> GetWithRulesAsync(int id) => Task.FromResult<FeatureFlag?>(null);
    public Task<FeatureFlag?> GetWithVariantsAsync(int id) => Task.FromResult<FeatureFlag?>(null);
    public Task<FeatureFlag?> GetWithAuditLogsAsync(int featureFlagId) => Task.FromResult<FeatureFlag?>(null);
    public Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count) => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<IEnumerable<FeatureFlag>> SearchAsync(string term) => Task.FromResult(Enumerable.Empty<FeatureFlag>());
}

internal class MockAuditLogRepository : IAuditLogRepository
{
    public Task<AuditLog> AddAsync(AuditLog entity) => Task.FromResult(new AuditLog());
    public Task DeleteAsync(int id) => Task.CompletedTask;
    public Task<bool> ExistsAsync(int id) => Task.FromResult(false);
    public Task SaveChangesAsync() => Task.CompletedTask;
    public Task<IEnumerable<AuditLog>> GetAllAsync() => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<AuditLog?> GetByIdAsync(int id) => Task.FromResult<AuditLog?>(null);
    public Task<IEnumerable<AuditLog>> GetByFeatureFlagIdAsync(int featureFlagId) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<IEnumerable<AuditLog>> GetByChangedByAsync(string changedBy) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<IEnumerable<AuditLog>> GetSinceAsync(DateTime dateTime) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<IEnumerable<AuditLog>> GetByFeatureFlagIdPagedAsync(int featureFlagId, int pageNumber, int pageSize) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<int> GetCountByFeatureFlagIdAsync(int featureFlagId) => Task.FromResult(0);
    public Task<AuditLog?> GetLastChangeAsync(int featureFlagId) => Task.FromResult<AuditLog?>(null);
    public Task<IEnumerable<AuditLog>> GetChangesInRangeAsync(DateTime startDate, DateTime endDate) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task<IEnumerable<AuditLog>> GetByActionAsync(string action) => Task.FromResult(Enumerable.Empty<AuditLog>());
    public Task CleanupOldLogsAsync(int retentionDays) => Task.CompletedTask;
    public Task UpdateAsync(AuditLog entity) => Task.CompletedTask;
}

internal class MockFlagEvaluationLogService : IFlagEvaluationLogService
{
    public void Log(FlagEvaluationLog entry) { }
    public IReadOnlyList<FlagEvaluationLog> GetAll() => Array.Empty<FlagEvaluationLog>();
    public IReadOnlyList<FlagEvaluationLog> GetByUserId(string userId) => Array.Empty<FlagEvaluationLog>();
    public IReadOnlyList<FlagEvaluationLog> GetByFlagName(string flagName) => Array.Empty<FlagEvaluationLog>();
}
