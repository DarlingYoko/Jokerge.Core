using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.News;

namespace GmlCore.Interfaces.Procedures;

/// <summary>News posts the admin writes in the panel (as opposed to external news listeners).</summary>
public interface IPanelNewsProcedures
{
    /// <summary>All posts, newest first.</summary>
    Task<IReadOnlyCollection<IPanelNews>> GetAllAsync();

    Task<IPanelNews?> GetAsync(int id);

    Task<IPanelNews> CreateAsync(PanelNewsDraft draft);

    /// <returns>The updated post, or null when no post has this id.</returns>
    Task<IPanelNews?> UpdateAsync(int id, PanelNewsDraft draft);

    /// <returns>False when no post has this id.</returns>
    Task<bool> DeleteAsync(int id);
}
