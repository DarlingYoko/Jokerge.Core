using Gml.Core.Helpers.Mods;

namespace GmlCore.Tests;

/// <summary>
///     An optional mod can list other optional mods it needs; the launcher turns those on with it. The panel
///     must not be able to save a list the launcher could loop on.
/// </summary>
public class ModDependenciesTests
{
    private static IReadOnlyDictionary<string, IReadOnlyCollection<string>> Graph(
        params (string Key, string[] Dependencies)[] entries) =>
        entries.ToDictionary(e => e.Key, e => (IReadOnlyCollection<string>)e.Dependencies);

    [Test]
    public void Normalize_TrimsAndDropsEmptySelfAndDuplicates()
    {
        var result = ModDependencies.Normalize("a-optional-mod.jar",
            [" b-optional-mod.jar ", "", "a-optional-mod.jar", "b-optional-mod.jar", "  ", "c-optional-mod.jar"]);

        Assert.That(result, Is.EqualTo(new[] { "b-optional-mod.jar", "c-optional-mod.jar" }));
    }

    [Test]
    public void Normalize_NullGivesEmptyList()
    {
        Assert.That(ModDependencies.Normalize("a.jar", null), Is.Empty);
    }

    [Test]
    public void FindCycle_ReturnsNull_WhenTheGraphStaysAcyclic()
    {
        var graph = Graph(("b.jar", ["c.jar"]), ("c.jar", []));

        Assert.That(ModDependencies.FindCycle(graph, "a.jar", ["b.jar", "c.jar"]), Is.Null);
    }

    [Test]
    public void FindCycle_FindsTwoModsNeedingEachOther()
    {
        var graph = Graph(("b.jar", ["a.jar"]));

        Assert.That(ModDependencies.FindCycle(graph, "a.jar", ["b.jar"]),
            Is.EqualTo(new[] { "a.jar", "b.jar", "a.jar" }));
    }

    [Test]
    public void FindCycle_FindsLongerCycle()
    {
        var graph = Graph(("b.jar", ["c.jar"]), ("c.jar", ["a.jar"]));

        Assert.That(ModDependencies.FindCycle(graph, "a.jar", ["b.jar"]),
            Is.EqualTo(new[] { "a.jar", "b.jar", "c.jar", "a.jar" }));
    }

    [Test]
    public void FindCycle_UsesTheNewListInsteadOfTheStoredOne()
    {
        // a used to need b; saving a without b must not report the old edge.
        var graph = Graph(("a.jar", ["b.jar"]), ("b.jar", ["a.jar"]));

        Assert.That(ModDependencies.FindCycle(graph, "a.jar", []), Is.Null);
    }

    [Test]
    public void FindCycle_IgnoresACycleThatDoesNotPassThroughTheSavedMod()
    {
        // Stored data from before this check could already loop; that must not hang or block unrelated saves.
        var graph = Graph(("x.jar", ["y.jar"]), ("y.jar", ["x.jar"]));

        Assert.That(ModDependencies.FindCycle(graph, "a.jar", ["x.jar"]), Is.Null);
    }
}
