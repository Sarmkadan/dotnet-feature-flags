#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Net;
using FeatureFlags.Events;
using FeatureFlags.Exceptions;

namespace FeatureFlags.Integration;

/// <summary>
/// Factory for creating and configuring HttpClient instances with consistent settings.
/// Provides typed HTTP clients for different integration scenarios with proper timeouts and retry policies.
/// </summary>
public interface IHttpClientFactory
{
    HttpClient CreateWebhookClient();
    HttpClient CreateExternalApiClient();
}

/// <summary>
/// Default implementation of HTTP client factory using standard HttpClient.
/// </summary>
public sealed class DefaultHttpClientFactory : IHttpClientFactory {
    private readonly System.Net.Http.IHttpClientFactory _factory;

    public DefaultHttpClientFactory(System.Net.Http.IHttpClientFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Creates an HttpClient configured for webhook delivery. Per-attempt timeout, retry and
    /// circuit breaker behavior are applied by <see cref="WebhookResilienceHandler"/> in the
    /// pipeline, so the client-level timeout is left unbounded to avoid double-timing-out
    /// a request that is still within its retry budget.
    /// </summary>
    public HttpClient CreateWebhookClient()
    {
        var client = _factory.CreateClient("WebhookClient");
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Add("User-Agent", "FeatureFlagEngine/1.0");
        return client;
    }

    public HttpClient CreateExternalApiClient()
    {
        var client = _factory.CreateClient("ExternalApiClient");
        client.Timeout = TimeSpan.FromSeconds(60);
        client.DefaultRequestHeaders.Add("User-Agent", "FeatureFlagEngine/1.0");
        return client;
    }
}

/// <summary>
/// Configuration for HTTP client factory setup during dependency injection.
/// </summary>
public static class HttpClientConfiguration
{
    public static IServiceCollection AddFeatureFlagHttpClients(this IServiceCollection services)
    {
        services.AddSingleton<WebhookCircuitBreakerRegistry>();
        services.AddSingleton(new WebhookResilienceOptions());
        services.AddTransient<WebhookResilienceHandler>();

        services.AddHttpClient("WebhookClient")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddHttpMessageHandler<WebhookResilienceHandler>();

        services.AddHttpClient("ExternalApiClient")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

        return services;
    }
}

/// <summary>
/// Tunable knobs for the webhook delivery resilience pipeline (timeout, retry, circuit breaker).
/// </summary>
public sealed class WebhookResilienceOptions
{
    /// <summary>
    /// Maximum time allowed for a single delivery attempt. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Number of retries performed after the initial attempt on transient failures. Defaults to 2.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 2;

    /// <summary>
    /// Base delay used for the exponential-with-jitter backoff between retries. Defaults to 200ms.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Number of consecutive failed attempts (post-retry) required to trip the circuit for a host. Defaults to 5.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// How long the circuit stays open for a host before a single probe request is allowed through. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Phase of a single host's circuit breaker.
/// </summary>
public enum WebhookCircuitPhase
{
    /// <summary>
    /// Requests flow normally; failures are being counted.
    /// </summary>
    Closed,

    /// <summary>
    /// Requests are rejected without being sent to the remote endpoint.
    /// </summary>
    Open,

    /// <summary>
    /// The open duration has elapsed; a single probe request is allowed to decide whether to close or reopen.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Mutable per-host circuit breaker state. Thread-safe via an internal lock.
/// </summary>
public sealed class WebhookHostCircuit
{
    private readonly object _syncLock = new();
    private int _consecutiveFailures;
    private DateTimeOffset _openedAt;
    private bool _probeInFlight;

    /// <summary>
    /// Current phase of the circuit.
    /// </summary>
    public WebhookCircuitPhase Phase { get; private set; } = WebhookCircuitPhase.Closed;

