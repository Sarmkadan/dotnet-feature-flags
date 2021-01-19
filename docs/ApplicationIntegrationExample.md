# ApplicationIntegrationExample

Demonstrates feature-flag-driven integration patterns for swapping payment gateways and recommendation engines at runtime, exposing observable execution state through public properties for verification and testing scenarios.

## API

### `public ApplicationIntegrationExample()`
Initializes a new instance with default property values. All string properties are initialized to `null`, numeric properties to zero, and `Success` to `false`.

### `public async Task RunAsync()`
Executes the integration workflow: selects a payment gateway and recommendation engine based on feature flags, processes a payment, retrieves recommendations, and updates the public result properties. Returns when the full sequence completes.

**Throws:**  
- `InvalidOperationException` if a required feature flag is not configured.  
- `HttpRequestException` propagated from gateway or engine HTTP calls.  
- `TaskCanceledException` if the operation is cancelled via a `CancellationToken` (not exposed in signature but may flow from dependencies).

### `public string Id { get; set; }`
Correlation identifier for the integration run. Set before or during `RunAsync`; read after completion for logging or tracing.

### `public string UserId { get; set; }`
Identifier of the user on whose behalf the integration executes. Used by payment and recommendation services.

### `public decimal Total { get; set; }`
Monetary amount for the payment request. Must be non-negative; behavior is undefined if negative.

### `public int Items { get; set; }`
Number of items in the transaction. Influences recommendation engine input.

### `public string TransactionId { get; set; }`
Identifier returned by the payment gateway after a successful charge. `null` if payment fails or has not yet executed.

### `public bool Success { get; set; }`
Indicates whether the full integration workflow completed without error. `true` only when payment succeeds and recommendations are retrieved.

### `public sealed class LegacyPaymentGateway`
Legacy payment provider implementation retained for gradual migration.

#### `public Task<PaymentResult> ProcessPaymentAsync(string userId, decimal amount)`
Submits a charge to the legacy gateway.

**Parameters:**  
- `userId`: Customer identifier.  
- `amount`: Charge amount in the gateway's base currency.

**Returns:** `PaymentResult` containing `TransactionId` and success flag.

**Throws:** `HttpRequestException` on network or HTTP errors; `ArgumentException` if `userId` is null or empty or `amount` is negative.

### `public sealed class NewPaymentGateway`
Modern payment provider implementation targeted for full rollout.

#### `public Task<PaymentResult> ProcessPaymentAsync(string userId, decimal amount)`
Submits a charge to the new gateway.

**Parameters:**  
- `userId`: Customer identifier.  
- `amount`: Charge amount in the gateway's base currency.

**Returns:** `PaymentResult` containing `TransactionId` and success flag.

**Throws:** `HttpRequestException` on network or HTTP errors; `ArgumentException` if `userId` is null or empty or `amount` is negative.

### `public sealed class RuleBasedRecommendationEngine`
Deterministic recommendation provider using static business rules.

#### `public Task<string[]> GetRecommendationsAsync(string userId, int itemCount)`
Retrieves product identifiers based on rule evaluation.

**Parameters:**  
- `userId`: Customer identifier for personalization context.  
- `itemCount`: Number of items in the current transaction, used as a rule input.

**Returns:** Array of product SKU strings; empty array if no matches.

**Throws:** `HttpRequestException` on network or HTTP errors; `ArgumentException` if `userId` is null or empty.

### `public sealed class MLRecommendationEngine`
Machine-learning-based recommendation provider.

#### `public Task<string[]> GetRecommendationsAsync(string userId, int itemCount)`
Retrieves product identifiers from a trained model endpoint.

**Parameters:**  
- `userId`: Customer identifier for inference input.  
- `itemCount`: Number of items in the current transaction, used as a feature.

**Returns:** Array of product SKU strings ranked by predicted relevance; empty array if the model returns no results.

**Throws:** `HttpRequestException` on network or HTTP errors; `ArgumentException` if `userId` is null or empty; `TimeoutException` if the model service exceeds its SLA.

## Usage

### Example 1: Basic execution with property inspection
```csharp
var example = new ApplicationIntegrationExample
{
    Id = "run-2024-001",
    UserId = "user-42",
    Total = 149.99m,
    Items = 3
};

await example.RunAsync();

if (example.Success)
{
    Console.WriteLine($"Transaction {example.TransactionId} completed for user {example.UserId}.");
}
else
{
    Console.WriteLine($"Integration run {example.Id} failed.");
}
```

### Example 2: Direct gateway and engine usage for testing
```csharp
var legacy = new ApplicationIntegrationExample.LegacyPaymentGateway();
var mlEngine = new ApplicationIntegrationExample.MLRecommendationEngine();

var payment = await legacy.ProcessPaymentAsync("user-99", 75.50m);
if (payment.Success)
{
    var recs = await mlEngine.GetRecommendationsAsync("user-99", 1);
    foreach (var sku in recs)
    {
        Console.WriteLine($"Recommended: {sku}");
    }
}
```

## Notes

- **Thread safety:** The `ApplicationIntegrationExample` instance is not thread-safe. Concurrent calls to `RunAsync` or simultaneous mutation of properties from multiple threads will result in race conditions on `TransactionId`, `Success`, and other fields. Each logical workflow should use a dedicated instance.
- **Idempotency:** `RunAsync` does not implement idempotency keys. Retrying a failed run by calling `RunAsync` again may cause duplicate charges if the payment gateway does not deduplicate based on `UserId` and `Total`.
- **Feature flag coupling:** The internal flag evaluation logic is not exposed. Changing flag state between construction and `RunAsync` may produce inconsistent gateway/engine selection. Treat flag configuration as immutable for the lifetime of the instance.
- **Nullability:** `TransactionId` remains `null` until a payment succeeds. Accessing it before `Success` is `true` yields `null`; callers must guard accordingly.
- **Exception propagation:** Exceptions from `ProcessPaymentAsync` and `GetRecommendationsAsync` bubble out of `RunAsync` unwrapped. Callers should catch `HttpRequestException`, `TimeoutException`, and `ArgumentException` to distinguish transient from configuration errors.
- **Resource disposal:** The gateway and engine classes do not implement `IDisposable`. Any underlying `HttpClient` or connection pooling is managed internally or by the DI container; no explicit cleanup is required by consumers.
