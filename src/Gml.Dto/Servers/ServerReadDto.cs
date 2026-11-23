namespace Gml.Dto.Servers;

public class ServerReadDto
{
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public string Version { get; set; }
    public bool IsOnline { get; set; }
    public int? Online { get; set; }
    public int? MaxOnline { get; set; }

    /// <summary>Status request round trip from the API host, in ms; null while offline or not checked yet.</summary>
    public int? Ping { get; set; }
}
