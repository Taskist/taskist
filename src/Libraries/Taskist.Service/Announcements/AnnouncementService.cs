using Microsoft.EntityFrameworkCore;
using Taskist.Core.Common;
using Taskist.Core.Domain.Announcements;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Users;

namespace Taskist.Service.Announcements;

public class AnnouncementService : IAnnouncementService
{
    #region Fields

    private readonly IRepository<Announcement> _announcementRepository;
    private readonly IRepository<AnnouncementAcknowledgement> _ackRepository;
    private readonly IRepository<AnnouncementRole> _roleRepository;
    private readonly IRepository<UserRoleMap> _roleMapRepository;
    private readonly IUserService _userService;

    #endregion

    #region Ctor

    public AnnouncementService(IRepository<Announcement> announcementRepository,
        IRepository<AnnouncementAcknowledgement> ackRepository,
        IRepository<AnnouncementRole> roleRepository,
        IRepository<UserRoleMap> roleMapRepository,
        IUserService userService)
    {
        _announcementRepository = announcementRepository;
        _ackRepository = ackRepository;
        _roleRepository = roleRepository;
        _roleMapRepository = roleMapRepository;
        _userService = userService;
    }

    #endregion

    #region CRUD

    public async Task<IPagedList<Announcement>> GetPagedListAsync(string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _announcementRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Title.Contains(search));

            query = query.ApplySafeSort(sortColumn, sortDirection);

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<Announcement> GetByIdAsync(int id)
        => await _announcementRepository.GetByIdAsync(id);

    public async Task InsertAsync(Announcement announcement, int[] roleIds)
    {
        await _announcementRepository.InsertAsync(announcement);
        await SaveTargetsAsync(announcement.Id, announcement.Audience, roleIds);
    }

    public async Task UpdateAsync(Announcement announcement, int[] roleIds)
    {
        await _announcementRepository.UpdateAsync(announcement);
        await SaveTargetsAsync(announcement.Id, announcement.Audience, roleIds);
    }

    public async Task DeleteAsync(Announcement announcement)
        => await _announcementRepository.DeleteAsync(announcement);

    private async Task SaveTargetsAsync(int announcementId, AnnouncementAudience audience, int[] roleIds)
    {
        var existingRoles = await _roleRepository.GetAllAsync(q => q.Where(x => x.AnnouncementId == announcementId));
        if (existingRoles.Count > 0)
            await _roleRepository.DeleteAsync(existingRoles);

        if (audience == AnnouncementAudience.Roles && roleIds is { Length: > 0 })
            await _roleRepository.InsertAsync(roleIds.Distinct()
                .Select(r => new AnnouncementRole { AnnouncementId = announcementId, UserRoleId = r }).ToList());
    }

    public async Task<int[]> GetRoleIdsAsync(int announcementId)
        => await _roleRepository.Table.Where(x => x.AnnouncementId == announcementId).Select(x => x.UserRoleId).ToArrayAsync();

    #endregion

    #region Display / acknowledgement

    public async Task<IList<Announcement>> GetActiveForUserAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return [];

        var myRoleIds = await _roleMapRepository.Table.Where(x => x.UserId == userId).Select(x => x.UserRoleId).ToListAsync();

        var ackedIds = await _ackRepository.Table.Where(x => x.UserId == userId).Select(x => x.AnnouncementId).ToListAsync();
        var ackedSet = ackedIds.ToHashSet();

        var candidates = await _announcementRepository.GetAllAsync(q => q
            .Where(a => a.Active && !a.Deleted && a.ValidFrom <= today && a.ValidTo >= today));

        var result = new List<Announcement>();
        foreach (var a in candidates)
        {
            if (!MatchesAudience(a, myRoleIds))
                continue;

            // Sticky always shows while valid; others hide once acknowledged.
            if (a.Type != AnnouncementType.Sticky && ackedSet.Contains(a.Id))
                continue;

            result.Add(a);
        }

        return result
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.DisplayOrder)
            .ThenByDescending(a => a.ValidFrom)
            .ToList();
    }

    private bool MatchesAudience(Announcement a, List<int> myRoleIds)
    {
        switch (a.Audience)
        {
            case AnnouncementAudience.Everyone:
                return true;
            case AnnouncementAudience.Roles:
                return _roleRepository.Table.Any(r => r.AnnouncementId == a.Id && myRoleIds.Contains(r.UserRoleId));
            default:
                return false;
        }
    }

    public async Task AcknowledgeAsync(int announcementId, int userId)
    {
        var exists = await _ackRepository.Table.AnyAsync(x => x.AnnouncementId == announcementId && x.UserId == userId);
        if (exists)
            return;

        await _ackRepository.InsertAsync(new AnnouncementAcknowledgement
        {
            AnnouncementId = announcementId,
            UserId = userId,
            AcknowledgedOnUtc = DateTime.UtcNow
        });
    }

    public async Task<IList<AnnouncementAcknowledgement>> GetAcknowledgementsAsync(int announcementId)
        => await _ackRepository.GetAllAsync(q => q
            .Include(x => x.User)
            .Where(x => x.AnnouncementId == announcementId)
            .OrderByDescending(x => x.AcknowledgedOnUtc));

    #endregion
}
