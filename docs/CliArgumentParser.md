# CliArgumentParser

`CliArgumentParser` provides a lightweight command-line argument parsing facility for the `dotnet-feature-flags` toolset. It transforms raw string arrays into a structured representation containing a primary command name, an optional dictionary of named arguments, and a flag indicating whether help text was requested. The type is designed for single-command, single-invocation scenarios where the caller needs to extract positional and named parameters without depending on heavier CLI frameworks.

## API

### public static void PrintHelp

Writes the standard usage and argument reference to the console output stream. This method does not accept parameters and does not return a value. It is intended to be invoked when the parser detects a help request or when the caller determines that guidance should be displayed. No exceptions are thrown by this method under normal operation.

### public static CliCommand Parse

Parses a raw argument array into a populated `CliCommand` instance. The method expects a `string[]` representing the command-line tokens, typically the array passed to `Main`. It returns a `CliCommand` whose `Command`, `Arguments`, and `ShowHelp` members are set according to the token content. The method throws an `ArgumentException` when the input array is null, and throws a `FormatException` when a named argument is supplied without a corresponding value (i.e. a key token that expects a subsequent value token is the final element).

### public string Command

Gets the primary command string extracted from the first positional token. If no tokens were provided, this property returns an empty string. The value is set exclusively by `Parse` and remains immutable for the lifetime of the instance.

### public Dictionary\<string, string\> Arguments

Gets a dictionary of named arguments parsed from tokens of the form `--key value` or `-k value`. Keys are stored without their leading dashes. If the input contains duplicate keys, the last occurrence wins. When no named arguments are present, the dictionary is empty. The dictionary is populated by `Parse` and should be treated as read-only after construction.

### public bool ShowHelp

Indicates whether the argument set included a help flag (`--help`, `-h`, or `-?`). When `true`, the caller is expected to display usage information, typically by calling `PrintHelp`, and should not proceed with normal command execution. The value is determined by `Parse` and does not change.

### public string? GetArgument

Retrieves the value associated with a named argument key. The `key` parameter is a case-sensitive string that must match the dictionary key exactly (without leading dashes). Returns the corresponding value if the key exists; otherwise returns `null`. This method never throws.

### public bool HasArgument

Determines whether a named argument key is present in the parsed argument set. The `key` parameter follows the same case-sensitive, dash-free convention as `GetArgument`. Returns `true` if the key exists in the `Arguments` dictionary, `false` otherwise. This method never throws.

## Usage

### Example 1: Basic command with feature flag lookup

```csharp
string[] args = new[] { "query", "--feature-id", "FF-2024", "--environment", "staging" };

CliCommand cmd = CliArgumentParser.Parse(args);

if (cmd.ShowHelp)
{
    CliArgumentParser.PrintHelp();
    return;
}

string featureId = cmd.GetArgument("feature-id") ?? "all";
string environment = cmd.GetArgument("environment") ?? "production";

Console.WriteLine($"Querying feature '{featureId}' in environment '{environment}'.");
```

### Example 2: Minimal invocation with help detection

```csharp
string[] args = new[] { "validate", "-h" };

CliCommand cmd = CliArgumentParser.Parse(args);

if (cmd.ShowHelp)
{
    CliArgumentParser.PrintHelp();
    return;
}

if (!cmd.HasArgument("config"))
{
    Console.WriteLine("Error: --config is required for validation.");
    return;
}

string configPath = cmd.GetArgument("config");
Console.WriteLine($"Validating configuration at '{configPath}'.");
```

## Notes

- **Edge cases**: When the input array is empty, `Parse` returns a `CliCommand` with an empty `Command`, an empty `Arguments` dictionary, and `ShowHelp` set to `false`. A named argument token appearing as the final element without a value causes a `FormatException`. Duplicate keys are resolved silently by last-write-wins semantics; no warning is emitted.
- **Thread safety**: `Parse` and `PrintHelp` are static methods that operate on their own local state and do not mutate shared resources (beyond `PrintHelp` writing to the console). `CliCommand` instances are effectively immutable after construction. Concurrent reads of `Command`, `Arguments`, `ShowHelp`, `GetArgument`, and `HasArgument` are safe without synchronization. Concurrent calls to `Parse` or `PrintHelp` from multiple threads are safe, though `PrintHelp` may interleave console output if invoked simultaneously.
- **Key format**: `GetArgument` and `HasArgument` expect keys without leading dashes. Passing `"--feature-id"` will not match an argument parsed from `--feature-id`; use `"feature-id"` instead.
