# ABTestVariant

The `ABTestVariant` class represents a specific variation within an A/B test linked to a feature flag, providing the data structure to manage allocation, track user assignments and conversions, and report statistical metrics for the variant.

## API

### Properties

*   **`Id` (int)**: The unique identifier for the variant.
*   **`FeatureFlagId` (int)**: The foreign key linking this variant to a parent feature flag.
*   **`VariantKey` (string)**: A unique alphanumeric key used to identify the variant in application logic (e.g., "variant_a").
*   **`DisplayName` (string)**: A human-readable name for the variant used in reporting.
*   **`Description` (string)**: A detailed description of the variant's purpose or characteristics.
*   **`AllocationPercentage` (int)**: The percentage of traffic (0–100) assigned to this variant.
*   **`UserCount` (long)**: The total number of unique users assigned to this variant.
*   **`ConversionCount` (long)**: The total number of users who have converted after being assigned to this variant.
*   **`CreatedAt` (DateTime)**: The timestamp indicating when the variant was created.
*   **`UpdatedAt` (DateTime)**: The timestamp indicating when the variant was last updated.
*   **`IsControl` (bool)**: Indicates whether this variant serves as the control group for the A/B test.
*   **`FeatureFlag` (FeatureFlag?)**: The navigation property to the associated `FeatureFlag` object; can be null if not loaded.

### Methods

*   **`GetConversionRate()`**: Returns the calculated conversion rate (`ConversionCount` / `UserCount`). Returns 0.0 if `UserCount` is 0.
*   **`RecordUserAssignment()`**: Increments the `UserCount` for this variant.
*   **`RecordConversion()`**: Increments the `ConversionCount` for this variant.
*   **`IsValid()`**: Validates whether the variant configuration meets required criteria (e.g., valid allocation range).
*   **`GetStatisticalConfidence()`**: Returns a string representation of the statistical confidence or analysis result based on current tracking data.

## Usage

### Example 1: Recording Assignments and Conversions
```csharp
// Retrieve a variant from a feature flag
var variant = featureFlag.Variants.FirstOrDefault(v => v.VariantKey == "new_ui");

if (variant != null)
{
    // Track user assignment to the variant
    variant.RecordUserAssignment();

    // Track a successful conversion
    variant.RecordConversion();
    
    Console.WriteLine($"Current conversion rate: {variant.GetConversionRate():P2}");
}
```

### Example 2: Validating Variant Configuration
```csharp
var variant = new ABTestVariant 
{ 
    VariantKey = "experimental_checkout", 
    AllocationPercentage = 50 
};

if (variant.IsValid())
{
    // Proceed to register the variant with the test engine
    RegisterVariant(variant);
}
else
{
    throw new InvalidOperationException("Variant configuration is invalid.");
}
```

## Notes

*   **Thread Safety**: The `ABTestVariant` class is not inherently thread-safe. If multiple threads concurrently call `RecordUserAssignment()` or `RecordConversion()`, external synchronization mechanisms (such as `lock` or atomic operations) must be implemented to ensure data integrity.
*   **Data Dependencies**: The `FeatureFlag` navigation property relies on the underlying data access layer to be populated (e.g., via Entity Framework Include). Accessing this property when it is null may result in a `NullReferenceException` if not checked beforehand.
*   **Calculations**: The `GetConversionRate()` method handles division by zero by returning 0.0, but calling code should be aware of this behavior if a 0% rate is indistinguishable from an uninitialized state.
