#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml;
using System.Xml.Linq;
using FeatureFlags.Models;

namespace FeatureFlags.Formatters;

/// <summary>
/// Exports feature flags to XML format for system integration and data exchange.
/// Produces properly formatted XML that can be imported into other systems.
/// </summary>
public static class XmlExporter
{
    /// <summary>
    /// Exports feature flags collection to XML document string with proper formatting.
    /// </summary>
    public static string ExportFeatureFlags(IEnumerable<FeatureFlag> flags, bool pretty = true)
    {
        var root = new XElement("FeatureFlags");

        foreach (var flag in flags)
        {
            var flagElement = new XElement("FeatureFlag",
                new XAttribute("id", flag.Id),
                new XAttribute("key", flag.Key),
                new XElement("DisplayName", flag.DisplayName),
                new XElement("Description", flag.Description),
                new XElement("IsEnabled", flag.IsEnabled),
                new XElement("RolloutType", flag.RolloutType),
                new XElement("PercentageRollout", flag.PercentageRollout ?? -1),
                new XElement("CreatedAt", flag.CreatedAt.ToString("O")),
                new XElement("UpdatedAt", flag.UpdatedAt.ToString("O")),
                new XElement("CreatedBy", flag.CreatedBy),
                new XElement("RulesCount", flag.Rules?.Count ?? 0),
                new XElement("VariantsCount", flag.Variants?.Count ?? 0)
            );

            root.Add(flagElement);
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            root);

        using var writer = new System.IO.StringWriter();
        var settings = new XmlWriterSettings
        {
            Indent = pretty,
            IndentChars = "  ",
            Encoding = System.Text.Encoding.UTF8,
            ConformanceLevel = ConformanceLevel.Document
        };

        using (var xmlWriter = XmlWriter.Create(writer, settings))
        {
            doc.WriteTo(xmlWriter);
        }

        return writer.ToString();
    }

    /// <summary>
    /// Exports audit logs to XML format for compliance reporting.
    /// </summary>
    public static string ExportAuditLogs(IEnumerable<AuditLog> logs, bool pretty = true)
    {
        var root = new XElement("AuditLogs");

        foreach (var log in logs)
        {
            var logElement = new XElement("AuditLog",
                new XAttribute("id", log.Id),
                new XElement("FeatureFlagId", log.FeatureFlagId),
                new XElement("Action", log.Action),
                new XElement("ChangedBy", log.ChangedBy),
                new XElement("Timestamp", log.ChangedAt.ToString("O")),
                new XElement("OldValue", log.OldValue ?? string.Empty),
                new XElement("NewValue", log.NewValue ?? string.Empty)
            );

            root.Add(logElement);
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            root);

        using var writer = new System.IO.StringWriter();
        var settings = new XmlWriterSettings
        {
            Indent = pretty,
            IndentChars = "  "
        };

        using (var xmlWriter = XmlWriter.Create(writer, settings))
        {
            doc.WriteTo(xmlWriter);
        }

        return writer.ToString();
    }

    /// <summary>
    /// Exports rules to XML format for backup and analysis.
    /// </summary>
    public static string ExportRules(IEnumerable<Rule> rules, bool pretty = true)
    {
        var root = new XElement("Rules");

        foreach (var rule in rules)
        {
            var ruleElement = new XElement("Rule",
                new XAttribute("id", rule.Id),
                new XElement("Name", rule.Name),
                new XElement("Priority", rule.Priority),
                new XElement("IsActive", rule.IsActive),
                new XElement("ConditionLogic", rule.ConditionLogic),
                new XElement("ConditionCount", rule.Conditions?.Count ?? 0)
            );

            root.Add(ruleElement);
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            root);

        using var writer = new System.IO.StringWriter();
        var settings = new XmlWriterSettings { Indent = pretty };

        using (var xmlWriter = XmlWriter.Create(writer, settings))
        {
            doc.WriteTo(xmlWriter);
        }

        return writer.ToString();
    }
}

/// <summary>
/// Parser for converting XML data back to FeatureFlag objects.
/// Supports the XML format produced by XmlExporter.
/// </summary>
public static class XmlParser
{
    /// <summary>
    /// Parses XML string into FeatureFlag objects.
    /// </summary>
    public static List<FeatureFlag> ParseFeatureFlags(string xmlContent)
    {
        var flags = new List<FeatureFlag>();

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var root = doc.Root;

            if (root?.Name.LocalName != "FeatureFlags")
            {
                return flags;
            }

            foreach (var element in root.Elements("FeatureFlag"))
            {
                var flag = new FeatureFlag
                {
                    Id = int.Parse(element.Attribute("id")?.Value ?? "0"),
                    Key = element.Attribute("key")?.Value ?? string.Empty,
                    DisplayName = element.Element("DisplayName")?.Value ?? string.Empty,
                    Description = element.Element("Description")?.Value ?? string.Empty,
                    IsEnabled = bool.Parse(element.Element("IsEnabled")?.Value ?? "false"),
                    RolloutType = Enum.Parse<Enums.RolloutType>(element.Element("RolloutType")?.Value ?? "Percentage"),
                    CreatedAt = DateTime.Parse(element.Element("CreatedAt")?.Value ?? DateTime.UtcNow.ToString("O")),
                    UpdatedAt = DateTime.Parse(element.Element("UpdatedAt")?.Value ?? DateTime.UtcNow.ToString("O")),
                    CreatedBy = element.Element("CreatedBy")?.Value ?? string.Empty
                };

                var percentageElement = element.Element("PercentageRollout");
                if (percentageElement is not null && int.TryParse(percentageElement.Value, out var percentage) && percentage >= 0)
                {
                    flag.PercentageRollout = percentage;
                }

                if (flag.IsValid())
                {
                    flags.Add(flag);
                }
            }
        }
        catch
        {
            // Invalid XML - return empty list
        }

        return flags;
    }
}
