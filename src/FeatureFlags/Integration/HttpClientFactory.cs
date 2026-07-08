#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    public HttpClient CreateWebhookClient()
    {
        var client = _factory.CreateClient("WebhookClient");
        client.Timeout = TimeSpan.FromSeconds(30);
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
        services.AddHttpClient("WebhookClient")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddHttpClient("ExternalApiClient")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

        return services;
    }
}

/// <summary>
/// Wrapper for typed HTTP requests with built-in error handling and retry logic.
/// </summary>
public sealed class HttpApiClient
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
    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
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
    public async Task<T?> PostAsync<T>(string url, object? body = null, CancellationToken cancellationToken = default)
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
    public async Task<T?> PutAsync<T>(string url, object? body = null, CancellationToken cancellationToken = default)
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
    public async Task<bool> DeleteAsync(string url, CancellationToken cancellationToken = default)
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
