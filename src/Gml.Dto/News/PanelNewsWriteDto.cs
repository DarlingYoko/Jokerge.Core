using System;

namespace Gml.Dto.News;

/// <summary>Body of create/update requests for a panel news post.</summary>
public class PanelNewsWriteDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ProfileName { get; set; }
    public string? BuildVersion { get; set; }

    /// <summary>Publication date; null means "now".</summary>
    public DateTimeOffset? Date { get; set; }
}
