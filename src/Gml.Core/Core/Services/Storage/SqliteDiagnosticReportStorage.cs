using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Storage;
using SQLite;

namespace Gml.Core.Services.Storage;

/// <summary>
///     Diagnostics reports live in the same SQLite file as the rest of the Core data
///     (<see cref="SqliteStorageService" />), in their own table.
/// </summary>
public class SqliteDiagnosticReportStorage : IDiagnosticReportStorage
{
    private readonly SQLiteAsyncConnection _database;

    public SqliteDiagnosticReportStorage(IGmlSettings settings)
    {
        var databasePath = Path.Combine(settings.InstallationDirectory, SqliteStorageService.DatabaseFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        _database = new SQLiteAsyncConnection(databasePath);
        _database.CreateTableAsync<DiagnosticReportItem>().Wait();
    }

    public async Task AddDiagnosticReportAsync(DiagnosticReportRecord record, int keepPerUser)
    {
        var key = NameKey(record.UserName);

        await _database.InsertAsync(new DiagnosticReportItem
        {
            Guid = record.Id.ToString(),
            UserName = record.UserName,
            UserNameKey = key,
            CreatedAtTicks = record.CreatedAt.ToUniversalTime().Ticks,
            Summary = record.Summary,
            Payload = record.Payload
        });

        await _database.ExecuteAsync(
            $"DELETE FROM {nameof(DiagnosticReportItem)} WHERE UserNameKey = ? AND Id NOT IN " +
            $"(SELECT Id FROM {nameof(DiagnosticReportItem)} WHERE UserNameKey = ? " +
            "ORDER BY CreatedAtTicks DESC, Id DESC LIMIT ?)",
            key, key, Math.Max(1, keepPerUser));
    }

    public async Task<DiagnosticReportPage> GetDiagnosticReportsAsync(int take, int offset, string? userName)
    {
        var filter = string.IsNullOrWhiteSpace(userName)
            ? string.Empty
            : " WHERE UserNameKey LIKE ? ESCAPE '\\'";
        var filterArgs = string.IsNullOrWhiteSpace(userName)
            ? Array.Empty<object>()
            : new object[] { "%" + EscapeLike(NameKey(userName!.Trim())) + "%" };

        var total = await _database.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {nameof(DiagnosticReportItem)}{filter}", filterArgs);

        var rows = await _database.QueryAsync<DiagnosticReportItem>(
            $"SELECT Id, Guid, UserName, UserNameKey, CreatedAtTicks, Summary, '' AS Payload " +
            $"FROM {nameof(DiagnosticReportItem)}{filter} " +
            "ORDER BY CreatedAtTicks DESC, Id DESC LIMIT ? OFFSET ?",
            filterArgs.Concat(new object[] { Math.Max(0, take), Math.Max(0, offset) }).ToArray());

        var items = rows
            .Select(r => new DiagnosticReportSummaryRecord(global::System.Guid.Parse(r.Guid), r.UserName,
                FromTicks(r.CreatedAtTicks), r.Summary))
            .ToList();

        return new DiagnosticReportPage(items, total);
    }

    public async Task<DiagnosticReportRecord?> GetDiagnosticReportAsync(Guid id)
    {
        var key = id.ToString();
        var row = await _database.Table<DiagnosticReportItem>()
            .Where(r => r.Guid == key)
            .FirstOrDefaultAsync();

        return row is null
            ? null
            : new DiagnosticReportRecord(id, row.UserName, FromTicks(row.CreatedAtTicks), row.Summary, row.Payload);
    }

    public async Task<bool> RemoveDiagnosticReportAsync(Guid id)
    {
        var key = id.ToString();
        return await _database.Table<DiagnosticReportItem>().DeleteAsync(r => r.Guid == key) > 0;
    }

    private static string NameKey(string userName) => userName.ToLowerInvariant();

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static DateTime FromTicks(long ticks) => new(ticks, DateTimeKind.Utc);

    public class DiagnosticReportItem
    {
        [PrimaryKey] [AutoIncrement] public int Id { get; set; }
        [Indexed] public string Guid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        [Indexed] public string UserNameKey { get; set; } = null!;
        [Indexed] public long CreatedAtTicks { get; set; }
        public string Summary { get; set; } = null!;
        public string Payload { get; set; } = null!;
    }
}
