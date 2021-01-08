# UserContextTests

`UserContextTests` is the test suite for the `UserContext` type within the `dotnet-feature-flags` project. It validates the core behaviors of user context objects—ensuring that required fields are enforced, custom attributes can be stored and retrieved, consistent hashing produces stable and well-distributed values, and attribute lookups respect case-insensitivity. The class exists solely to confirm that `UserContext` meets its specification and handles edge cases correctly.

## API

All members are public test methods following the NUnit/xUnit convention of `[Fact]` or `[Test]` attributes (the exact framework is not specified by the signatures). Each method documents the scenario it exercises.

### `IsValid_WithRequiredFields_ReturnsTrue`
- **Purpose**: Verifies that a `UserContext` instance constructed with all required fields (`UserId`, `Email`) reports itself as valid.
- **Parameters**: None (parameterless test method).
- **Return value**: `void` (asserts that `IsValid` returns `true`).
- **Throws**: Test assertion failure if the result is not `true`.

### `IsValid_WithoutUserId_ReturnsFalse`
- **Purpose**: Confirms that omitting `UserId` causes the validation check to fail.
- **Parameters**: None.
- **Return value**: `void` (asserts that `IsValid` returns `false`).
- **Throws**: Test assertion failure if the result is not `false`.

### `IsValid_WithoutEmail_ReturnsFalse`
- **Purpose**: Confirms that omitting `Email` causes the validation check to fail.
- **Parameters**: None.
- **Return value**: `void` (asserts that `IsValid` returns `false`).
- **Throws**: Test assertion failure if the result is not `false`.

### `GetAttribute_StandardAttribute_ReturnsValue`
- **Purpose**: Ensures that a standard, well-known attribute (e.g., `"UserId"` or `"Email"`) can be retrieved and returns the expected value.
- **Parameters**: None.
- **Return value**: `void` (asserts equality between the retrieved value and the expected one).
- **Throws**: Test assertion failure on mismatch.

### `GetAttribute_CustomAttribute_ReturnsValue`
- **Purpose**: Validates that a user-defined custom attribute set via `SetCustomAttribute` is correctly returned by `GetAttribute`.
- **Parameters**: None.
- **Return value**: `void` (asserts the retrieved value matches what was stored).
- **Throws**: Test assertion failure on mismatch.

### `GetAttribute_NonExistentAttribute_ReturnsNull`
- **Purpose**: Checks that requesting an attribute key that has never been set yields `null` rather than throwing or returning a default value.
- **Parameters**: None.
- **Return value**: `void` (asserts the result is `null`).
- **Throws**: Test assertion failure if the result is non-null.

### `SetCustomAttribute_AddsAttribute`
- **Purpose**: Demonstrates that calling `SetCustomAttribute` with a valid key and value successfully stores the attribute so it becomes retrievable.
- **Parameters**: None.
- **Return value**: `void` (asserts the attribute is present after the call).
- **Throws**: Test assertion failure if the attribute is not found.

### `SetCustomAttribute_WithEmptyKey_ThrowsArgumentException`
- **Purpose**: Ensures that defensive validation rejects an empty or whitespace-only key by throwing an `ArgumentException`.
- **Parameters**: None.
- **Return value**: `void` (asserts the exception type and possibly the parameter name).
- **Throws**: Test assertion failure if no exception is thrown or the wrong type is raised.

### `GetConsistentHash_SameInput_ReturnsSameHash`
- **Purpose**: Verifies deterministic behavior: two calls to `GetConsistentHash` with the same `UserContext` state must return identical integer values.
- **Parameters**: None.
- **Return value**: `void` (asserts equality of two hash results).
- **Throws**: Test assertion failure if the hashes differ.

### `GetConsistentHash_DifferentUsers_ReturnsDifferentHashes`
- **Purpose**: Confirms that distinct user contexts (e.g., different `UserId` values) produce different hash values with high probability, avoiding collisions that would undermine feature flag distribution.
- **Parameters**: None.
- **Return value**: `void` (asserts inequality of the two hashes).
- **Throws**: Test assertion failure if the hashes are equal.

### `GetConsistentHash_ReturnsValueBetween0And99`
- **Purpose**: Validates the output range contract: the hash must fall within the inclusive range 0–99, suitable for percentage-based feature flag rollouts.
- **Parameters**: None.
- **Return value**: `void` (asserts the hash is ≥ 0 and ≤ 99).
- **Throws**: Test assertion failure if the value is out of bounds.

### `GetAttribute_CaseInsensitive_ReturnsValue`
- **Purpose**: Proves that attribute lookup is case-insensitive—requesting a key with different casing than the one used during storage still returns the correct value.
- **Parameters**: None.
- **Return value**: `void` (asserts the value is found despite casing differences).
- **Throws**: Test assertion failure if the lookup fails.

## Usage

The tests are invoked by a test runner (e.g., `dotnet test`). Below are two examples illustrating how the methods exercise `UserContext` indirectly.

### Example 1: Validating required fields

```csharp
// This test method would be structured as follows inside UserContextTests:
public void IsValid_WithRequiredFields_ReturnsTrue()
{
    var user = new UserContext
    {
        UserId = "user-123",
        Email = "user@example.com"
    };

    bool result = user.IsValid();

    Assert.True(result);
}

public void IsValid_WithoutUserId_ReturnsFalse()
{
    var user = new UserContext
    {
        Email = "user@example.com"
        // UserId intentionally missing
    };

    bool result = user.IsValid();

    Assert.False(result);
}
```

### Example 2: Consistent hashing and custom attributes

```csharp
public void GetConsistentHash_SameInput_ReturnsSameHash()
{
    var user = new UserContext
    {
        UserId = "consistent-user",
        Email = "consistent@example.com"
    };

    int hash1 = user.GetConsistentHash();
    int hash2 = user.GetConsistentHash();

    Assert.Equal(hash1, hash2);
}

public void SetCustomAttribute_AddsAttribute()
{
    var user = new UserContext
    {
        UserId = "attr-user",
        Email = "attr@example.com"
    };

    user.SetCustomAttribute("plan", "enterprise");

    string? value = user.GetAttribute("plan");
    Assert.Equal("enterprise", value);
}
```

## Notes

- **Edge cases**: `SetCustomAttribute_WithEmptyKey_ThrowsArgumentException` explicitly guards against empty keys, but tests do not cover null keys—consumers should treat null as a separate invalid case unless the implementation coalesces it to empty. `GetAttribute_NonExistentAttribute_ReturnsNull` establishes that missing keys return null; callers must null-check before using the result.
- **Hash distribution**: `GetConsistentHash_ReturnsValueBetween0And99` only validates the range, not uniformity. Additional statistical tests may be warranted if the hash is used for traffic splitting where bias would matter.
- **Case-insensitivity**: `GetAttribute_CaseInsensitive_ReturnsValue` confirms lookup is case-insensitive, but the storage semantics (whether keys are normalized on set) are not directly tested. If two keys differing only by case are set, behavior should be separately verified.
- **Thread-safety**: None of the test signatures indicate concurrent access scenarios. `UserContext` appears mutable via `SetCustomAttribute`, so external synchronization is presumed necessary if instances are shared across threads. The test suite does not validate thread safety.
