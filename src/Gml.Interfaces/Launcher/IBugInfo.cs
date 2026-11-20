using System;
using System.Collections.Generic;
using GmlCore.Interfaces.Sentry;

namespace GmlCore.Interfaces.Launcher;

public interface IBugInfo
{
    string Id { get; set; }
    public string? PcName { get; set; }
    public string? Username { get; set; }
    public IMemoryInfo? MemoryInfo { get; set; }
    public IEnumerable<IExceptionReport> Exceptions { get; set; }
    public DateTime SendAt { get; set; }
    public string? IpAddress { get; set; }
    public string? OsVersion { get; set; }
    public string? OsIdentifier { get; set; }

    /// <summary>Sentry release of the reporting app (launcher build), when it sent one.</summary>
    public string? Release { get; set; }
    ProjectType ProjectType { get; set; }
}
