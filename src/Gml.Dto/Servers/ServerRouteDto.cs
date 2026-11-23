namespace Gml.Dto.Servers;

/// <summary>
///     One way into a profile's game server (the direct domain or a bridge). The launcher lets the player pick
///     one; the first route is the main address that stays in the player's server list.
/// </summary>
public class ServerRouteDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
}
