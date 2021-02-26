// existing content ...

## UserContextTestsExtensions

The `UserContextTestsExtensions` class provides extension methods for testing `UserContext` model behavior, including validation, attribute manipulation, and hash generation. These methods simplify creating test scenarios for user context validation, custom attributes, and consistent hashing in feature flag evaluation.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Assuming you have an instance of UserContextTests
var userContextTests = new UserContextTests();

// Create a valid user context with default attributes
var validUser = userContextTests.CreateValidUserContext();
userContextTests.ShouldBeValid(validUser); // Validate required fields

// Create a user with custom attributes
var customUser = userContextTests.WithCustomAttributes(new Dictionary<string, string>
{
    { "plan", "premium" },
    { "region", "eu" }
});

userContextTests.ShouldHaveAttribute(customUser, "plan", "premium"); // Verify attribute

// Create a user with deterministic hash for testing percentage rollouts
var hashUser = userContextTests.CreateUserContextWithHash("12345", "feature-abc");
int hashValue = userContextTests.GetHash(hashUser, "feature-abc");
Console.WriteLine($"Computed hash: {hashValue}");

// Create a collection of users for batch testing
var userCollection = userContextTests.CreateUserContextCollection(5);
foreach (var user in userCollection)
{
    Console.WriteLine($"User ID: {user.UserId}");
}

// Test tier-based evaluation
var tierUser = userContextTests.WithTier("enterprise");
userContextTests.ShouldHaveAttribute(tierUser, "tier", "enterprise");
```
