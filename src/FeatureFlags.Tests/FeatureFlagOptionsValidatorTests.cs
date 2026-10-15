#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for FeatureFlagOptionsValidator and AddFeatureFlags
// =============================================================================

using FeatureFlags.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace FeatureFlags.Tests;

/// <summary>
/// Unit tests for <see cref="FeatureFlagOptionsValidator"/> and the <c>AddFeatureFlags</c> registration.
/// </summary>
public sealed class FeatureFlagOptionsValidatorTests
{
    private static readonly FeatureFlagOptionsValidator Validator = new();

    #region Validator

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        var result = Validator.Validate(null, new FeatureFlagOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ZeroCacheDurationMinutes_FailsNamingPropertyAndValue()
    {
        var options = new FeatureFlagOptions { CacheDurationMinutes = 0 };

        var result = Validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, f => f.Contains("CacheDurationMinutes") && f.Contains("'0'"));
    }

    [Fact]
    public void Validate_NegativeCacheDurationMinutes_Fails()
    {
        var options = new FeatureFlagOptions { CacheDurationMinutes = -3 };

        var result = Validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, f => f.Contains("CacheDurationMinutes") && f.Contains("'-3'"));
    }

    [Fact]
    public void Validate_AuditEnabledWithoutRetention_FailsNamingRetentionProperty()
    {
        var options = new FeatureFlagOptions
        {
            EnableAuditLogging = true,
            AuditLogRetentionDays = 0
        };

        var result = Validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, f => f.Contains("AuditLogRetentionDays") && f.Contains("'0'"));
    }

    [Fact]
    public void Validate_AuditDisabledWithoutRetention_Succeeds()
    {
        var options = new FeatureFlagOptions
        {
            EnableAuditLogging = false,
            AuditLogRetentionDays = 0
        };

        var result = Validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_DefaultRolloutPercentageOutOfRange_FailsNamingValue(int percentage)
    {
        var options = new FeatureFlagOptions { DefaultRolloutPercentage = percentage };

        var result = Validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, f => f.Contains("DefaultRolloutPercentage") && f.Contains($"'{percentage}'"));
    }

    [Fact]
    public void Validate_MultipleInvalidProperties_ReportsEachFailure()
    {
        var options = new FeatureFlagOptions
        {
            CacheDurationMinutes = 0,
            MaxRulesPerFlag = 0,
            MaxVariantsPerFlag = 0
        };

        var result = Validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Equal(3, result.Failures.Count());
    }

    [Fact]
    public void Validate_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Validator.Validate(null, null!));
    }

    #endregion

    #region AddFeatureFlags

    [Fact]
    public void AddFeatureFlags_InvalidOptions_ThrowsOptionsValidationExceptionOnResolve()
    {
        var services = new ServiceCollection();
        services.AddFeatureFlags(o => o.CacheDurationMinutes = 0);

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<FeatureFlagOptions>>().Value);
        Assert.Contains("CacheDurationMinutes", exception.Message);
        Assert.Contains("'0'", exception.Message);
    }

    [Fact]
    public void AddFeatureFlags_ValidOptions_ResolvesConfiguredValues()
    {
        var services = new ServiceCollection();
        services.AddFeatureFlags(o => o.CacheDurationMinutes = 15);

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<FeatureFlagOptions>>().Value;
        Assert.Equal(15, options.CacheDurationMinutes);
    }

    [Fact]
    public async Task AddFeatureFlags_InvalidOptions_FailsHostStartup()
    {
        using var host = new HostBuilder()
            .ConfigureServices(services => services.AddFeatureFlags(o => o.AuditLogRetentionDays = 0))
            .Build();

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
        Assert.Contains("AuditLogRetentionDays", exception.Message);
        Assert.Contains("'0'", exception.Message);
    }

    [Fact]
    public void AddFeatureFlags_NullServices_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddFeatureFlags(_ => { }));
    }

    [Fact]
    public void AddFeatureFlags_NullConfigure_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddFeatureFlags(null!));
    }

    #endregion
}
