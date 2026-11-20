using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GmlCore.Interfaces.Storage;

/// <summary>
///     Stores the diagnostics reports players send from the launcher. The payload and summary are
///     opaque JSON owned by the API, so the storage schema does not change with the report format.
/// </summary>
public interface IDiagnosticReportStorage
{
    /// <summary>Adds a report and drops the player's older reports beyond <paramref name="keepPerUser" />.</summary>
    Task AddDiagnosticReportAsync(DiagnosticReportRecord record, int keepPerUser);

    /// <summary>Newest first; <paramref name="userName" /> is a case-insensitive substring filter.</summary>
    Task<DiagnosticReportPage> GetDiagnosticReportsAsync(int take, int offset, string? userName);

    Task<DiagnosticReportRecord?> GetDiagnosticReportAsync(Guid id);

    /// <returns>Whether a report with that id existed.</returns>
    Task<bool> RemoveDiagnosticReportAsync(Guid id);
}

public class DiagnosticReportRecord
{
    public DiagnosticReportRecord(Guid id, string userName, DateTime createdAt, string summary, string payload)
    {
        Id = id;
        UserName = userName;
        CreatedAt = createdAt;
        Summary = summary;
        Payload = payload;
    }

    public Guid Id { get; }
    public string UserName { get; }

    /// <summary>UTC.</summary>
    public DateTime CreatedAt { get; }

    /// <summary>Small JSON shown in the list without loading the payload.</summary>
    public string Summary { get; }

    public string Payload { get; }
}

public class DiagnosticReportSummaryRecord
{
    public DiagnosticReportSummaryRecord(Guid id, string userName, DateTime createdAt, string summary)
    {
        Id = id;
        UserName = userName;
        CreatedAt = createdAt;
        Summary = summary;
    }

    public Guid Id { get; }
    public string UserName { get; }
    public DateTime CreatedAt { get; }
    public string Summary { get; }
}

public class DiagnosticReportPage
{
    public DiagnosticReportPage(IReadOnlyList<DiagnosticReportSummaryRecord> items, int total)
    {
        Items = items;
        Total = total;
    }

    public IReadOnlyList<DiagnosticReportSummaryRecord> Items { get; }
    public int Total { get; }
}
