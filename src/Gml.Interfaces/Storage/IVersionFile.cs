using System;

namespace GmlCore.Interfaces.Storage;

public interface IVersionFile : ICloneable
{
    string Version { get; set; }
    string Title { get; set; }
    string Description { get; set; }
    string Guid { get; set; }

    /// <summary>
    /// SHA-256 of the launcher executable this version points to, lowercase hex. Lets a
    /// downloading client verify the file it received matches what the server actually has
    /// before running it. Null for versions created before this field existed.
    /// </summary>
    string? Sha256 { get; set; }
}
