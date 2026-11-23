using System.Collections.Generic;

namespace Gml.Dto.Mods;

public class ModsDetailsInfoDto
{
    public string Key { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    /// <summary>Keys of the optional mods this one needs; <c>null</c> keeps the saved list.</summary>
    public List<string>? Dependencies { get; set; }
}
