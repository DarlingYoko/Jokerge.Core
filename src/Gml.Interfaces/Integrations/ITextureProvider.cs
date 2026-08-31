using System.IO;
using System.Threading.Tasks;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Integrations;

public interface ITextureProvider
{
    Task<string> SetSkin(IUser user, string skinUrl);
    Task<string> SetCloak(IUser user, string skinUrl);
    Task<Stream> GetSkinStream(string? textureUrl);
    Task<Stream> GetCloakStream(string? userTextureSkinUrl);
    Task<Stream> GetHeadByNameStream(string? userName);

    /// <summary>
    /// Looks up a player's current texture info without uploading anything — Skin.Service
    /// resolves this to their custom skin/cloak if set, or transparently falls back to their
    /// cached official Mojang skin (or the bundled default) otherwise.
    /// </summary>
    Task<(string? SkinUrl, bool HasSkin, string? CloakUrl, bool HasCloak)> GetUserTexture(string userName);
}
