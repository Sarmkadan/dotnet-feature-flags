#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.Json;
using FeatureFlags.HealthChecks;
using FeatureFlags.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Integration;

public sealed class FeatureStoreHealthCheckTests
{
    [Fact]
    public async Task Health_ReturnsHealthy_WhenStoreReachableAndRefreshedRecently()
    {
        using var factory = new FeatureStoreHealthFactory();
        factory.Repository.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(3);
        factory.Services.GetRequiredService<FeatureStoreRefreshTracker>().MarkRefreshed();

        using var response = await factory.CreateClient().GetAsync("/health");
        var body = await ReadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("status").GetString().Should().Be("Healthy");
        var entry = body.GetProperty("entries").GetProperty("feature-store");
        entry.GetProperty("status").GetString().Should().Be("Healthy");
        entry.GetProperty("data").GetProperty(FeatureStoreHealthCheck.FlagCountKey).GetInt32().Should().Be(3);
        entry.GetProperty("data").GetProperty(FeatureStoreHealthCheck.LastRefreshUtcKey).GetString().Should().NotBe("never");
    }

    [Fact]
    public async Task Health_ReturnsDegraded_WhenLastRefreshIsOlderThanThreshold()
    {
        using var factory = new FeatureStoreHealthFactory();
        factory.Repository.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(3);
        factory.Services.GetRequiredService<FeatureStoreRefreshTracker>().MarkRefreshed();
        factory.Clock.Advance(TimeSpan.FromHours(2));

        using var response = await factory.CreateClient().GetAsync("/health");
        var body = await ReadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("status").GetString().Should().Be("Degraded");
        body.GetProperty("entries").GetProperty("feature-store").GetProperty("description").GetString()
            .Should().Contain("exceeding");
    }

    [Fact]
    public async Task Health_ReturnsDegraded_WhenStoreHasNoFlags()
    {
        using var factory = new FeatureStoreHealthFactory();
        factory.Repository.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(0);
        factory.Services.GetRequiredService<FeatureStoreRefreshTracker>().MarkRefreshed();

        using var response = await factory.CreateClient().GetAsync("/health");
        var body = await ReadAsync(response);

        body.GetProperty("status").GetString().Should().Be("Degraded");
        body.GetProperty("entries").GetProperty("feature-store").GetProperty("data")
            .GetProperty(FeatureStoreHealthCheck.FlagCountKey).GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Health_ReturnsUnhealthy_WhenStoreThrows()
    {
        using var factory = new FeatureStoreHealthFactory();
        factory.Repository.Setup(r => r.GetTotalCountAsync()).ThrowsAsync(new InvalidOperationException("store down"));

        using var response = await factory.CreateClient().GetAsync("/health");
        var body = await ReadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        body.GetProperty("status").GetString().Should().Be("Unhealthy");
        var entry = body.GetProperty("entries").GetProperty("feature-store");
        entry.GetProperty("description").GetString().Should().Be("Feature store is unreachable.");
        entry.GetProperty("data").GetProperty(FeatureStoreHealthCheck.FlagCountKey).GetString().Should().Be("unknown");
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    /// <summary>
    /// Hosts the real API with a mocked flag repository (the store boundary) and a controllable clock.
    /// </summary>
    private sealed class FeatureStoreHealthFactory : WebApplicationFactory<Program>
    {
        public Mock<IFeatureFlagRepository> Repository { get; } = new();

        public ManualClock Clock { get; } = new(new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=unused;Database=unused"
            }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFeatureFlagRepository>();
                services.AddScoped(_ => Repository.Object);

                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Clock);
            });
        }
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now;

        public ManualClock(DateTimeOffset start)
        {
            _now = start;
        }

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now = _now.Add(by);
    }
}
