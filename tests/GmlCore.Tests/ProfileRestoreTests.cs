using System.Reflection;
using Gml;
using Gml.Core.Constants;
using Gml.Core.Launcher;
using Gml.Core.Helpers.Profiles;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Tests;

/// <summary>
///     Every <c>GetProfile</c>/<c>GetProfiles</c> call used to read and deserialize the whole stored
///     profiles JSON again, only to throw it away once profiles were in memory. Each whitelist
///     change makes every online launcher re-request the profile list, so the panel and API slowed
///     down the more players were added.
/// </summary>
public class ProfileRestoreTests
{
    private string _baseDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), "gml-restore-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_baseDirectory);
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

    [Test]
    public async Task GetProfiles_ReadsStoredProfilesOnlyOnce()
    {
        var settings = new GmlSettings("GmlServer", "gfweagertghuysergfbsuyerbgiuyserg", _baseDirectory,
            new HttpClient())
        {
            TextureServiceEndpoint = "http://gml-web-skins:8085"
        };
        var manager = new GmlManager(settings);
        var storage = CountingStorage.Wrap(manager.Storage, out var counter);
        var procedures = new ProfileProcedures(manager.LauncherInfo, storage, manager.Notifications,
            manager.BugTracker, manager);

        await procedures.GetProfiles();
        await procedures.GetProfiles();
        await procedures.GetProfile("missing");
        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => procedures.GetProfiles()));

        Assert.That(counter.ProfileReads, Is.EqualTo(1));
    }

    public class Counter
    {
        public int ProfileReads;
    }

    public class CountingStorage : DispatchProxy
    {
        private Counter _counter = null!;
        private IStorageService _inner = null!;

        public static IStorageService Wrap(IStorageService inner, out Counter counter)
        {
            var proxy = Create<IStorageService, CountingStorage>();
            var self = (CountingStorage)(object)proxy;
            self._inner = inner;
            self._counter = counter = new Counter();
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name == nameof(IStorageService.GetAsync) &&
                args?.FirstOrDefault() as string == StorageConstants.GameProfiles)
                Interlocked.Increment(ref _counter.ProfileReads);

            return targetMethod.Invoke(_inner, args);
        }
    }
}
