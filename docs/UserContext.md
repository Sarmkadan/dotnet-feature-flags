# UserContext

The `UserContext` class encapsulates identity and metadata information about a user, designed primarily for feature flag evaluation. It provides a standardized structure for required identification fields such as `UserId` and `Email`, while offering support for optional categorization through fields like `Country`, `Tier`, and `Region`. Furthermore, it includes a flexible mechanism for storing arbitrary key-value pairs via a custom attributes dictionary, enabling complex targeting rules based on user-specific properties.

## API

### Properties

*   **`UserId` (string)**: The unique identifier for the user.
*   **`Email` (string)**: The email address associated with the user.
*   **`Country` (string?)**: The optional two-letter ISO country code or full country name of the user.
*   **`Tier` (string?)**: An optional string representing the user's subscription level or account tier (e.g., "Free", "Premium", "Enterprise").
*   **`Region` (string?)**: An optional identifier for the user's geographic region, used for localized feature rollouts.
*   **`CustomAttributes` (Dictionary<string, string>)**: A dictionary containing arbitrary key-value pairs assigned to the user for advanced feature flag targeting.
*   **`CreatedAt` (DateTime)**: The timestamp indicating when the `UserContext` instance was created.
*   **`IsValid` (bool)**: A read-only property that indicates whether the `UserContext` contains the minimum required data (e.g., non-empty `UserId` and `Email`).

### Methods

*   **`GetAttribute(string key)` (string?)**: Retrieves the value associated with the specified key from `CustomAttributes`. Returns `null` if the key does not exist.
*   **`SetCustomAttribute(string key, string value)` (void)**: Adds or updates a key-value pair in the `CustomAttributes` dictionary.
*   **`GetConsistentHash()` (int)**: Computes a consistent integer hash based on the `UserId` to facilitate stable feature flag assignment, ensuring that a user consistently receives the same feature state.

## Usage

### Basic Initialization
```csharp
var context = new UserContext 
{
    UserId = "user-123",
    Email = "jane.doe@example.com",
    Country = "US",
    Tier = "Premium"
};

if (context.IsValid)
{
    // Proceed with feature evaluation
}
```

### Setting Custom Attributes
```csharp
var context = new UserContext { UserId = "user-456", Email = "bob@example.com" };

context.SetCustomAttribute("BetaUser", "true");
context.SetCustomAttribute("DeviceType", "Mobile");

string isBeta = context.GetAttribute("BetaUser"); // returns "true"
```

## Notes

*   **Thread Safety**: The `UserContext` class and its `CustomAttributes` dictionary are not inherently thread-safe. If multiple threads access or modify a `UserContext` instance simultaneously, appropriate external synchronization mechanisms should be employed.
*   **Validity Logic**: The `IsValid` property performs basic validation to ensure the essential identity fields (`UserId` and `Email`) are not null or whitespace. It does not validate the format of the email or the content of custom attributes.
*   **Hashing Stability**: The `GetConsistentHash` method provides a stable hash for a given `UserId`. This is essential for ensuring that users do not experience flickering when feature flags are evaluated across different sessions or servers.
