using System;

namespace GmlCore.Interfaces.News;

/// <summary>
///     A news post written by the admin in the panel. <see cref="Content" /> is Markdown.
/// </summary>
public interface IPanelNews
{
    int Id { get; }
    string Title { get; }
    string Content { get; }

    /// <summary>Optional game profile the post is about; null means it concerns everyone.</summary>
    string? ProfileName { get; }

    /// <summary>Optional modpack build version the post is about, free text (e.g. "1.2.0").</summary>
    string? BuildVersion { get; }

    DateTimeOffset Date { get; }
}
