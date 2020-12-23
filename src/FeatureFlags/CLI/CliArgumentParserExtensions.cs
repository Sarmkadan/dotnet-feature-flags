#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace FeatureFlags.CLI;

/// <summary>
/// Provides extension methods for <see cref="CliArgumentParser"/> to enhance CLI argument parsing functionality.
/// </summary>
public static class CliArgumentParserExtensions
{
    /// <summary>
    /// Gets the command name from parsed arguments in a case-insensitive manner.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The parsed command, or empty string if no command provided.</returns>
    public static string GetCommand(this CliArgumentParser parser, string[] args)
    {
        if (args.Length == 0)
        {
            return string.Empty;
        }

        return args[0].ToLower();
    }

    /// <summary>
    /// Parses arguments and returns a command with validation for required arguments.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="args">The command line arguments.</param>
    /// <param name="requiredArguments">Collection of required argument keys.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="ArgumentException">Thrown when required arguments are missing.</exception>
    public static CliCommand ParseWithValidation(this CliArgumentParser parser, string[] args, params string[] requiredArguments)
    {
        var command = CliArgumentParser.Parse(args);

        if (command.ShowHelp)
        {
            return command;
        }

        var missingArguments = requiredArguments
            .Where(arg => !command.HasArgument(arg))
            .ToList();

        if (missingArguments.Count > 0)
        {
            var missing = string.Join(", ", missingArguments);
            throw new ArgumentException($"Missing required arguments: {missing}");
        }

        return command;
    }

    /// <summary>
    /// Parses arguments and returns a command with validation for at least one of the required argument groups.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="args">The command line arguments.</param>
    /// <param name="requiredGroups">Collection of argument groups where at least one must be present.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="ArgumentException">Thrown when no arguments from any required group are provided.</exception>
    public static CliCommand ParseWithGroupValidation(this CliArgumentParser parser, string[] args, params string[][] requiredGroups)
    {
        var command = CliArgumentParser.Parse(args);

        if (command.ShowHelp)
        {
            return command;
        }

        var hasAnyRequired = requiredGroups.Any(group => group.Any(arg => command.HasArgument(arg)));

        if (!hasAnyRequired)
        {
            var allRequired = requiredGroups.SelectMany(g => g).ToList();
            var missing = string.Join(" or ", allRequired);
            throw new ArgumentException($"Missing required arguments: {missing}");
        }

        return command;
    }

