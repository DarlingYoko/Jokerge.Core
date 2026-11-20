using Gml;
using Gml.Core.Launcher;
using Gml.Core.News;
using GmlCore.Interfaces.News;

namespace GmlCore.Tests;

/// <summary>
///     News written by the admin in the panel live in the same SQLite file as the rest of the
///     server state and are shown in the launcher, newest first.
/// </summary>
public class PanelNewsProceduresTests
{
    private string _baseDirectory = null!;
    private PanelNewsProcedures _news = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), "gml-news-tests-" + Guid.NewGuid().ToString("N"));
        _news = new PanelNewsProcedures(new GmlSettings("GmlServer", "gfweagertghuysergfbsuyerbgiuyserg",
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

    private static PanelNewsDraft Draft(string title = "Обновление", DateTimeOffset? date = null,
        string? profile = null, string? version = null) => new()
    {
        Title = title,
        Content = "**Новое**",
        ProfileName = profile,
        BuildVersion = version,
        Date = date ?? DateTimeOffset.UtcNow
    };

    [Test]
    public async Task CreateAsync_StoresNewsAndAssignsId()
    {
        var date = new DateTimeOffset(2026, 11, 1, 12, 30, 0, TimeSpan.FromHours(3));

        var created = await _news.CreateAsync(Draft("Вайп", date, "Survival", "1.2.0"));
        var stored = await _news.GetAsync(created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.Title, Is.EqualTo("Вайп"));
            Assert.That(stored.Content, Is.EqualTo("**Новое**"));
            Assert.That(stored.ProfileName, Is.EqualTo("Survival"));
            Assert.That(stored.BuildVersion, Is.EqualTo("1.2.0"));
            Assert.That(stored.Date, Is.EqualTo(date));
        });
    }

    [Test]
    public async Task CreateAsync_TrimsFieldsAndDropsBlankBindings()
    {
        var draft = Draft("  Вайп  ", profile: "   ", version: " 1.3 ");

        var created = await _news.CreateAsync(draft);

        Assert.Multiple(() =>
        {
            Assert.That(created.Title, Is.EqualTo("Вайп"));
            Assert.That(created.ProfileName, Is.Null);
            Assert.That(created.BuildVersion, Is.EqualTo("1.3"));
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_BlankTitle_Throws(string title)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _news.CreateAsync(Draft(title)));
    }

    [Test]
    public async Task GetAllAsync_ReturnsNewestFirst()
    {
        var now = DateTimeOffset.UtcNow;
        await _news.CreateAsync(Draft("old", now.AddDays(-2)));
        await _news.CreateAsync(Draft("new", now));
        await _news.CreateAsync(Draft("mid", now.AddDays(-1)));

        var all = await _news.GetAllAsync();

        Assert.That(all.Select(n => n.Title), Is.EqualTo(new[] { "new", "mid", "old" }));
    }

    [Test]
    public async Task UpdateAsync_ChangesStoredFields()
    {
        var created = await _news.CreateAsync(Draft("Черновик", profile: "Survival"));
        var newDate = new DateTimeOffset(2026, 12, 31, 23, 0, 0, TimeSpan.Zero);

        var updated = await _news.UpdateAsync(created.Id, Draft("Готово", newDate));
        var stored = await _news.GetAsync(created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.Not.Null);
            Assert.That(stored!.Title, Is.EqualTo("Готово"));
            Assert.That(stored.ProfileName, Is.Null);
            Assert.That(stored.Date, Is.EqualTo(newDate));
        });
    }

    [Test]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        Assert.That(await _news.UpdateAsync(12345, Draft()), Is.Null);
    }

    [Test]
    public async Task DeleteAsync_RemovesOnlyThatNews()
    {
        var keep = await _news.CreateAsync(Draft("keep"));
        var drop = await _news.CreateAsync(Draft("drop"));

        var removed = await _news.DeleteAsync(drop.Id);
        var left = await _news.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.True);
            Assert.That(left.Select(n => n.Id), Is.EqualTo(new[] { keep.Id }));
        });
    }

    [Test]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        Assert.That(await _news.DeleteAsync(12345), Is.False);
    }
}
