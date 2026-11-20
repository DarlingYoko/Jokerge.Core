using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gml.Core.Services.Storage;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.News;
using GmlCore.Interfaces.Procedures;
using SQLite;

namespace Gml.Core.News;

/// <summary>
///     Panel news stored in the server's SQLite file (<see cref="SqliteStorageService.DatabaseFileName" />).
///     sqlite-net pools connections by path, so this shares the connection with the storage service.
/// </summary>
public class PanelNewsProcedures : IPanelNewsProcedures
{
    private readonly SQLiteAsyncConnection _database;
    private readonly Lazy<Task> _initialization;

    public PanelNewsProcedures(IGmlSettings settings)
    {
        Directory.CreateDirectory(settings.InstallationDirectory);
        _database = new SQLiteAsyncConnection(
            Path.Combine(settings.InstallationDirectory, SqliteStorageService.DatabaseFileName));
        _initialization = new Lazy<Task>(() => _database.CreateTableAsync<PanelNewsItem>());
    }

    public async Task<IReadOnlyCollection<IPanelNews>> GetAllAsync()
    {
        await _initialization.Value;

        var items = await _database.Table<PanelNewsItem>()
            .OrderByDescending(n => n.DateUtcTicks)
            .ThenByDescending(n => n.Id)
            .ToListAsync();

        return items;
    }

    public async Task<IPanelNews?> GetAsync(int id)
    {
        await _initialization.Value;

        return await FindAsync(id);
    }

    public async Task<IPanelNews> CreateAsync(PanelNewsDraft draft)
    {
        await _initialization.Value;

        var item = new PanelNewsItem();
        Apply(item, draft);
        await _database.InsertAsync(item);

        return item;
    }

    public async Task<IPanelNews?> UpdateAsync(int id, PanelNewsDraft draft)
    {
        await _initialization.Value;

        var item = await FindAsync(id);
        if (item is null) return null;

        Apply(item, draft);
        await _database.UpdateAsync(item);

        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _initialization.Value;

        return await _database.DeleteAsync<PanelNewsItem>(id) > 0;
    }

    private Task<PanelNewsItem?> FindAsync(int id)
    {
        return _database.Table<PanelNewsItem>().FirstOrDefaultAsync(n => n.Id == id)!;
    }

    private static void Apply(PanelNewsItem item, PanelNewsDraft draft)
    {
        var title = draft.Title?.Trim();
        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("News title is required.", nameof(draft));

        item.Title = title!;
        item.Content = draft.Content ?? string.Empty;
        item.ProfileName = NullIfBlank(draft.ProfileName);
        item.BuildVersion = NullIfBlank(draft.BuildVersion);
        item.Date = draft.Date;
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
    }
}
