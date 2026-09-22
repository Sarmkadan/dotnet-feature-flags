#nullable enable

using System;
using System.Collections.Generic;
using FeatureFlags.CLI;
using Xunit;

namespace FeatureFlags.Tests;

public class CliArgumentParserExtensionsTests
{
    private readonly CliArgumentParser _parser = new();

    [Fact]
    public void GetCommand_WithArguments_ReturnsFirstArgumentLowercase()
    {
        // Arrange
        string[] args = { "EVALUATE", "--key", "test" };

        // Act
        string command = _parser.GetCommand(args);

        // Assert
        Assert.Equal("evaluate", command);
    }

    [Fact]
    public void GetCommand_WithEmptyArguments_ReturnsEmptyString()
    {
        // Arrange
        string[] args = Array.Empty<string>();

        // Act
        string command = _parser.GetCommand(args);

        // Assert
        Assert.Equal(string.Empty, command);
    }

    [Fact]
    public void GetCommand_WithNullArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.GetCommand(null!));
    }

    [Fact]
    public void ParseWithValidation_WithAllRequiredArguments_ReturnsParsedCommand()
    {
        // Arrange
        string[] args = { "create", "--key", "test-flag", "--name", "Test Flag" };
        string[] requiredArguments = { "key", "name" };

        // Act
        CliCommand command = _parser.ParseWithValidation(args, requiredArguments);

        // Assert
        Assert.Equal("create", command.Command);
        Assert.Equal("test-flag", command.GetArgument("key"));
        Assert.Equal("Test Flag", command.GetArgument("name"));
    }

    [Fact]
    public void ParseWithValidation_WithMissingRequiredArgument_ThrowsArgumentException()
    {
        // Arrange
        string[] args = { "create", "--key", "test-flag" };
        string[] requiredArguments = { "key", "name" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _parser.ParseWithValidation(args, requiredArguments));
        Assert.Contains("Missing required arguments: name", exception.Message);
    }

    [Fact]
    public void ParseWithValidation_WithNullArgs_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.ParseWithValidation(null!, "key"));
    }

    [Fact]
    public void ParseWithValidation_WithNullRequiredArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.ParseWithValidation(new[] { "test" }, null!));
    }

    [Fact]
    public void ParseWithValidation_WithHelpCommand_ReturnsCommandWithoutValidation()
    {
        // Arrange
        string[] args = { "help" };
        string[] requiredArguments = { "some-required-arg" };

        // Act
        CliCommand command = _parser.ParseWithValidation(args, requiredArguments);

        // Assert
        Assert.Equal("help", command.Command);
        Assert.True(command.ShowHelp);
    }

    [Fact]
    public void ParseWithGroupValidation_WithAtLeastOneArgumentFromEachGroup_ReturnsParsedCommand()
    {
        // Arrange
        string[] args = { "create", "--key", "test-flag" };
        string[][] requiredGroups = {
            new[] { "key", "name" }, // Either key or name is required
            new[] { "description", "percentage" } // Either description or percentage is required
        };

        // Act
        CliCommand command = _parser.ParseWithGroupValidation(args, requiredGroups);

        // Assert
        Assert.Equal("create", command.Command);
        Assert.Equal("test-flag", command.GetArgument("key"));
    }

    [Fact]
    public void ParseWithGroupValidation_WithNoArgumentsFromAnyGroup_ThrowsArgumentException()
    {
        // Arrange
        string[] args = { "create" }; // No arguments provided
        string[][] requiredGroups = {
            new[] { "key", "name" },
            new[] { "description", "percentage" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _parser.ParseWithGroupValidation(args, requiredGroups));
        Assert.Contains("Missing required arguments: key or name or description or percentage", exception.Message);
    }

    [Fact]
    public void ParseWithGroupValidation_WithNullArgs_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.ParseWithGroupValidation(null!, new[] { new[] { "key" } }));
    }

    [Fact]
    public void ParseWithGroupValidation_WithNullRequiredGroups_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.ParseWithGroupValidation(new[] { "test" }, null!));
    }

    [Fact]
    public void ParseWithGroupValidation_WithHelpCommand_ReturnsCommandWithoutValidation()
    {
        // Arrange
        string[] args = { "help" };
        string[][] requiredGroups = { new[] { "some-required-arg" } };

        // Act
        CliCommand command = _parser.ParseWithGroupValidation(args, requiredGroups);

        // Assert
        Assert.Equal("help", command.Command);
        Assert.True(command.ShowHelp);
    }

    [Fact]
    public void GetAllArguments_WithExistingKey_ReturnsValueAsCollection()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };
        command.Arguments["key"] = "value";

        // Act
        IReadOnlyCollection<string> result = _parser.GetAllArguments(command, "key");

        // Assert
        Assert.Single(result);
        Assert.Contains("value", result);
    }

    [Fact]
    public void GetAllArguments_WithNonExistingKey_ReturnsEmptyCollection()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };
        command.Arguments["existing"] = "value";

        // Act
        IReadOnlyCollection<string> result = _parser.GetAllArguments(command, "missing");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllArguments_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.GetAllArguments(null!, "key"));
    }

    [Fact]
    public void GetAllArguments_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.GetAllArguments(command, null!));
    }

    [Fact]
    public void GetCommandHelp_WithKnownCommand_ReturnsFormattedHelp()
    {
        // Arrange
        string commandName = "evaluate";

        // Act
        string help = _parser.GetCommandHelp(commandName);

        // Assert
        Assert.Contains("Help for command: evaluate", help);
        Assert.Contains("EXAMPLES:", help);
        Assert.Contains("OPTIONS:", help);
        Assert.Contains("--key KEY\tFeature flag key to evaluate", help);
    }

    [Fact]
    public void GetCommandHelp_WithUnknownCommand_ReturnsHelpWithEmptySections()
    {
        // Arrange
        string commandName = "unknown";

        // Act
        string help = _parser.GetCommandHelp(commandName);

        // Assert
        Assert.Contains("Help for command: unknown", help);
        // Should not contain examples or options for unknown command
        Assert.DoesNotContain("EXAMPLES:", help);
        Assert.DoesNotContain("OPTIONS:", help);
    }

    [Fact]
    public void GetCommandHelp_WithNullCommandName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.GetCommandHelp(null!));
    }

    [Fact]
    public void HasAnyArguments_WithArgumentsPresent_ReturnsTrue()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };
        command.Arguments["key"] = "value";

        // Act
        bool hasArguments = _parser.HasAnyArguments(command);

        // Assert
        Assert.True(hasArguments);
    }

    [Fact]
    public void HasAnyArguments_WithNoArguments_ReturnsFalse()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };

        // Act
        bool hasArguments = _parser.HasAnyArguments(command);

        // Assert
        Assert.False(hasArguments);
    }

    [Fact]
    public void HasAnyArguments_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.HasAnyArguments(null!));
    }

    [Fact]
    public void GetArgumentCount_WithMultipleArguments_ReturnsCount()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };
        command.Arguments["key1"] = "value1";
        command.Arguments["key2"] = "value2";
        command.Arguments["key3"] = "value3";

        // Act
        int count = _parser.GetArgumentCount(command);

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void GetArgumentCount_WithNoArguments_ReturnsZero()
    {
        // Arrange
        var command = new CliCommand { Command = "test" };

        // Act
        int count = _parser.GetArgumentCount(command);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void GetArgumentCount_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.GetArgumentCount(null!));
    }
}