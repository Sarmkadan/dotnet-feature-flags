// existing content ...

## IHttpClientFactory

The `IHttpClientFactory` interface provides methods for creating and configuring `HttpClient` instances with consistent settings for different integration scenarios. It allows for typed HTTP clients with proper timeouts and retry policies.

Example usage:
```csharp
var factory = new DefaultHttpClientFactory(
    new System.Net.Http.HttpClientFactory()
);

var webhookClient = factory.CreateWebhookClient();
var externalApiClient = factory.CreateExternalApiClient();

var httpApiClient = new HttpApiClient(webhookClient, new NullLogger<HttpApiClient>());
var result = await httpApiClient.GetAsync<string>("https://example.com/api/data");
```
