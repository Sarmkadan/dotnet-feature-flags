# CacheServiceTests
The `CacheServiceTests` class is a test suite designed to validate the functionality of a cache service, ensuring it correctly stores, retrieves, and manages cached data. This class contains a comprehensive set of test methods that cover various scenarios, including setting and getting cache values, handling non-existent keys, removing cache entries, and testing custom time-to-live (TTL) settings.

## API
* `public CacheServiceTests()`: The constructor for the `CacheServiceTests` class, initializing the test suite.
* `public void Set_And_Get_ReturnsValue()`: Tests that setting a value and then getting it returns the expected value.
* `public void Get_NonExistentKey_ReturnsNull()`: Verifies that attempting to retrieve a non-existent key returns null.
* `public void Remove_DeletesValue()`: Confirms that removing a key deletes its associated value from the cache.
* `public void Set_WithCustomTtl_ExpiresAfterTimeout()`: Tests that a cache entry with a custom TTL expires after the specified timeout.
* `public void Set_ComplexObject_StoresAndRetrieves()`: Validates that complex objects can be stored and retrieved correctly.
* `public async Task SetAsync_And_GetAsync_Works()`: Asynchronously sets and then gets a value, ensuring the async operations work as expected.
* `public async Task Clear_RemovesAllEntries()`: Tests that clearing the cache removes all entries.
* `public void Set_NullKey_DoesNotThrow()`: Verifies that setting a value with a null key does not throw an exception.
* `public void Set_OverwritesExistingKey()`: Confirms that setting a value overwrites any existing value for the same key.
* `public void ValidateAndNormalizePaging_OutOfRangePage_NormalizesCorrectly()`: Tests that out-of-range page values are normalized correctly.
* `public void ValidateAndNormalizePaging_MaxPageSize_ClampedCorrectly()`: Verifies that the maximum page size is clamped correctly.
* `public void CalculateOffset_ReturnsCorrectValue()`: Validates that the offset calculation returns the correct value.
* `public void CreateMetadata_CalculatesCorrectly()`: Tests that metadata creation calculates correctly.

## Usage
The following examples demonstrate how to use the `CacheServiceTests` class in a real-world scenario:
```csharp
// Example 1: Basic cache operations
var cacheService = new CacheService();
cacheService.Set("key", "value");
var cachedValue = cacheService.Get("key");
Assert.AreEqual("value", cachedValue);

// Example 2: Asynchronous cache operations
var cacheServiceAsync = new CacheService();
await cacheServiceAsync.SetAsync("asyncKey", "asyncValue");
var asyncCachedValue = await cacheServiceAsync.GetAsync("asyncKey");
Assert.AreEqual("asyncValue", asyncCachedValue);
```

## Notes
When using the `CacheServiceTests` class, consider the following edge cases and thread-safety remarks:
- The cache service is designed to be thread-safe, allowing concurrent access and modifications.
- Custom TTL settings can significantly impact cache behavior, so test thoroughly with different TTL values.
- Null keys are intentionally allowed to avoid throwing exceptions, but this may lead to unexpected behavior if not handled properly.
- The `ValidateAndNormalizePaging` methods are crucial for ensuring correct pagination, especially when dealing with large datasets or custom page sizes.
- Asynchronous operations, like `SetAsync` and `GetAsync`, should be used when performance and responsiveness are critical, but be aware of the potential for concurrent modifications and race conditions.
