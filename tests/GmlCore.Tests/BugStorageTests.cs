using Gml;
using Gml.Core.Launcher;
using Gml.Core.Services.Storage;
using GmlCore.Interfaces.Sentry;

namespace GmlCore.Tests;

/// <summary>
///     The error dashboard resolves one issue at a time, shows how many reports are stored and
///     opens a single report by id. These cover the storage calls behind that.
/// </summary>
public class BugStorageTests
{
    private string _baseDirectory = null!;
    private SqliteStorageService _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), "gml-bug-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_baseDirectory);
        _storage = new SqliteStorageService(new GmlSettings("GmlServer", "gfweagertghuysergfbsuyerbgiuyserg",
            _baseDirectory, new HttpClient()));
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            Directory.Delete(_baseDirectory, true);
        }
        catch (IOException)
        {
            // SQLite may still hold the file on Windows; a leftover temp folder is harmless.
        }
    }

    private static BugInfo Bug(string? release = null) => new()
    {
        Id = Guid.NewGuid().ToString(),
        SendAt = DateTime.UtcNow,
        PcName = "PC",
        Username = "user",
        MemoryInfo = new MemoryInfo(),
        Release = release,
        ProjectType = ProjectType.Launcher,
        Exceptions = new List<ExceptionReport>
        {
            new() { Type = "System.Exception", ValueData = "boom", Module = "m", StackTrace = [] }
        }
    };

    [Test]
    public async Task RemoveBugsAsync_RemovesOnlyGivenIds()
    {
        var keep = Bug();
        var drop1 = Bug();
        var drop2 = Bug();
        foreach (var bug in new[] { keep, drop1, drop2 }) await _storage.AddBugAsync(bug);

        var removed = await _storage.RemoveBugsAsync([Guid.Parse(drop1.Id), Guid.Parse(drop2.Id)]);

        var left = (await _storage.GetFilteredBugsAsync(_ => true)).Select(b => b.Id).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.EqualTo(2));
            Assert.That(left, Is.EqualTo(new[] { keep.Id }));
        });
    }

    [Test]
    public async Task CountBugsAsync_CountsStoredReports()
    {
        await _storage.AddBugAsync(Bug());
        await _storage.AddBugAsync(Bug());

        Assert.That(await _storage.CountBugsAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetBugIdAsync_UnknownId_ReturnsNull()
    {
        Assert.That(await _storage.GetBugIdAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task AddBugAsync_KeepsLauncherRelease()
    {
        var bug = Bug("Gml.Launcher@1.0.0.15");
        await _storage.AddBugAsync(bug);

        var stored = await _storage.GetBugIdAsync(Guid.Parse(bug.Id));

        Assert.That(stored?.Release, Is.EqualTo("Gml.Launcher@1.0.0.15"));
    }
}
