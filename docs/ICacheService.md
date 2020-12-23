# ICacheService

Provides an abstraction for caching values with synchronous and asynchronous operations, supporting both in‑memory and distributed storage scenarios.

## API

### Constructors
- **`InMemoryCacheService()`**  
  Creates a new instance of the in‑memory cache service.  
  *Parameters*: none.  
  *Return*: a ready‑to‑use `ICacheService` implementation.  
  *Throws*: none.

- **`DistributedCacheService()`**  
  Creates a new instance of the distributed cache service.  
  *Parameters*: none.  
  *Return*: a ready‑to‑use `ICacheService` implementation that delegates to an underlying distributed cache.  
  *Throws*: depends on the concrete distributed cache provider; may throw if the provider cannot be initialized.

### Synchronous retrieval
- **`T? Get<T>(string key)`**  
  Attempts to retrieve a value of type `T` associated with `key`.  
  *Parameters*:  
    - `key`: The identifier of the cached item. Must not be `null`.  
  *Return*: The cached value if present and not expired; otherwise `default(T)`.  
  *Throws*:  
    - `ArgumentNullException` if `key` is `null`.  
    - `InvalidOperationException` if the underlying cache throws during retrieval.

### Asynchronous retrieval
- **`Task<T?> GetAsync<T>(string key)`**  
  Asynchronously attempts to retrieve a value of type `T` associated with `key`.  
  *Parameters*:  
    - `key`: The identifier of the cached item. Must not be `null`.  
  *Return*: A task that resolves to the cached value if present and not expired; otherwise `default(T)`.  
  *Throws*:  
    - `ArgumentNullException` if `key` is `null`.  
    - Any exception thrown by the underlying cache provider is propagated through the returned task.

### Synchronous storage
- **`void Set<T>(string key, T value, TimeSpan? slidingExpiration = null, DateTimeOffset? absoluteExpiration = null)`**  
  Stores `value` in the cache under `key`.  
  *Parameters*:  
    - `key`: Identifier for the item. Must not be `null`.  
    - `value`: The object to cache; may be `null`.  
    - `slidingExpiration`: Optional sliding expiration; if both this and `absoluteExpiration` are `null`, the item uses the cache’s default policy.  
    - `absoluteExpiration`: Optional absolute expiration; if both this and `slidingExpiration` are `null`, the item uses the cache’s default policy.  
  *Return*: none.  
  *Throws*:  
    - `ArgumentNullException` if `key` is `null`.  
    - `ArgumentOutOfRangeException` if a supplied expiration is negative.  
    - Any exception thrown by the underlying cache provider.

### Asynchronous storage
- **`Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, DateTimeOffset? absoluteExpiration = null)`**  
  Asynchronously stores `value` in the cache under `key`.  
  *Parameters*: same as `Set<T>`.  
  *Return*: A task that completes when the operation finishes.  
  *Throws*:  
    - `ArgumentNullException` if `key` is `null`.  
    - `ArgumentOutOfRangeException` if a supplied expiration is negative.  
    - Any exception thrown by the underlying cache provider is propagated through the returned task.

### Synchronous removal
- **`void Remove(string key)`**  
  Removes the item identified by `key` from the cache, if it exists.  
  *Parameters*:  
    - `key`: Identifier of the item to remove. Must not be `null`.  
  *Return*: none.  
  *Throws*:  
    - `ArgumentNullException` if `key` is `null`.  
    - Any exception thrown by the underlying cache provider.

### Asynchronous removal
- **`Task RemoveAsync(string key)`**  
  Asynchronously removes the item identified by `key` from the cache, if it exists.  
  *Parameters*:  
    - `key`: Identifier of the item to remove. Must not be `null`.  
  *Return*: A task that completes when the operation finishes.  
  *Throws*:  
    - `ArgumentNullException` if `key` is `null`.  
    - Any exception thrown by the underlying cache provider is propagated through the returned task.

### Synchronous clear
- **`void Clear()`**  
  Removes all items from the cache.  
  *Parameters*: none.  
  *Return*: none.  
  *Throws*:  
    - Any exception thrown by the underlying cache provider.

