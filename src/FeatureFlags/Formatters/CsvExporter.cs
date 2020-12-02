#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using FeatureFlags.Models;

namespace FeatureFlags.Formatters;

/// <summary>
/// Exports feature flags to CSV format for reporting and data analysis.
/// Provides configurable column selection and handles CSV escaping correctly.
/// </summary>
public static class CsvExporter
{
    /// <summary>
    /// Exports a collection of feature flags to CSV format string.
    /// Includes header row and properly escapes values containing commas and quotes.
    /// </summary>
    public static string ExportFeatureFlags(IEnumerable<FeatureFlag> flags, bool includeRules = false)
    {
        var sb = new StringBuilder();

        // Write header
        var headers = new[] { "Id", "Key", "DisplayName", "Description", "IsEnabled", "RolloutType", "PercentageRollout", "CreatedAt", "UpdatedAt", "CreatedBy" };
        if (includeRules)
        {
            headers = headers.Append("RulesCount").ToArray();
        }

        sb.AppendLine(string.Join(",", headers));

        // Write data rows
        foreach (var flag in flags)
        {
            var values = new[]
            {
                flag.Id.ToString(),
                EscapeCsvValue(flag.Key),
                EscapeCsvValue(flag.DisplayName),
                EscapeCsvValue(flag.Description),
                flag.IsEnabled.ToString(),
                flag.RolloutType.ToString(),
                flag.PercentageRollout?.ToString() ?? "",
                flag.CreatedAt.ToString("O"),
                flag.UpdatedAt.ToString("O"),
                EscapeCsvValue(flag.CreatedBy)
            };

            if (includeRules)
            {
                values = values.Append(flag.Rules?.Count.ToString() ?? "0").ToArray();
            }

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports audit logs to CSV format for compliance and audit trails.
    /// </summary>
    public static string ExportAuditLogs(IEnumerable<AuditLog> logs)
    {
        var sb = new StringBuilder();

        // Write header
        sb.AppendLine("Id,FeatureFlagId,Action,ChangedBy,Timestamp,OldValue,NewValue");

        // Write data rows
        foreach (var log in logs)
        {
            var values = new[]
            {
                log.Id.ToString(),
                log.FeatureFlagId.ToString(),
                log.Action.ToString(),
                EscapeCsvValue(log.ChangedBy),
                log.ChangedAt.ToString("O"),
                EscapeCsvValue(log.OldValue),
                EscapeCsvValue(log.NewValue)
            };

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Properly escapes CSV values by wrapping in quotes if needed and escaping internal quotes.
    /// </summary>
    private static string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

/// <summary>
/// Parser for converting CSV data back to FeatureFlag objects.
/// Handles quoted values and multiple data formats.
/// </summary>
public static class CsvParser
{
    /// <summary>
    /// Parses CSV string into FeatureFlag objects.
    /// First row should be header, subsequent rows are data.
    /// </summary>
    public static List<FeatureFlag> ParseFeatureFlags(string csvContent)
    {
        var flags = new List<FeatureFlag>();
        var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length < 2)
        {
            return flags;
        }

        var headers = ParseCsvLine(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            var values = ParseCsvLine(lines[i]);
            var flag = MapRowToFeatureFlag(headers, values);

            if (flag is not null)
            {
                flags.Add(flag);
            }
        }

        return flags;
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values.ToArray();
    }

    private static FeatureFlag? MapRowToFeatureFlag(string[] headers, string[] values)
    {
        if (values.Length < 3)
        {
            return null;
        }

        var flag = new FeatureFlag();

        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            var header = headers[i].Trim().ToLower();
            var value = values[i].Trim();

            switch (header)
            {
                case "id":
                    if (int.TryParse(value, out var id)) flag.Id = id;
                    break;
                case "key":
                    flag.Key = value;
                    break;
                case "displayname":
                    flag.DisplayName = value;
                    break;
                case "description":
                    flag.Description = value;
                    break;
                case "isenabled":
                    flag.IsEnabled = bool.Parse(value);
                    break;
                case "rollouttype":
                    if (Enum.TryParse<Enums.RolloutType>(value, out var rolloutType))
                    {
                        flag.RolloutType = rolloutType;
                    }
                    break;
                case "percentagerollout":
                    if (int.TryParse(value, out var percentage)) flag.PercentageRollout = percentage;
                    break;
            }
        }

        return flag.IsValid() ? flag : null;
    }
}
