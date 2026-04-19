#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.CLI;

/// <summary>
/// Parses command-line arguments and converts them to structured command objects.
/// Provides help text generation and validation for CLI arguments.
/// </summary>
public sealed class CliArgumentParser
{
    public static void PrintHelp()
    {
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                     Feature Flag Engine - CLI Interface                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

COMMANDS:
  evaluate     Evaluate if a feature flag is enabled for a user
  create       Create a new feature flag
  update       Update an existing feature flag
  list         List all feature flags
  get          Get details of a specific feature flag
  enable       Enable a feature flag
  disable      Disable a feature flag
  audit        Show audit log for a feature flag
  webhook      Manage webhooks
  export       Export feature flags to file
  import       Import feature flags from file
  help         Show this help message

EXAMPLES:
  dotnet run -- evaluate --key payment-v2 --user john@example.com
  dotnet run -- create --key new-feature --name ""New Feature""
  dotnet run -- list --page 1 --size 20
  dotnet run -- enable --key payment-v2
  dotnet run -- audit --key payment-v2 --days 30
  dotnet run -- export --format csv --output flags.csv

GLOBAL OPTIONS:
  --help, -h           Show help information
  --verbose, -v        Enable verbose logging
  --quiet, -q          Suppress non-error output
  --config, -c PATH    Path to configuration file
  --json               Output results as JSON

For command-specific help: dotnet run -- <command> --help
");
    }

    /// <summary>
    /// Parses command line arguments into a structured CLI command.
    /// </summary>
    public static CliCommand Parse(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return new CliCommand { Command = "help" };
        }

        var command = args[0].ToLower();

        var command_result = command switch
        {
            "evaluate" => ParseEvaluateCommand(args),
            "create" => ParseCreateCommand(args),
            "update" => ParseUpdateCommand(args),
            "list" => ParseListCommand(args),
            "get" => ParseGetCommand(args),
            "enable" => ParseEnableCommand(args),
            "disable" => ParseDisableCommand(args),
            "audit" => ParseAuditCommand(args),
            "export" => ParseExportCommand(args),
            "import" => ParseImportCommand(args),
            "webhook" => ParseWebhookCommand(args),
            "help" or "-h" or "--help" => new CliCommand { Command = "help" },
            _ => throw new ArgumentException($"Unknown command: {command}")
        };

        return command_result;
    }

    private static CliCommand ParseEvaluateCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "evaluate" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--key" when i + 1 < args.Length:
                    cmd.Arguments["key"] = args[++i];
                    break;
                case "--user" when i + 1 < args.Length:
                    cmd.Arguments["user"] = args[++i];
                    break;
                case "--context" when i + 1 < args.Length:
                    cmd.Arguments["context"] = args[++i];
                    break;
                case "--help":
                    cmd.ShowHelp = true;
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseCreateCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "create" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--key" when i + 1 < args.Length:
                    cmd.Arguments["key"] = args[++i];
                    break;
                case "--name" when i + 1 < args.Length:
                    cmd.Arguments["name"] = args[++i];
                    break;
                case "--description" when i + 1 < args.Length:
                    cmd.Arguments["description"] = args[++i];
                    break;
                case "--rollout-type" when i + 1 < args.Length:
                    cmd.Arguments["rolloutType"] = args[++i];
                    break;
                case "--percentage" when i + 1 < args.Length:
                    cmd.Arguments["percentage"] = args[++i];
                    break;
                case "--help":
                    cmd.ShowHelp = true;
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseUpdateCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "update" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--id" when i + 1 < args.Length:
                    cmd.Arguments["id"] = args[++i];
                    break;
                case "--name" when i + 1 < args.Length:
                    cmd.Arguments["name"] = args[++i];
                    break;
                case "--description" when i + 1 < args.Length:
                    cmd.Arguments["description"] = args[++i];
                    break;
                case "--percentage" when i + 1 < args.Length:
                    cmd.Arguments["percentage"] = args[++i];
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseListCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "list" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--page" when i + 1 < args.Length:
                    cmd.Arguments["page"] = args[++i];
                    break;
                case "--size" when i + 1 < args.Length:
                    cmd.Arguments["size"] = args[++i];
                    break;
                case "--filter" when i + 1 < args.Length:
                    cmd.Arguments["filter"] = args[++i];
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseGetCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "get" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--key" when i + 1 < args.Length:
                    cmd.Arguments["key"] = args[++i];
                    break;
                case "--id" when i + 1 < args.Length:
                    cmd.Arguments["id"] = args[++i];
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseEnableCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "enable" };

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].ToLower() == "--key" && i + 1 < args.Length)
            {
                cmd.Arguments["key"] = args[++i];
            }
        }

        return cmd;
    }

    private static CliCommand ParseDisableCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "disable" };

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].ToLower() == "--key" && i + 1 < args.Length)
            {
                cmd.Arguments["key"] = args[++i];
            }
        }

        return cmd;
    }

    private static CliCommand ParseAuditCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "audit" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--key" when i + 1 < args.Length:
                    cmd.Arguments["key"] = args[++i];
                    break;
                case "--days" when i + 1 < args.Length:
                    cmd.Arguments["days"] = args[++i];
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseExportCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "export" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--format" when i + 1 < args.Length:
                    cmd.Arguments["format"] = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    cmd.Arguments["output"] = args[++i];
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseImportCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "import" };

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--file" when i + 1 < args.Length:
                    cmd.Arguments["file"] = args[++i];
                    break;
                case "--format" when i + 1 < args.Length:
                    cmd.Arguments["format"] = args[++i];
                    break;
            }
        }

        return cmd;
    }

    private static CliCommand ParseWebhookCommand(string[] args)
    {
        var cmd = new CliCommand { Command = "webhook" };

        if (args.Length > 1)
        {
            cmd.Arguments["action"] = args[1].ToLower();
        }

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--url" when i + 1 < args.Length:
                    cmd.Arguments["url"] = args[++i];
                    break;
                case "--event" when i + 1 < args.Length:
                    cmd.Arguments["event"] = args[++i];
                    break;
            }
        }

        return cmd;
    }
}

/// <summary>
/// Represents a parsed CLI command with its arguments.
/// </summary>
public sealed class CliCommand
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, string> Arguments { get; set; } = new();
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Gets an argument value by key, or null if not provided.
    /// </summary>
    public string? GetArgument(string key)
    {
        return Arguments.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Checks if a required argument is present.
    /// </summary>
    public bool HasArgument(string key)
    {
        return Arguments.ContainsKey(key);
    }
}