    /// <summary>
    /// Gets all argument values for a given key, returning an empty collection if the key doesn't exist.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="command">The parsed command.</param>
    /// <param name="key">The argument key to retrieve.</param>
    /// <returns>Collection of values for the specified key.</returns>
    public static IReadOnlyCollection<string> GetAllArguments(this CliArgumentParser parser, CliCommand command, string key)
    {
        if (command.Arguments.TryGetValue(key, out var value))
        {
            return new[] { value };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Parses arguments and returns a formatted help string for the specified command.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="commandName">Name of the command to get help for.</param>
    /// <returns>Formatted help string.</returns>
    public static string GetCommandHelp(this CliArgumentParser parser, string commandName)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Help for command: {commandName}");
        sb.AppendLine();

        var examples = GetCommandExamples(commandName);
        if (examples.Count > 0)
        {
            sb.AppendLine("EXAMPLES:");
            foreach (var example in examples)
            {
                sb.AppendLine($"  {example}");
            }
            sb.AppendLine();
        }

        var options = GetCommandOptions(commandName);
        if (options.Count > 0)
        {
            sb.AppendLine("OPTIONS:");
            foreach (var option in options)
            {
                sb.AppendLine($"  {option}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Checks if the command has any arguments at all.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="command">The parsed command.</param>
    /// <returns>True if the command has arguments; otherwise, false.</returns>
    public static bool HasAnyArguments(this CliArgumentParser parser, CliCommand command)
    {
        return command.Arguments.Count > 0;
    }

    /// <summary>
    /// Gets the number of arguments for the command.
    /// </summary>
    /// <param name="parser">The argument parser instance.</param>
    /// <param name="command">The parsed command.</param>
    /// <returns>Number of arguments.</returns>
    public static int GetArgumentCount(this CliArgumentParser parser, CliCommand command)
    {
        return command.Arguments.Count;
    }

    private static List<string> GetCommandExamples(string commandName)
    {
        return commandName.ToLower() switch
        {
            "evaluate" => new List<string>
            {
                "dotnet run -- evaluate --key payment-v2 --user john@example.com",
                "dotnet run -- evaluate --key new-feature --context environment=production"
            },
            "create" => new List<string>
            {
                "dotnet run -- create --key new-feature --name \"New Feature\"",
                "dotnet run -- create --key experimental --percentage 50 --rollout-type gradual"
            },
            "update" => new List<string>
            {
                "dotnet run -- update --id 123 --name \"Updated Feature\"",
                "dotnet run -- update --id 456 --percentage 75"
            },
            "list" => new List<string>
            {
                "dotnet run -- list --page 1 --size 20",
                "dotnet run -- list --filter enabled=true"
            },
            "get" => new List<string>
            {
                "dotnet run -- get --key payment-v2",
                "dotnet run -- get --id 123"
            },
            "enable" => new List<string> { "dotnet run -- enable --key payment-v2" },
            "disable" => new List<string> { "dotnet run -- disable --key payment-v2" },
            "audit" => new List<string>
            {
                "dotnet run -- audit --key payment-v2 --days 30",
                "dotnet run -- audit --key new-feature"
            },
            "export" => new List<string>
            {
                "dotnet run -- export --format csv --output flags.csv",
                "dotnet run -- export --format json"
            },
            "import" => new List<string>
            {
                "dotnet run -- import --file flags.json --format json",
                "dotnet run -- import --file flags.csv --format csv"
            },
            "webhook" => new List<string>
            {
                "dotnet run -- webhook add --url https://example.com/webhook --event flag-updated",
                "dotnet run -- webhook list"
            },
            _ => new List<string>()
        };
    }

    private static List<string> GetCommandOptions(string commandName)
    {
        return commandName.ToLower() switch
        {
            "evaluate" => new List<string>
            {
                "--key KEY\tFeature flag key to evaluate",
                "--user USER\tUser identifier",
                "--context CONTEXT\tContext information in key=value format"
            },
            "create" => new List<string>
            {
                "--key KEY\tUnique feature flag key",
                "--name NAME\tHuman-readable name",
                "--description DESCRIPTION\tFeature flag description",
                "--rollout-type TYPE\tRollout strategy type",
                "--percentage PERCENTAGE\tPercentage of users to enable (0-100)"
            },
            "update" => new List<string>
            {
                "--id ID\tFeature flag ID to update",
                "--name NAME\tUpdated feature flag name",
                "--description DESCRIPTION\tUpdated description",
                "--percentage PERCENTAGE\tUpdated percentage (0-100)"
            },
            "list" => new List<string>
            {
                "--page PAGE\tPage number (default: 1)",
                "--size SIZE\tItems per page (default: 20)",
                "--filter FILTER\tFilter expression"
            },
            "get" => new List<string>
            {
                "--key KEY\tFeature flag key",
                "--id ID\tFeature flag ID"
            },
            "enable" => new List<string> { "--key KEY\tFeature flag key to enable" },
            "disable" => new List<string> { "--key KEY\tFeature flag key to disable" },
            "audit" => new List<string>
            {
                "--key KEY\tFeature flag key",
                "--days DAYS\tNumber of days to audit (default: 30)"
            },
            "export" => new List<string>
            {
                "--format FORMAT\tExport format (json, csv) (default: json)",
                "--output OUTPUT\tOutput file path"
            },
            "import" => new List<string>
            {
                "--file FILE\tFile to import",
                "--format FORMAT\tImport format (json, csv)"
            },
            "webhook" => new List<string>
            {
                "add\tAdd a new webhook",
                "remove\tRemove a webhook",
                "list\tList all webhooks",
                "--url URL\tWebhook URL",
                "--event EVENT\tEvent type (flag-created, flag-updated, flag-deleted)"
            },
            _ => new List<string>()
        };
    }
}