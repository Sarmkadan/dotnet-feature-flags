#nullable enable

namespace FeatureFlags.Models;

/// <summary>
/// Provides convenience helpers for inspecting a <see cref="UserContext"/>.
/// </summary>
public static class UserContextHelperExtensions
{
    /// <summary>
    /// Determines whether the context contains a value for the specified built-in or custom attribute.
    /// </summary>
    /// <param name="context">The user context to inspect.</param>
    /// <param name="attributeName">The name of the attribute to find.</param>
    /// <returns><see langword="true"/> when the attribute has a non-null value; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="attributeName"/> is <see langword="null"/>.</exception>
    public static bool HasAttribute(this UserContext context, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(attributeName);

        return context.GetAttribute(attributeName) is not null;
    }
}
