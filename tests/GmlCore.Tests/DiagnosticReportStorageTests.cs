using Gml.Core.Launcher;
using Gml.Core.Services.Storage;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Tests;

/// <summary>
///     Players send a diagnostics report from the launcher settings; the panel lists them newest
///     first, filters by nickname and opens one by id. Storage keeps only the last few per player.
/// </summary>
public class DiagnosticReportStorageTests
{
    private string _baseDirectory = null!;
    private SqliteDiagnosticReportStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), "gml-diag-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_baseDirectory);
        _storage = new SqliteDiagnosticReportStorage(new GmlSettings("GmlServer",
            "gfweagertghuysergfbsuyerbgiuyserg", _baseDirectory, new HttpClient()));
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

    private static DiagnosticReportRecord Report(string user, DateTime createdAt, string payload = "{}") =>
        new(Guid.NewGuid(), user, createdAt, "{\"s\":1}", payload);

    private static readonly DateTime T0 = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task Add_ThenGetById_ReturnsPayload()
    {
        var report = Report("yoko", T0, "{\"text\":\"hello\"}");

        await _storage.AddDiagnosticReportAsync(report, keepPerUser: 10);
        var loaded = await _storage.GetDiagnosticReportAsync(report.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(loaded!.UserName, Is.EqualTo("yoko"));
            Assert.That(loaded.Payload, Is.EqualTo("{\"text\":\"hello\"}"));
            Assert.That(loaded.Summary, Is.EqualTo("{\"s\":1}"));
            Assert.That(loaded.CreatedAt, Is.EqualTo(T0));
            Assert.That(loaded.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    public async Task GetById_Unknown_ReturnsNull()
    {
        Assert.That(await _storage.GetDiagnosticReportAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task List_IsNewestFirst_WithTotalAndPaging()
    {
        for (var i = 0; i < 5; i++)
            await _storage.AddDiagnosticReportAsync(Report("user" + i, T0.AddMinutes(i)), keepPerUser: 10);

        var page = await _storage.GetDiagnosticReportsAsync(take: 2, offset: 1, userName: null);

        Assert.Multiple(() =>
        {
            Assert.That(page.Total, Is.EqualTo(5));
            Assert.That(page.Items.Select(x => x.UserName), Is.EqualTo(new[] { "user3", "user2" }));
        });
    }

    [Test]
    public async Task List_FiltersByNicknameSubstring_CaseInsensitive()
    {
        await _storage.AddDiagnosticReportAsync(Report("DarlingYoko", T0), keepPerUser: 10);
        await _storage.AddDiagnosticReportAsync(Report("vipig", T0.AddMinutes(1)), keepPerUser: 10);

        var page = await _storage.GetDiagnosticReportsAsync(take: 20, offset: 0, userName: "yoko");

        Assert.Multiple(() =>
        {
            Assert.That(page.Total, Is.EqualTo(1));
            Assert.That(page.Items.Single().UserName, Is.EqualTo("DarlingYoko"));
        });
    }

    [Test]
    public async Task Add_KeepsOnlyNewestPerUser()
    {
        var reports = Enumerable.Range(0, 4).Select(i => Report("yoko", T0.AddMinutes(i))).ToList();
        foreach (var report in reports) await _storage.AddDiagnosticReportAsync(report, keepPerUser: 3);
        await _storage.AddDiagnosticReportAsync(Report("other", T0), keepPerUser: 3);

        var page = await _storage.GetDiagnosticReportsAsync(take: 20, offset: 0, userName: null);

        Assert.Multiple(() =>
        {
            Assert.That(page.Total, Is.EqualTo(4));
            Assert.That(page.Items.Select(x => x.Id), Does.Not.Contain(reports[0].Id));
            Assert.That(page.Items.Select(x => x.UserName), Does.Contain("other"));
        });
    }

    [Test]
    public async Task Remove_DeletesOnlyThatReport()
    {
        var keep = Report("yoko", T0);
        var drop = Report("yoko", T0.AddMinutes(1));
        await _storage.AddDiagnosticReportAsync(keep, keepPerUser: 10);
        await _storage.AddDiagnosticReportAsync(drop, keepPerUser: 10);

        Assert.That(await _storage.RemoveDiagnosticReportAsync(drop.Id), Is.True);
        Assert.That(await _storage.RemoveDiagnosticReportAsync(drop.Id), Is.False);

        var page = await _storage.GetDiagnosticReportsAsync(take: 20, offset: 0, userName: null);
        Assert.That(page.Items.Select(x => x.Id), Is.EqualTo(new[] { keep.Id }));
    }
}
