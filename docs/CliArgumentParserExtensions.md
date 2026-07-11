# CliArgumentParserExtensions

`CliArgumentParserExtensions` provides a set of static extension methods for parsing, validating, and inspecting command-line arguments in the context of feature flag management. These methods facilitate the extraction and validation of CLI commands and arguments, enabling structured interaction with feature flag configurations through command-line interfaces.

## API

### GetCommand
```csharp
public static string GetCommand(this string[] args, string commandName)
```
Extracts the value associated with a specified command name from the argument array.  
**Parameters**:  
- `args`: The array of command-line arguments.  
- `commandName`: The name of the command to retrieve.  
**Returns**: The string value of the command if found; otherwise, `null`.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args` or `commandName` is `null`.

---

### ParseWithValidation
```csharp
public static CliCommand ParseWithValidation(this string[] args, IEnumerable<string> validCommands)
```
Parses the argument array into a `CliCommand` object after validating against a list of allowed commands.  
**Parameters**:  
- `args`: The array of command-line arguments.  
- `validCommands`: A collection of valid command names for validation.  
**Returns**: A `CliCommand` instance representing the parsed command.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args` or `validCommands` is `null`.  
- `InvalidOperationException`: Thrown when no valid command is found in `args`.

---

### ParseWithGroupValidation
```csharp
public static CliCommand ParseWithGroupValidation(this string[] args, IEnumerable<string> validCommands, string groupName)
```
Parses and validates the argument array against a group of commands, ensuring the command belongs to the specified group.  
**Parameters**:  
- `args`: The array of command-line arguments.  
- `validCommands`: A collection of valid command names for validation.  
- `groupName`: The name of the command group to validate against.  
**Returns**: A `CliCommand` instance representing the parsed command.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args`, `validCommands`, or `groupName` is `null`.  
- `InvalidOperationException`: Thrown when no valid command is found in `args` or the command does not belong to the specified group.

---

### GetAllArguments
```csharp
public static IReadOnlyCollection<string> GetAllArguments(this string[] args)
```
Retrieves all arguments from the input array as a read-only collection.  
**Parameters**:  
- `args`: The array of command-line arguments.  
**Returns**: An `IReadOnlyCollection<string>` containing all arguments.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args` is `null`.

---

### GetCommandHelp
```csharp
public static string GetCommandHelp(this string[] args, string commandName)
```
Generates help text for a specified command based on the argument array.  
**Parameters**:  
- `args`: The array of command-line arguments.  
- `commandName`: The name of the command to generate help text for.  
**Returns**: A string containing the help text for the command.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args` or `commandName` is `null`.

---

### HasAnyArguments
```csharp
public static bool HasAnyArguments(this string[] args)
```
Determines whether the argument array contains any elements.  
**Parameters**:  
- `args`: The array of command-line arguments.  
**Returns**: `true` if the array contains one or more arguments; otherwise, `false`.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args` is `null`.

---

### GetArgumentCount
```csharp
public static int GetArgumentCount(this string[] args)
```
Returns the total number of arguments in the array.  
**Parameters**:  
- `args`: The array of command-line arguments.  
**Returns**: The count of arguments as an integer.  
**Exceptions**:  
- `ArgumentNullException`: Thrown when `args` is `null`.

---

## Usage

### Example 1: Parsing and Validating Commands
```csharp
string[] args = { "--command", "enable-feature", "--group", "security" };
var validCommands = new[] { "enable-feature", "disable-feature" };
try
{
    CliCommand command = args.ParseWithGroupValidation(validCommands, "security");
    Console.WriteLine($"Parsed command: {command.Name}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

### Example 2: Checking for Arguments
```csharp
string[] args = { "--verbose", "true" };
if (args.HasAnyArguments())
{
    int count = args.GetArgumentCount();
    Console.WriteLine($"Total arguments: {count}");
}
else
{
    Console.WriteLine("No arguments provided.");
}
```

---

## Notes

- All methods are static and thread-safe provided that the input `args` array is not modified concurrently.  
- `GetCommand`, `ParseWithValidation`, and `ParseWithGroupValidation` may return `null` or throw exceptions if the input array is empty or does not contain expected command structures.  
- `GetAllArguments` returns an empty collection rather than `null` for empty input arrays.  
- `GetCommandHelp` relies on predefined help templates and may return an empty string if no help text is associated with the specified command.
