# IHttpClientFactory

The `IHttpClientFactory` interface and its associated `DefaultHttpClientFactory` implementation provide a centralized mechanism for managing the lifetime and configuration of `HttpClient` instances within the `dotnet-feature-flags` project. This ensures that HTTP clients are appropriately configured for distinct operational needs—specifically webhook communications and external API integrations—while mitigating common issues associated with manual `HttpClient` instantiation, such as socket exhaustion.

## API

### DefaultHttpClientFactory
`public sealed class DefaultHttpClientFactory : IHttpClientFactory`

The primary implementation of the `IHttpClientFactory` interface, responsible for creating and configuring `HttpClient` instances.

`public DefaultHttpClientFactory()`
Initializes a new instance of the `DefaultHttpClientFactory` class.

`public HttpClient CreateWebhookClient()`
Creates and returns an `HttpClient` configured specifically for sending webhook notifications.

`public HttpClient CreateExternalApiClient()`
Creates and returns an `HttpClient` configured for consuming external APIs required by the feature flag system.

### Service Registration
`public static IServiceCollection AddFeatureFlagHttpClients(this IServiceCollection services)`
Registers the necessary HTTP client dependencies into the `IServiceCollection`, enabling dependency injection for `IHttpClientFactory` and `HttpApiClient`.

### HttpApiClient
`public HttpApiClient`
A wrapper class designed to simplify common HTTP operations.

`public async Task<T?> GetAsync<T>(string requestUri)`
Performs an asynchronous HTTP GET request and deserializes the response content into the specified type `T`. Returns `null` if the request fails or content is empty.

`public async Task<T?> PostAsync<T>(string requestUri, object content)`
Performs an asynchronous HTTP POST request with the specified content, deserializing the response into type `T`. Returns `null` if the request fails.

`public async Task<T?> PutAsync<T>(string requestUri, object content)`
Performs an asynchronous HTTP PUT request with the specified content, deserializing the response into type `T`. Returns `null` if the request fails.

`public async Task<bool> DeleteAsync(string requestUri)`
Performs an asynchronous HTTP DELETE request. Returns `true` if the request is successful, otherwise `false`.

## Usage

### Registering Services
The following example demonstrates how to configure the HTTP client services during application startup in `Program.cs`.

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient factory services
builder.Services.AddFeatureFlagHttpClients();

var app = builder.Build();
```

### Consuming the Client
The following example demonstrates injecting and utilizing the `HttpApiClient` within a service.

```csharp
public class FeatureFlagService
{
    private readonly HttpApiClient _apiClient;

    public FeatureFlagService(HttpApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<FeatureFlagConfig?> GetConfigAsync(string flagId)
    {
        return await _apiClient.GetAsync<FeatureFlagConfig>($"/api/flags/{flagId}");
    }
}
```

## Notes

*   **Resource Management**: `DefaultHttpClientFactory` is designed to manage the underlying `HttpMessageHandler` lifetime. Consumers should not dispose of the `HttpClient` instances returned by `CreateWebhookClient` or `CreateExternalApiClient`.
*   **Thread Safety**: The `DefaultHttpClientFactory` and `HttpApiClient` instances are intended to be thread-safe when registered and consumed via dependency injection.
*   **Error Handling**: Methods in `HttpApiClient` (`GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`) handle underlying HTTP request exceptions internally, returning default values or `false` rather than throwing exceptions, unless specified otherwise by the underlying library implementation. Ensure appropriate null-checking when processing results from `GetAsync`, `PostAsync`, and `PutAsync`.
