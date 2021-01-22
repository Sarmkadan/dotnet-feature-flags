# UserContextTestsExtensions

`UserContextTestsExtensions` is a utility class in the `dotnet-feature-flags` project that provides helper methods for creating, modifying, and asserting the validity of `UserContext` instances in unit tests. It simplifies test setup and validation by encapsulating common patterns for user context manipulation and verification.

## API

### CreateValidUserContext

Creates a valid `UserContext` instance with default or required properties populated.

**Parameters:**  
None.

**Return Value:**  
A `UserContext` instance with valid default values.

**Exceptions:**  
None. Always returns a valid instance.

---

### WithCustomAttributes

Adds custom attributes to an existing `UserContext` instance.

**Parameters:**  
- `userContext` (`UserContext`): The target user context to modify.  
- `attributes` (`Dictionary<string, string>`): Key-value pairs representing custom attributes.

**Return Value:**  
The modified `UserContext` instance with added attributes.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `userContext` or `attributes` is `null`.

---

### ShouldHaveAttribute

Asserts that a `UserContext` contains a specific attribute key.

**Parameters:**  
- `userContext` (`UserContext`): The user context to validate.  
- `attributeKey` (`string`): The attribute key to check for existence.

**Return Value:**  
`void`.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `userContext` or `attributeKey` is `null`.  
- `AssertFailedException`: Thrown if the attribute key does not exist in the user context.

---

### ShouldBeValid

Validates that a `UserContext` meets all required criteria for validity.

**Parameters:**  
- `userContext` (`UserContext`): The user context to validate.

**Return Value:**  
`void`.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `userContext` is `null`.  
- `AssertFailedException`: Thrown if the user context is invalid.

---

### ShouldBeInvalid

Asserts that a `UserContext` fails validation checks.

**Parameters:**  
- `userContext` (`UserContext`): The user context to validate.

**Return Value:**  
`void`.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `userContext` is `null`.  
- `AssertFailedException`: Thrown if the user context is valid.

---

### CreateUserContextWithHash

Creates a `UserContext` instance with a precomputed hash value.

**Parameters:**  
- `name` (`string`): The user's name.  
- `hash` (`int`): The precomputed hash value.

**Return Value:**  
A `UserContext` instance with the specified hash.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `name` is `null`.

---

### GetHash

Retrieves the hash value from a `UserContext` instance.

**Parameters:**  
- `userContext` (`UserContext`): The user context to extract the hash from.

**Return Value:**  
The `int` hash value of the user context.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `userContext` is `null`.

---

### CreateUserContextCollection

Generates a collection of `UserContext` instances for testing scenarios involving multiple users.

**Parameters:**  
- `count` (`int`): The number of user contexts to generate.

**Return Value:**  
An `IEnumerable<UserContext>` containing the specified number of valid user contexts.

**Exceptions:**  
- `ArgumentOutOfRangeException`: Thrown if `count` is less than 1.

---

### WithTier

Adds a tier value to a `UserContext` instance.

**Parameters:**  
- `userContext` (`UserContext`): The target user context to modify.  
- `tier` (`string`): The tier value to assign.

**Return Value:**  
The modified `UserContext` instance with the tier set.

**Exceptions:**  
- `ArgumentNullException`: Thrown if `userContext` or `tier` is `null`.

---

## Usage

```csharp
[Test]
public void UserContext_WithCustomAttributes_ShouldContainAttributes()
{
    var userContext = UserContextTestsExtensions.CreateValidUserContext()
        .WithCustomAttributes(new Dictionary<string, string>
        {
            { "Region", "NorthAmerica" },
            { "SubscriptionLevel", "Premium" }
        });

    userContext.ShouldHaveAttribute("Region");
    userContext.ShouldHaveAttribute("SubscriptionLevel");
}
```

```csharp
[Test]
public void UserContext_WithHash_ShouldMatchExpectedValue()
{
    var userContext = UserContextTestsExtensions.CreateUserContextWithHash("Alice", 12345);
    var hash = UserContextTestsExtensions.GetHash(userContext);

    Assert.AreEqual(12345, hash);
}
```

---

## Notes

- **Null Handling:** All methods that accept `UserContext` or string parameters will throw `ArgumentNullException` if passed `null`, ensuring explicit failure in invalid test setups.  
- **Thread Safety:** These methods are stateless and do not modify shared static state. They are safe for concurrent use in parallel test execution environments.  
- **Edge Cases:**  
  - `CreateUserContextCollection(0)` will throw an exception due to the `count` validation.  
  - `WithCustomAttributes` and `WithTier` return the modified instance, allowing method chaining.  
  - `ShouldBeInvalid` is intended for negative test cases where invalid user contexts are explicitly constructed.
