# StringExtensions
Provides a collection of pure‑function extension methods for `System.String` that perform common transformations, validation, and utility operations without relying on external state.

## API
### ToSha256
**Purpose** – Computes the SHA‑256 hash of the string and returns it as a lowercase hexadecimal string.  
**Parameters**  
- `this string input` – The string to hash.  
**Return value** – A 64‑character hex string representing the hash.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### ToHash32
**Purpose** – Calculates a deterministic 32‑bit hash code for the string.  
**Parameters**  
- `this string input` – The string to hash.  
**Return value** – An `Int32` hash code.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### IsValidEmail
**Purpose** – Determines whether the string resembles a valid e‑mail address using a regular expression.  
**Parameters**  
- `this string input` – The string to test.  
**Return value** – `true` if the string matches the e‑mail pattern; otherwise `false`.  
**Exceptions** – None; returns `false` for `null` or empty input.

### SnakeCaseToPascalCase
**Purpose** – Converts a `snake_case` string to `PascalCase`.  
**Parameters**  
- `this string input` – The string to convert.  
**Return value** – The transformed string; underscores are removed and the following character is capitalized.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### ToSnakeCase
**Purpose** – Converts a string to `snake_case`.  
**Parameters**  
- `this string input` – The string to convert.  
**Return value** – The string with spaces and punctuation replaced by underscores and all letters lower‑cased.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### Truncate
**Purpose** – Shortens the string to a specified maximum length, appending an ellipsis (`…`) when truncation occurs.  
**Parameters**  
- `this string input` – The string to truncate.  
- `int maxLength` – The maximum length of the returned string, including the ellipsis. Must be non‑negative.  
**Return value** – The original string if its length ≤ `maxLength`; otherwise the first `maxLength‑3` characters followed by `…`.  
**Exceptions** –  
- `ArgumentNullException` if `input` is `null`.  
- `ArgumentOutOfRangeException` if `maxLength` is less than 0.

### ToIntOrDefault
**Purpose** – Attempts to parse the string as a 32‑bit signed integer, returning a fallback value on failure.  
**Parameters**  
- `this string input` – The string to parse.  
- `int defaultValue` (optional, default = 0) – The value to return when parsing fails.  
**Return value** – The parsed integer or `defaultValue` if the input is `null`, empty, or not a valid integer.  
**Exceptions** – None; invalid input yields the default value.

### ToDoubleOrDefault
**Purpose** – Attempts to parse the string as a double‑precision floating‑point number, returning a fallback value on failure.  
**Parameters**  
- `this string input` – The string to parse.  
- `double defaultValue` (optional, default = 0.0) – The value to return when parsing fails.  
**Return value** – The parsed double or `defaultValue` for invalid input.  
**Exceptions** – None; invalid input yields the input`value` – `default` – The value to return when parsing results in the default value.

### ContainsAny
**Purpose** – Checks whether the string contains any of the supplied substrings.  
**Parameters**  
- `this string input` – The string to search.  
- `params string[] values` – One or more substrings to look for.  
**Return value** – `true` if at least one substring is found within `input`; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null` or if `values` is `null`.

### Repeat
**Purpose** – Produces a new string consisting of the original string repeated a specified number of times.  
**Parameters**  
- `this string input` – The string to repeat.  
- `int count` – Number of repetitions; must be non‑negative.  
**Return value** – A string with `input` concatenated `count` times. Returns an empty string when `count` is 0.  
**Exceptions** –  
- `ArgumentNullException` if `input` is `null`.  
- `ArgumentOutOfRangeException` if `count` is less than 0.

## Usage
```csharp
using DotNetFeatureFlags.Extensions; // assuming the namespace

string raw = "user@example.com";
bool isEmail = raw.IsValidEmail(); // true

string hashed = raw.ToSha256();
// hashed is a 64‑character hex string, e.g.
// "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"

string truncated = "The quick brown fox jumps over the lazy dog".Truncate(20);
// truncated => "The quick brown f…"
```

```csharp
string[] tags = { "alpha", "beta", "gamma" };
string source = "We have a beta release.";
bool contains = source.ContainsAny(tags); // true

string repeated = "ha".Repeat(3);
// repeated => "hahaha"

int number = "42".ToIntOrDefault(-1);
// number => 42
int fallback = "not a number".ToIntOrDefault(-1);
// fallback => -1
```

## Notes
- All methods are stateless and rely only on their input parameters; therefore they are thread‑safe and can be invoked concurrently from multiple threads.  
- Methods that accept a `string` instance treat a `null` reference as an error condition unless explicitly documented otherwise (`IsValidEmail`, `ToIntOrDefault`, `ToDoubleOrDefault` return sensible defaults instead of throwing).  
- Culture‑sensitive operations (e.g., case conversion in `SnakeCaseToPascalCase` and `ToSnakeCase`) use the invariant culture to guarantee consistent results across different environments.  
- The hash methods (`ToSha256`, `ToHash32`) produce the same output for identical inputs regardless of runtime or platform, making them suitable for use in caching or checksum scenarios.  
- `Truncate` counts UTF‑16 code units; surrogate pairs are treated as two characters, which may split a Unicode grapheme when the limit falls inside a pair.  
- `Repeat` may produce very large strings; callers should ensure that the resulting length does not exceed memory limits for their application.  
- The email validation performed by `IsValidEmail` follows a basic RFC‑5322‑inspired pattern; it is intended for quick sanity checks rather than full RFC compliance.
