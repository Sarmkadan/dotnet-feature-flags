#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using FeatureFlags.Events;
using FeatureFlags.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Integration;

public sealed class WebhookResilienceHandlerTests
{
    private const string Url = "https://hooks.example.test/feature-flags";

    [Fact]
    public async Task SendAsync_RequestExceedsTimeout_ThrowsTimeoutException()
    {
        var inner = new StubHandler((_, ct) => HangUntilCancelled(ct));
        var options = new WebhookResilienceOptions { RequestTimeout = TimeSpan.FromMilliseconds(50), MaxRetryAttempts = 0 };
        using var invoker = CreateInvoker(inner, new Mock<IEventBus>(), options);

        var act = () => invoker.SendAsync(CreatePost(), CancellationToken.None);

        await act.Should().ThrowAsync<TimeoutException>();
        inner.Calls.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_TransientStatusesThenSuccess_RetriesUntilSuccessfulResponse()
    {
        var statuses = new Queue<HttpStatusCode>([HttpStatusCode.ServiceUnavailable, HttpStatusCode.RequestTimeout, HttpStatusCode.OK]);
        var inner = new StubHandler((_, _) => Respond(statuses.Dequeue()));
        using var invoker = CreateInvoker(inner, new Mock<IEventBus>());

        using var response = await invoker.SendAsync(CreatePost(), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.Calls.Should().Be(3);
    }

    [Fact]
    public async Task SendAsync_PersistentServerError_ExhaustsRetriesThenThrows()
    {
        var inner = new StubHandler((_, _) => Respond(HttpStatusCode.InternalServerError));
        using var invoker = CreateInvoker(inner, new Mock<IEventBus>());

        var act = () => invoker.SendAsync(CreatePost(), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
        inner.Calls.Should().Be(3);
    }

    [Fact]
    public async Task SendAsync_NonTransientClientError_IsNotRetried()
    {
        var inner = new StubHandler((_, _) => Respond(HttpStatusCode.NotFound));
        using var invoker = CreateInvoker(inner, new Mock<IEventBus>());

        using var response = await invoker.SendAsync(CreatePost(), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        inner.Calls.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_ThresholdReached_OpensCircuitAndRejectsWithoutSending()
    {
        var inner = new StubHandler((_, _) => Respond(HttpStatusCode.InternalServerError));
        var eventBus = new Mock<IEventBus>();
        var options = new WebhookResilienceOptions { FailureThreshold = 2, MaxRetryAttempts = 0, OpenDuration = TimeSpan.FromMinutes(1) };
        using var invoker = CreateInvoker(inner, eventBus, options);

        await FailAsync(invoker);
        await FailAsync(invoker);
        var act = () => invoker.SendAsync(CreatePost(), CancellationToken.None);

        await act.Should().ThrowAsync<WebhookCircuitOpenException>().Where(ex => ex.Host == "hooks.example.test");
        inner.Calls.Should().Be(2);
        VerifyPublished(eventBus, "webhook.circuit_breaker.opened", Times.Once());
        VerifyPublished(eventBus, "webhook.circuit_breaker.rejected", Times.Once());
    }

    [Fact]
    public async Task SendAsync_SuccessfulProbe_ClosesCircuitAndPublishesRecovery()
    {
        var statuses = new Queue<HttpStatusCode>([HttpStatusCode.InternalServerError, HttpStatusCode.OK, HttpStatusCode.OK]);
        var inner = new StubHandler((_, _) => Respond(statuses.Dequeue()));
        var eventBus = new Mock<IEventBus>();
        var options = new WebhookResilienceOptions { FailureThreshold = 1, MaxRetryAttempts = 0, OpenDuration = TimeSpan.Zero };
        using var invoker = CreateInvoker(inner, eventBus, options);

        await FailAsync(invoker);
        using var probe = await invoker.SendAsync(CreatePost(), CancellationToken.None);
        using var afterRecovery = await invoker.SendAsync(CreatePost(), CancellationToken.None);

        probe.StatusCode.Should().Be(HttpStatusCode.OK);
        afterRecovery.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.Calls.Should().Be(3);
        VerifyPublished(eventBus, "webhook.circuit_breaker.closed", Times.Once());
    }

    [Fact]
    public async Task SendAsync_ProbeCancelledByCaller_DoesNotLeaveCircuitStuckHalfOpen()
    {
        var inner = new StubHandler((index, _, ct) => index switch
        {
            0 => Respond(HttpStatusCode.InternalServerError),
            1 => HangUntilCancelled(ct),
            _ => Respond(HttpStatusCode.OK)
        });
        var options = new WebhookResilienceOptions { FailureThreshold = 1, MaxRetryAttempts = 0, OpenDuration = TimeSpan.Zero };
        using var invoker = CreateInvoker(inner, new Mock<IEventBus>(), options);
        await FailAsync(invoker);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
        var cancelled = () => invoker.SendAsync(CreatePost(), cts.Token);
        await cancelled.Should().ThrowAsync<OperationCanceledException>();

        using var response = await invoker.SendAsync(CreatePost(), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.Calls.Should().Be(3);
    }

    private static HttpMessageInvoker CreateInvoker(HttpMessageHandler inner, Mock<IEventBus> eventBus, WebhookResilienceOptions? options = null)
    {
        var resilienceOptions = options ?? new WebhookResilienceOptions { RetryBaseDelay = TimeSpan.FromMilliseconds(1) };
        var handler = new WebhookResilienceHandler(
            new WebhookCircuitBreakerRegistry(),
            resilienceOptions,
            eventBus.Object,
            NullLogger<WebhookResilienceHandler>.Instance)
        {
            InnerHandler = inner
        };

        return new HttpMessageInvoker(handler);
    }

    private static HttpRequestMessage CreatePost() =>
        new(HttpMethod.Post, Url) { Content = new StringContent("{}") };

    private static async Task FailAsync(HttpMessageInvoker invoker)
    {
        var act = () => invoker.SendAsync(CreatePost(), CancellationToken.None);
        await act.Should().ThrowAsync<Exception>();
    }

    private static void VerifyPublished(Mock<IEventBus> eventBus, string eventType, Times times) =>
        eventBus.Verify(
            bus => bus.PublishAsync(
                eventType,
                0,
                string.Empty,
                "webhook-resilience-handler",
                It.Is<Dictionary<string, object?>?>(metadata => metadata != null && metadata["host"]!.Equals("hooks.example.test")),
                It.IsAny<CancellationToken>()),
            times);

    private static Task<HttpResponseMessage> Respond(HttpStatusCode statusCode) =>
        Task.FromResult(new HttpResponseMessage(statusCode));

    private static async Task<HttpResponseMessage> HangUntilCancelled(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK);
    }

    /// <summary>
    /// Inner handler that answers each call from a delegate keyed by the zero-based call index, and counts calls.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;
        private int _calls;

        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
            : this((_, request, ct) => respond(request, ct))
        {
        }

        public StubHandler(Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            _respond = respond;
        }

        /// <summary>
        /// Number of requests that reached this stub.
        /// </summary>
        public int Calls => Volatile.Read(ref _calls);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var index = Interlocked.Increment(ref _calls) - 1;
            return _respond(index, request, cancellationToken);
        }
    }
}
