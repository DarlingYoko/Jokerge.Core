using Gml;
using Gml.Core.Launcher;
using Gml.Models.User;
using GmlCore.Interfaces;
using GmlCore.Interfaces.User;

namespace GmlCore.Tests;

/// <summary>
///     Users are stored by login, but the game identifies them by UUID. When a player renamed
///     themselves on the auth site, the integration kept reporting the same UUID and login created a
///     second row with it. Lookups by UUID (join, hasJoined, profiles) kept finding the old row with
///     the old access token, so the player got "authentication servers are unavailable" on every join.
/// </summary>
public class UserRenameTests
{
    private const string PlayerUuid = "49A32560-AFF8-4F07-BAD7-5252F253D1E3";

    private string _baseDirectory = null!;
    private IGmlManager _gmlManager = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), "gml-rename-tests-" + Guid.NewGuid().ToString("N"));
        _gmlManager = new GmlManager(new GmlSettings("GmlServer", "gfweagertghuysergfbsuyerbgiuyserg",
            _baseDirectory, new HttpClient())
        {
            TextureServiceEndpoint = "http://gml-web-skins:8085"
        });
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
    public async Task GetAuthData_RenamedPlayer_LookupByUuidFindsNewName()
    {
        await Login("HyundaiAccentTagaz2006", PlayerUuid);

        IUser renamed = await Login("HyundaiAccentTaa", PlayerUuid);
        IUser? byUuid = await _gmlManager.Users.GetUserByUuid(PlayerUuid);

        Assert.Multiple(() =>
        {
            Assert.That(renamed.Name, Is.EqualTo("HyundaiAccentTaa"));
            Assert.That(byUuid?.Name, Is.EqualTo("HyundaiAccentTaa"));
        });
    }

    [Test]
    public async Task GetAuthData_RenamedPlayer_OldNameIsGone()
    {
        await Login("HyundaiAccentTagaz2006", PlayerUuid);
        await Login("HyundaiAccentTaa", PlayerUuid);

        IUser? oldName = await _gmlManager.Users.GetUserByName("HyundaiAccentTagaz2006");

        Assert.That(oldName, Is.Null);
    }

    [Test]
    public async Task GetAuthData_RenamedPlayer_KeepsStoredData()
    {
        IUser original = await Login("HyundaiAccentTagaz2006", PlayerUuid);
        original.TextureSkinGuid = "skin-guid";
        await _gmlManager.Users.UpdateUser(original);

        IUser renamed = await Login("HyundaiAccentTaa", PlayerUuid);

        Assert.Multiple(() =>
        {
            Assert.That(renamed.TextureSkinGuid, Is.EqualTo("skin-guid"));
            Assert.That(((AuthUser)renamed).AuthHistory, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetAuthData_RenamedPlayer_StaysBanned()
    {
        IUser original = await Login("HyundaiAccentTagaz2006", PlayerUuid);
        original.IsBanned = true;
        await _gmlManager.Users.UpdateUser(original);

        IUser renamed = await Login("HyundaiAccentTaa", PlayerUuid);

        Assert.That(renamed.IsBanned, Is.True);
    }

    [Test]
    public async Task GetAuthData_ExistingDuplicateRows_AreCollapsedOnNextLogin()
    {
        // Rows duplicated before the fix: both logins already exist with the same UUID.
        IUser stale = await Login("HyundaiAccentTagaz2006", PlayerUuid);
        stale.IsBanned = true;
        await _gmlManager.Users.UpdateUser(stale);
        await _gmlManager.Storage.SetUserAsync("HyundaiAccentTaa", PlayerUuid,
            new AuthUser { Name = "HyundaiAccentTaa", Uuid = PlayerUuid });

        IUser current = await Login("HyundaiAccentTaa", PlayerUuid);
        IUser? byUuid = await _gmlManager.Users.GetUserByUuid(PlayerUuid);
        IUser? oldName = await _gmlManager.Users.GetUserByName("HyundaiAccentTagaz2006");

        Assert.Multiple(() =>
        {
            Assert.That(byUuid?.Name, Is.EqualTo("HyundaiAccentTaa"));
            Assert.That(oldName, Is.Null);
            Assert.That(current.IsBanned, Is.True);
        });
    }

    [Test]
    public async Task GetAuthData_WithoutCustomUuid_DoesNotMergeDifferentPlayers()
    {
        await Login("PlayerOne", null);
        await Login("PlayerTwo", null);

        Assert.Multiple(async () =>
        {
            Assert.That(await _gmlManager.Users.GetUserByName("PlayerOne"), Is.Not.Null);
            Assert.That(await _gmlManager.Users.GetUserByName("PlayerTwo"), Is.Not.Null);
        });
    }

    private Task<IUser> Login(string login, string? uuid)
    {
        return _gmlManager.Users.GetAuthData(login, "password", "Desktop", "HTTP/1.1", null, uuid, null, false);
    }
}