    /// <summary>
    /// Determines whether a request is currently allowed to proceed to the remote endpoint.
    /// Transitions <see cref="WebhookCircuitPhase.Open"/> to <see cref="WebhookCircuitPhase.HalfOpen"/>
    /// once the open duration has elapsed, admitting exactly one probe request.
    /// </summary>
    /// <param name="openDuration">Configured duration a trip stays open before probing.</param>
    /// <returns><see langword="true"/> if the request may proceed; otherwise <see langword="false"/>.</returns>
    public bool TryAcquire(TimeSpan openDuration)
    {
        lock (_syncLock)
        {
            switch (Phase)
            {
                case WebhookCircuitPhase.Closed:
                    return true;

                case WebhookCircuitPhase.Open when DateTimeOffset.UtcNow - _openedAt >= openDuration && !_probeInFlight:
                    Phase = WebhookCircuitPhase.HalfOpen;
                    _probeInFlight = true;
                    return true;

                case WebhookCircuitPhase.Open:
                    return false;

                case WebhookCircuitPhase.HalfOpen:
                    return false;

                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Records a successful delivery attempt, closing the circuit and resetting the failure count.
    /// </summary>
    public void RecordSuccess()
    {
        lock (_syncLock)
        {
            _consecutiveFailures = 0;
            _probeInFlight = false;
            Phase = WebhookCircuitPhase.Closed;
        }
    }

    /// <summary>
    /// Records a failed delivery attempt. Trips the circuit open once <paramref name="failureThreshold"/>
    /// consecutive failures have been observed, or immediately re-opens it if the failure occurred during
    /// a half-open probe.
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures required to trip the circuit.</param>
    /// <returns><see langword="true"/> if this failure just tripped the circuit open.</returns>
    public bool RecordFailure(int failureThreshold)
    {
        lock (_syncLock)
        {
            _probeInFlight = false;

            if (Phase == WebhookCircuitPhase.HalfOpen)
            {
                Phase = WebhookCircuitPhase.Open;
                _openedAt = DateTimeOffset.UtcNow;
                return true;
            }

            _consecutiveFailures++;
            if (_consecutiveFailures >= failureThreshold && Phase != WebhookCircuitPhase.Open)
            {
                Phase = WebhookCircuitPhase.Open;
                _openedAt = DateTimeOffset.UtcNow;
                return true;
            }

            return false;
        }
    }
}

/// <summary>
/// Registry of per-host circuit breaker state for webhook deliveries. Registered as a singleton so
/// state survives across the transient <see cref="WebhookResilienceHandler"/> instances created per
/// outgoing <see cref="HttpClient"/> handler.
/// </summary>
public sealed class WebhookCircuitBreakerRegistry
{
    private readonly ConcurrentDictionary<string, WebhookHostCircuit> _circuits = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets (creating if absent) the circuit breaker state for the given host.
    /// </summary>
    /// <param name="host">Destination host of the webhook endpoint.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="host"/> is null or empty.</exception>
    public WebhookHostCircuit GetCircuit(string host)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        return _circuits.GetOrAdd(host, static _ => new WebhookHostCircuit());
    }
}

/// <summary>
/// Thrown when a webhook delivery is rejected because the circuit breaker for the destination host is open.
/// </summary>
public sealed class WebhookCircuitOpenException : FeatureFlagException
{
    /// <summary>
    /// Initializes a new instance of <see cref="WebhookCircuitOpenException"/>.
    /// </summary>
    /// <param name="host">The host whose circuit is currently open.</param>
    public WebhookCircuitOpenException(string host)
        : base($"Circuit breaker is open for webhook host '{host}'; delivery skipped.", "WEBHOOK_CIRCUIT_OPEN")
    {
        Host = host;
    }

    /// <summary>
    /// The destination host whose circuit rejected the request.
    /// </summary>
    public string Host { get; }
}

/// <summary>
/// Delegating handler that applies a per-request timeout, jittered retry on 5xx/408 responses and
/// transient transport failures, and a per-host circuit breaker to outgoing webhook delivery requests.
/// When the circuit trips open, a <see cref="FeatureFlagEvent"/> is published on the event bus so the
/// dead-endpoint condition shows up in the audit trail instead of silently backing up dispatch.
/// </summary>
public sealed class WebhookResilienceHandler : DelegatingHandler
{
    private readonly WebhookCircuitBreakerRegistry _registry;
    private readonly WebhookResilienceOptions _options;
    private readonly IEventBus _eventBus;
    private readonly ILogger<WebhookResilienceHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WebhookResilienceHandler"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public WebhookResilienceHandler(
        WebhookCircuitBreakerRegistry registry,
        WebhookResilienceOptions options,
        IEventBus eventBus,
        ILogger<WebhookResilienceHandler> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="WebhookCircuitOpenException">Thrown when the destination host's circuit breaker is open.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var host = request.RequestUri?.Host ?? "unknown";
        var circuit = _registry.GetCircuit(host);

        if (!circuit.TryAcquire(_options.OpenDuration))
        {
            await PublishCircuitEventAsync("webhook.circuit_breaker.rejected", host, cancellationToken).ConfigureAwait(false);
            throw new WebhookCircuitOpenException(host);
        }

        Exception? lastFailure = null;
        var maxAttempts = _options.MaxRetryAttempts + 1;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var attemptRequest = attempt == 1 ? request : await CloneRequestAsync(request).ConfigureAwait(false);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.RequestTimeout);

            try
            {
                var response = await base.SendAsync(attemptRequest, timeoutCts.Token).ConfigureAwait(false);

                if (!IsTransientFailure(response.StatusCode))
                {
                    circuit.RecordSuccess();
                    return response;
                }

                lastFailure = new HttpRequestException($"Webhook host '{host}' responded with transient status {(int)response.StatusCode}");
                response.Dispose();
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastFailure = new TimeoutException($"Webhook request to '{host}' timed out after {_options.RequestTimeout.TotalSeconds}s", ex);
            }
            catch (HttpRequestException ex)
            {
                lastFailure = ex;
            }
            finally
            {
                if (attempt > 1)
                {
                    attemptRequest.Dispose();
                }
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(ComputeJitteredBackoff(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }
        }

        var tripped = circuit.RecordFailure(_options.FailureThreshold);
        if (tripped)
        {
            _logger.LogWarning("Circuit breaker tripped open for webhook host {Host} after repeated failures", host);
            await PublishCircuitEventAsync("webhook.circuit_breaker.opened", host, cancellationToken).ConfigureAwait(false);
        }

        throw lastFailure switch
        {
            TimeoutException timeout => timeout,
            HttpRequestException httpEx => httpEx,
            _ => new HttpRequestException($"Webhook request to '{host}' failed after {maxAttempts} attempt(s)")
        };
    }

    private static bool IsTransientFailure(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.RequestTimeout || (int)statusCode >= 500;

    private TimeSpan ComputeJitteredBackoff(int attempt)
    {
        var exponential = _options.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        var jitterMs = Random.Shared.NextDouble() * _options.RetryBaseDelay.TotalMilliseconds;
        return TimeSpan.FromMilliseconds(exponential + jitterMs);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version
        };

        if (original.Content is not null)
        {
            var buffer = await original.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var content = new ByteArrayContent(buffer);
            foreach (var header in original.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in original.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }

    private Task PublishCircuitEventAsync(string eventType, string host, CancellationToken cancellationToken) =>
        _eventBus.PublishAsync(
            eventType,
            featureFlagId: 0,
            featureFlagKey: string.Empty,
            triggeredBy: "webhook-resilience-handler",
            metadata: new Dictionary<string, object?> { ["host"] = host },
            cancellationToken: cancellationToken);
}

/// <summary>
/// Wrapper for typed HTTP requests with built-in error handling and retry logic.
/// </summary>
public class HttpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpApiClient> _logger;
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 100;

    public HttpApiClient(HttpClient httpClient, ILogger<HttpApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends GET request with automatic retries on transient failures.
    /// </summary>
    public virtual async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        return await SendWithRetryAsync(async () =>
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            return await HandleResponseAsync<T>(response, url);
        });
    }

    /// <summary>
    /// Sends POST request with JSON body and automatic retries.
    /// </summary>
    public virtual async Task<T?> PostAsync<T>(string url, object? body = null, CancellationToken cancellationToken = default)
    {
        return await SendWithRetryAsync(async () =>
        {
            var content = body is not null
                ? new StringContent(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json")
                : null;

            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            return await HandleResponseAsync<T>(response, url);
        });
    }

    /// <summary>
    /// Sends PUT request with JSON body.
    /// </summary>
    public virtual async Task<T?> PutAsync<T>(string url, object? body = null, CancellationToken cancellationToken = default)
    {
        return await SendWithRetryAsync(async () =>
        {
            var content = body is not null
                ? new StringContent(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json")
                : null;

            using var response = await _httpClient.PutAsync(url, content, cancellationToken);
            return await HandleResponseAsync<T>(response, url);
        });
    }

    /// <summary>
    /// Sends DELETE request.
    /// </summary>
    public virtual async Task<bool> DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE request failed: {Url}", url);
            return false;
        }
    }

    private async Task<T?> SendWithRetryAsync<T>(Func<Task<T?>> request)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return await request();
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries - 1)
            {
                var delayMs = InitialDelayMs * (int)Math.Pow(2, attempt);
                _logger.LogWarning(ex, "Request failed, retrying in {DelayMs}ms (attempt {Attempt}/{MaxRetries})", delayMs, attempt + 1, MaxRetries);
                await Task.Delay(delayMs);
            }
        }

        return default;
    }

    private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response, string url)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("HTTP request failed: {Url} - {StatusCode}", url, response.StatusCode);
            return default;
        }

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {Url}", url);
            return default;
        }
    }
}
