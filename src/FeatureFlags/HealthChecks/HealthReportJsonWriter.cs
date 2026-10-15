#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FeatureFlags.HealthChecks;

/// <summary>
/// Writes a <see cref="HealthReport"/> as JSON, including each entry's description and data.
/// </summary>
public static class HealthReportJsonWriter
{
    /// <summary>
    /// Writes the health report to the HTTP response as JSON.
    /// </summary>
    /// <param name="context">The HTTP context to write to.</param>
    /// <param name="report">The health report.</param>
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(report);

        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration.TotalMilliseconds,
                    data = entry.Value.Data
                })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
