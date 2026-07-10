# FeatureFlagException

Represents the base exception type for all feature flag related errors in the `dotnet-feature-flags` library. It extends `System.Exception` and adds an optional `ErrorCode` property to provide additional context about the failure.

## API

### FeatureFlagException

- **FeatureFlagException()**  
  Initializes a new instance of the `FeatureFlagException` class with a default message supplied by the base `Exception` class.  
  *Parameters:* none.  
  *Return:* (constructor).  
  *Throws:* does not throw any exceptions itself.

- **FeatureFlagException(string message)**  
  Initializes a new instance with a specified error message.  
  *Parameters:*  
    - `message`: The message that describes the error.  
  *Return:* (constructor).  
  *Throws:* `ArgumentNullException` if `message` is `null`.

- **FeatureFlagException(string message, string errorCode)**  
  Initializes a new instance with a specified error message and an associated error code.  
  *Parameters:*  
    - `message`: The message that describes the error.  
    - `errorCode`: An optional identifier for the error.  
  *Return:* (constructor).  
  *Throws:* `ArgumentNullException` if `message` is `null`.

- **FeatureFlagException(string message, Exception innerException)**  
  Initializes a new instance with a specified error message and a reference to the inner exception that caused this exception.  
  *Parameters:*  
    - `message`: The message that describes the error.  
    - `innerException`: The exception that is the cause of the current exception, or `null` if no inner exception is specified.  
  *Return:* (constructor).  
  *Throws:* `ArgumentNullException` if `message` is `null`.

- **string? ErrorCode**  
  Gets or sets an optional error code associated with the exception.  
  *Parameters:* none.  
  *Return:* A nullable string containing the error code, or `null` if no code is set.  
  *Throws:* does not throw any exceptions.

### FeatureFlagNotFoundException

- **FeatureFlagNotFoundException()**  
  Initializes a new instance of the `FeatureFlagNotFoundException` class with a default message.  
  *Parameters:* none.  
  *Return:* (constructor).  
  *Throws:* does not throw any exceptions.

- **FeatureFlagNotFoundException(string message)**  
  Initializes a new instance with a specified error message.  
  *Parameters:*  
    - `message`: The message that describes the error.  
  *Return:* (constructor.*Throws:* `ArgumentNullException` if `message` is `null`.

### InvalidFeatureFlagException

- **InvalidFeatureFlagException()**  
  Initializes a new instance of the `InvalidFeatureFlagException` class with a default message.  
  *Parameters:* none.  
  *Return:* (constructor).  
  *Throws:* does not throw any exceptions.

- **InvalidFeatureFlagException(string message)**  
  Initializes a new instance with a specified error message.  
  *Parameters:*  
    - `message`: The message that describes the error.  
  *Return:* (constructor).  
  *Throws:* `ArgumentNullException` if `message` is `null`.

### RuleEvaluationException

- **RuleEvaluationException()**  
  Initializes a new instance of the `RuleEvaluationException` class with a default message.  
  *Parameters:* none.  
  *Return:* (constructor).  
  *Throws:* does not throw any exceptions.

- **RuleEvaluationException(string message)**  
  Initializes a new instance with a specified error message.  
  *Parameters:*  
    - `message`: The message that describes the error.  
  *Return:* (constructor).  
  *Throws:* `ArgumentNullException` if `message` is `null`.

### FeatureFlagDataException

- **FeatureFlagDataException()**  
  Initializes a new instance of the `FeatureFlagDataException` class with a default message.  
  *Parameters:* none.  
  *Return:* (constructor).  
  *Throws:* does not throw any exceptions.

- **FeatureFlagDataException(string message)**  
  Initializes a new instance with a specified error message.  
  *Parameters:*  
    - `message`: The message that describes the error.  
  *Return:* (constructor).  
  *Throws:* `ArgumentNullException` if `message` is `null`.

## Usage

```csharp
using Microsoft.FeatureManagement;

public class FeatureService
{
    public bool IsEnabled(string featureName)
    {
        try
        {
            // Assume _featureManager is an IFeatureManager instance
            return _featureManager.IsEnabledAsync(featureName).GetAwaiter().GetResult();
        }
        catch (FeatureFlagException ex) when (ex.ErrorCode == "FEATURE_NOT_FOUND")
        {
            // Log or handle missing feature flag
            Console.WriteLine($"Feature flag '{featureName}' is not defined.");
            return false;
        }
    }
}
```

```csharp
using Microsoft.FeatureManagement;

public class FeatureValidator
{
    public void ValidateFeature(string featureName, string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new InvalidFeatureFlagException(
                $"The evaluation expression for feature '{featureName}' is invalid.",
                errorCode: "INVALID_EXPRESSION");
        }

        // Additional validation logic...
    }
}
```

## Notes

- The `ErrorCode` property is optional and may be `null`. Consumers should check for `null` before relying on its value.
- Derived exception types (`FeatureFlagNotFoundException`, `InvalidFeatureFlagException`, `RuleEvaluationException`, `FeatureFlagDataException`) are intended to be caught specifically to handle distinct error scenarios while still allowing a catch-all for `FeatureFlagException`.
- Exception objects are immutable after construction; therefore, they are safe to publish and share across multiple threads without additional synchronization.
- Setting the `ErrorCode` property after construction is not thread‑safe if the same exception instance is accessed concurrently; it is recommended to set the property only during initialization or from a single thread.
