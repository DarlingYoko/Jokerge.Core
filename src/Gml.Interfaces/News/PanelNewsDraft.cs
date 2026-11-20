using System;

namespace GmlCore.Interfaces.News;

/// <summary>Editable fields of a panel news post, used to create or replace one.</summary>
public class PanelNewsDraft
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ProfileName { get; set; }
    public string? BuildVersion { get; set; }
    public DateTimeOffset Date { get; set; }
}
