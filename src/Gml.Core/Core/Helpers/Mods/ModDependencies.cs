using System;
using System.Collections.Generic;
using System.Linq;

namespace Gml.Core.Helpers.Mods;

/// <summary>
///     Rules for the optional mods an optional mod needs. Mods are named by their details key (the jar file name,
///     e.g. <c>sodium-optional-mod.jar</c>); the launcher turns the dependencies on together with the mod.
/// </summary>
public static class ModDependencies
{
    /// <summary>Trims the keys and drops empty ones, the mod itself and repeats, keeping the order.</summary>
    public static IReadOnlyList<string> Normalize(string key, IEnumerable<string>? dependencies)
    {
        if (dependencies is null)
            return [];

        return dependencies
            .Select(dependency => dependency?.Trim() ?? string.Empty)
            .Where(dependency => dependency.Length > 0 && !string.Equals(dependency, key, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    ///     Checks whether giving <paramref name="key" /> the new <paramref name="dependencies" /> would let it end up
    ///     needing itself. Returns the loop (starting and ending with <paramref name="key" />) or <c>null</c>.
    /// </summary>
    public static IReadOnlyList<string>? FindCycle(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> stored,
        string key,
        IReadOnlyCollection<string> dependencies)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string> { key };

        foreach (var dependency in dependencies)
        {
            if (Walk(dependency))
                return path;
        }

        return null;

        bool Walk(string current)
        {
            path.Add(current);
            if (string.Equals(current, key, StringComparison.Ordinal))
                return true;

            if (visited.Add(current) &&
                stored.TryGetValue(current, out var next) &&
                next.Any(Walk))
                return true;

            path.RemoveAt(path.Count - 1);
            return false;
        }
    }
}
