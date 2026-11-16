using Taskist.Core.Common;
using Taskist.Core.Domain.Announcements;

namespace Taskist.Service.Announcements;

public interface IAnnouncementService
{
    Task<IPagedList<Announcement>> GetPagedListAsync(string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "");

    Task<Announcement> GetByIdAsync(int id);

    Task InsertAsync(Announcement announcement, int[] roleIds);

    Task UpdateAsync(Announcement announcement, int[] roleIds);

    Task DeleteAsync(Announcement announcement);

    Task<int[]> GetRoleIdsAsync(int announcementId);


    Task<IList<Announcement>> GetActiveForUserAsync(int userId);

    Task AcknowledgeAsync(int announcementId, int userId);

    Task<IList<AnnouncementAcknowledgement>> GetAcknowledgementsAsync(int announcementId);
}
