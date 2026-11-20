using System;
using GmlCore.Interfaces.News;
using SQLite;

namespace Gml.Core.News;

/// <summary>SQLite row of a panel news post. The date is kept as UTC ticks plus the author's offset.</summary>
[Table("PanelNews")]
public class PanelNewsItem : IPanelNews
{
    [PrimaryKey] [AutoIncrement] public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ProfileName { get; set; }
    public string? BuildVersion { get; set; }
    [Indexed] public long DateUtcTicks { get; set; }
    public int DateOffsetMinutes { get; set; }

    [Ignore]
    public DateTimeOffset Date
    {
        get => new DateTimeOffset(DateUtcTicks, TimeSpan.Zero).ToOffset(TimeSpan.FromMinutes(DateOffsetMinutes));
        set
        {
            DateUtcTicks = value.UtcTicks;
            DateOffsetMinutes = (int)value.Offset.TotalMinutes;
        }
    }
}
