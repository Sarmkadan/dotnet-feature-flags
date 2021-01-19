# FeatureFlagMiddleware

FeatureFlagMiddleware is an ASP.NET Core middleware component that evaluates feature flags for each incoming request and conditionally invokes the next pipeline delegate based on flag state.

## API

### FeatureFlagMiddleware()
**Purpose:** Initializes a new instance of the FeatureFlagMiddleware.  
**Parameters:** As defined by the implementation (typically includes the next request delegate and any required feature‑flag services).  
**Return:** A new FeatureFlagMiddleware instance.  
**Throws:** May throw `ArgumentNullException` if any required argument is `null`.

### InvokeAsync()
**Purpose:** Processes an HTTP request, evaluates feature flags, and either short‑circuits the pipeline or calls the next middleware.  
**Parameters:** As defined by the implementation (normally an `HttpContext`).  
**Return:** A `Task` that completes when the request has been processed.  
**Throws:** May propagate exceptions from downstream middleware or throw `InvalidOperationException` if the middleware is used outside of a valid HTTP context.

### FeatureFlagEnabledControllerExample
**Purpose:** Represents a nested type (typically a controller or example class) that demonstrates how to use feature flags within an MVC controller.  
**Parameters:** N/A (type declaration).  
**Return:** N/A.  
**Throws:** N/A.

### HandleRequestAsync()
**Purpose:** Asynchronously handles a request within the `FeatureFlagEnabledControllerExample` context, applying feature‑flag logic before executing controller actions.  
**Parameters:** As defined by the implementation.  
**Return:** A `Task` representing the asynchronous operation.  
**Throws:** May throw exceptions arising from flag evaluation or downstream processing.

### ConfigureFeatureFlags()
**Purpose:** Static method that registers feature‑flag services with the application’s dependency‑injection container.  
**Parameters:** As defined by the implementation (usually an `IServiceCollection`).  
**Return:** `void`.  
**Throws:** May throw `ArgumentNullException` if the service collection is `null`.

### UseFeatureFlags()
**Purpose:** Static extension method that inserts the feature‑flag middleware into the ASP.NET Core request pipeline.  
**Parameters:** As defined by the implementation (typically an `IApplicationBuilder`).  
**Return:** `void`.  
**Throws:** May throw `ArgumentNullException` if the builder is `null`.

### FeatureFlagCachingMiddleware
**Purpose:** Represents a nested middleware type that adds caching behavior to feature‑flag evaluations.  
**Parameters:** N/A (type declaration).  
**Return:** N/A.  
**Throws:** N/A.

### InvokeAsync() (FeatureFlagCachingMiddleware)
**Purpose:** Processes a request, consulting an internal cache for flag values before invoking the next middleware.  
**Parameters:** As defined by the implementation.  
**Return:** A `Task` that completes when the request has been processed.  
**Throws:** May throw exceptions from the underlying cache or flag provider.

### FeatureFlagRateLimitMiddleware
**Purpose:** Represents a nested middleware type that applies rate‑limiting to feature‑flag evaluations.  
**Parameters:** N/A (type declaration).  
**Return:** N/A.  
**Throws:** N/A.

### InvokeAsync() (FeatureFlagRateLimitMiddleware)
**Purpose:** Processes a request, enforcing rate limits on flag look‑ups before delegating to the next middleware.  
**Parameters:** As defined by the implementation.  
**Return:** A `Task` that completes when the request has been processed.  
**Throws:** May throw exceptions from the rate‑limiting store or flag provider.

### ProductService
**Purpose:** Represents a nested service type that provides product data, potentially influenced by feature flags.  
**Parameters:** N/A (type declaration).  
**Return:** N/A.  
**Throws:** N/A.

### GetProductAsync()
**Purpose:** Asynchronously retrieves a product by its identifier, applying any relevant feature‑flag logic.  
**Parameters:** As defined by the implementation (commonly a product ID).  
**Return:** A `Task<Product>` that yields the requested product.  
**Throws:** May throw `ArgumentException` for invalid identifiers or `NotFoundException` if the product does not exist.

