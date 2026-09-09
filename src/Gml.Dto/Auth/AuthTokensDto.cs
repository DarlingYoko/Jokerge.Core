using System.Collections.Generic;

namespace Gml.Dto.Auth;

public class AuthTokensDto
{
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The same claims encoded in AccessToken, as plain JSON — lets a browser client that
    /// receives the token only as an httpOnly cookie (and so can never decode the JWT itself)
    /// still read the user's name/email/role/permissions/expiry.
    /// </summary>
    public Dictionary<string, object>? Profile { get; set; }
}
