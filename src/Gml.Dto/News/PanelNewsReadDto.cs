using System;

namespace Gml.Dto.News;

/// <summary>A panel news post as the admin panel sees it; <see cref="Content" /> is raw Markdown.</summary>
public class PanelNewsReadDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ProfileName { get; set; }
    public string? BuildVersion { get; set; }
    public DateTimeOffset Date { get; set; }
}