### Id
**Purpose:** Gets the unique identifier of a product.  
**Parameters:** N/A.  
**Return:** An `int` representing the product ID.  
**Throws:** N/A.

### Name
**Purpose:** Gets the display name of a product.  
**Parameters:** N/A.  
**Return:** A `string` containing the product name.  
**Throws:** N/A.

### BasePrice
**Purpose:** Gets the base price of a product before any adjustments.  
**Parameters:** N/A.  
**Return:** A `decimal` representing the base price.  
**Throws:** N/A.

### Price
**Purpose:** Gets the final price of a product after applying feature‑flag‑based adjustments.  
**Parameters:** N/A.  
**Return:** A `decimal` representing the adjusted price.  
**Throws:** N/A.

### Recommendations
**Purpose:** Gets an array of recommended product identifiers associated with the product.  
**Parameters:** N/A.  
**Return:** A `string[]` containing recommendation IDs.  
**Throws:** N/A.

### UseFeatureFlagRouting()
**Purpose:** Static extension method that adds routing‑based feature‑flag middleware to the application pipeline.  
**Parameters:** As defined by the implementation (typically an `IApplicationBuilder`).  
**Return:** The original `IApplicationBuilder` instance to allow chaining.  
**Throws:** May throw `ArgumentNullException` if the builder is `null`.

### UseFeatureFlagCaching()
**Purpose:** Static extension method that inserts the caching feature‑flag middleware into the pipeline.  
**Parameters:** As defined by the implementation (typically an `IApplicationBuilder`).  
**Return:** The original `IApplicationBuilder` instance to allow chaining.  
**Throws:** May throw `ArgumentNullException` if the builder is `null`.

### UseFeatureFlagRateLimiting()
**Purpose:** Static extension method that inserts the rate‑limiting feature‑flag middleware into the pipeline.  
**Parameters:** As defined by the implementation (typically an `IApplicationBuilder`).  
**Return:** The original `IApplicationBuilder` instance to allow chaining.  
**Throws:** May throw `ArgumentNullException` if the builder is `null`.

## Usage

### Registering and using the middleware in an ASP.NET Core application

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register feature‑flag services
builder.Services.ConfigureFeatureFlags();

var app = builder.Build();

// Add the core feature‑flag middleware
app.UseFeatureFlags();

// Optional: add caching and rate‑limiting wrappers
app.UseFeatureFlagCaching();
app.UseFeatureFlagRateLimiting();

app.MapGet("/", async context =>
{
    // Example endpoint that relies on feature flags
    await context.Response.WriteAsync("Feature flag middleware is active.");
});

app.Run();
```

### Consuming the ProductService within a controller that respects feature flags

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
            return NotFound();

        return Ok(new
        {
            product.Id,
            product.Name,
            product.BasePrice,
            product.Price,
            Recommendations = product.Recommendations
        });
    }
}
```

## Notes

- The middleware classes are stateless after construction; multiple concurrent requests can safely share the same instance.
- Static extension methods (`UseFeatureFlags`, `UseFeatureFlagRouting`, `UseFeatureFlagCaching`, `UseFeatureFlagRateLimiting`) are thread‑safe and may be called only once during application startup.
- `FeatureFlagCachingMiddleware` maintains an internal cache that is accessed concurrently; the implementation should provide thread‑safe read/write operations (e.g., using `ConcurrentDictionary` or `MemoryCache` with appropriate locking).
- `FeatureFlagRateLimitMiddleware` typically relies on an external store (such as Redis or an in‑memory counter) to track request counts; misuse of a non‑thread‑safe store could lead to inaccurate rate‑limiting behavior.
- The `ProductService` methods do not retain state across calls; however, any dependencies injected into the service (e.g., a database context) must themselves be thread‑safe or scoped appropriately.
- If any constructor or method receives a `null` argument where a service or delegate is expected, the implementation is expected to throw an `ArgumentNullException`; callers should validate inputs before invoking these members.