### Asynchronous clear
- **`Task ClearAsync()`**  
  Asynchronously removes all items from the cache.  
  *Parameters*: none.  
  *Return*: A task that completes when the operation finishes.  
  *Throws*:  
    - Any exception thrown by the underlying cache provider is propagated through the returned task.

### Disposal
- **`void Dispose()`**  
  Releases any resources held by the cache service.  
  *Parameters*: none.  
  *Return*: none.  
  *Throws*: none.  
  *Remarks*: After calling `Dispose`, all other members may throw `ObjectDisposedException`.

### Properties (exposed on cache entries)
- **`object? Value`**  
  Gets the cached object associated with the most recent successful `Get`/`GetAsync` operation for a given key, or `null` if no value is present.  
  *Return*: The cached value or `null`.  
  *Throws*: none.

- **`DateTime? ExpiresAt`**  
  Gets the absolute expiration timestamp of the cached entry, if an expiration was set; otherwise `null`.  
  *Return*: The expiration date/time or `null`.  
  *Throws*: none.

- **`DateTime CreatedAt`**  
  Gets the timestamp when the cached entry was inserted into the cache.  
  *Return*: The creation date/time.  
  *Throws*: none.

## Usage

### Basic in‑memory caching
```csharp
using System;
using System.Threading.Tasks;
using DotNetFeatureFlags.Caching; // hypothetical namespace

class Example
{
    static async Task Main()
    {
        var cache = new InMemoryCacheService();

        // Store a value with a sliding expiration of 5 minutes
        cache.Set("greeting", "Hello, World!", TimeSpan.FromMinutes(5));

        // Retrieve the value synchronously
        string? msg = cache.Get<string>("greeting");
        Console.WriteLine(msg ?? "Not found");

        // Retrieve the value asynchronously
        string? asyncMsg = await cache.GetAsync<string>("greeting");
        Console.WriteLine(asyncMsg ?? "Not found");

        // Clean up
        cache.Dispose();
    }
}
```

### Using a distributed cache
```csharp
using System;
using System.Threading.Tasks;
using DotNetFeatureFlags.Caching;

class DistributedExample
{
    static async Task Main()
    {
        var cache = new DistributedCacheService();

        // Store a complex object
        var user = new User { Id = 42, Name = "Ada" };
        await cache.SetAsync("user:42", user, TimeSpan.FromHours(1));

        // Retrieve it later
        User? fetched = await cache.GetAsync<User>("user:42");
        if (fetched != null)
        {
            Console.WriteLine($"User {fetched.Name} loaded from cache.");
        }

        // Remove when no longer needed
        await cache.RemoveAsync("user:42");

        cache.Dispose();
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

## Notes
- The `InMemoryCacheService` implementation is thread‑safe for concurrent reads and writes; internal locking ensures consistent state without exposing locks to callers.  
- The `DistributedCacheService` forwards all operations to an underlying distributed cache (e.g., Redis, SQL Server). Thread‑safety therefore depends on the provider; most mainstream providers are safe for concurrent use, but consult the specific provider’s documentation.  
- Expiration semantics: if both `slidingExpiration` and `absoluteExpiration` are `null`, the cache applies its default policy (which may be infinite or a configured sliding window). Supplying both parameters results in the earlier of the two expiring the entry.  
- The `Value`, `ExpiresAt`, and `CreatedAt` properties reflect the state of the entry most recently accessed via a successful `Get`/`GetAsync` call; they are undefined after a `Remove`, `Clear`, or after the entry has expired.  
- Calling any member after `Dispose` results in an `ObjectDisposedException`.  
- Null keys are not permitted; all methods that accept a `key` argument will throw `ArgumentNullException` if `key` is `null`.  
- The generic type `T` can be any reference or nullable value type; the cache stores the object reference directly, so care should be taken with mutable structs.  
- Asynchronous methods do not guarantee a different threading model; they simply delegate to the provider’s async API when available, otherwise they execute synchronously and return a completed task.
