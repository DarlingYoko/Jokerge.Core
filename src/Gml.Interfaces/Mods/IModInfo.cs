using System.Collections.Generic;

namespace GmlCore.Interfaces.Mods;

public interface IModInfo
{
    string Key { get; set; }
    string Title { get; set; }
    string Description { get; set; }

    /// <summary>Keys of the optional mods this one needs; the launcher turns them on together with it.</summary>
    List<string> Dependencies { get; set; }
}
