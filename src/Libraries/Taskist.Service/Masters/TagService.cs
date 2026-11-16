using Taskist.Core.Extensions;
using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace Taskist.Service.Masters;

public class TagService : ITagService
{
    #region Fields

    protected readonly IRepository<Tag> _tagRepository;
    protected readonly IRepository<BacklogTag> _backlogTagRepository;

    #endregion

    #region Ctor
    public TagService(IRepository<Tag> tagRepository,
        IRepository<BacklogTag> backlogTagRepository)
    {
        _tagRepository = tagRepository;
        _backlogTagRepository = backlogTagRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Tag>> GetPagedListAsync(string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _tagRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.ApplySafeSort(sortColumn, sortDirection);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IList<Tag>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _tagRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<Tag> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _tagRepository.GetByIdAsync(id);
    }

    public async Task<Tag> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _tagRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task<Tag> GetOrCreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        name = name.Trim();
        var existing = await GetByNameAsync(name);
        if (existing != null)
            return existing;

        var entity = new Tag { Name = name, Active = true };
        await _tagRepository.InsertAsync(entity);
        return entity;
    }

    public async Task InsertAsync(Tag entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _tagRepository.InsertAsync(entity);
    }

    public async Task UpdateAsync(Tag entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _tagRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Tag entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _tagRepository.DeleteAsync(entity);
    }

    public async Task MergeAsync(int sourceId, int targetId)
    {
        if (sourceId == targetId || sourceId == 0 || targetId == 0)
            return;

        var targetTaskIds = await _backlogTagRepository.Table
            .Where(x => x.TagId == targetId)
            .Select(x => x.BacklogId)
            .ToListAsync();

        var sourceLinks = await _backlogTagRepository.GetAllAsync(q => q.Where(x => x.TagId == sourceId));

        foreach (var link in sourceLinks)
        {
            if (targetTaskIds.Contains(link.BacklogId))
            {
                await _backlogTagRepository.DeleteAsync(link);
            }
            else
            {
                link.TagId = targetId;
                await _backlogTagRepository.UpdateAsync(link);
            }
        }

        var source = await GetByIdAsync(sourceId);
        if (source != null)
        {
            source.Deleted = true;
            await _tagRepository.UpdateAsync(source);
        }
    }

    public async Task<List<int>> GetTagIdsByTaskAsync(int taskId)
    {
        return await _backlogTagRepository.Table
            .Where(x => x.BacklogId == taskId)
            .Select(x => x.TagId)
            .ToListAsync();
    }

    public async Task<Dictionary<int, string>> GetTagNamesByTaskIdsAsync(int[] taskIds)
    {
        if (taskIds is not { Length: > 0 })
            return [];

        var rows = await _backlogTagRepository.Table
            .Where(x => taskIds.Contains(x.BacklogId))
            .Select(x => new { x.BacklogId, x.Tag.Name })
            .ToListAsync();

        return rows
            .GroupBy(r => r.BacklogId)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(r => r.Name).OrderBy(n => n)));
    }

    public async Task<int[]> ResolveTagIdsAsync(string[] tokens)
    {
        var ids = new List<int>();

        foreach (var token in tokens ?? [])
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;

            if (int.TryParse(token, out var id) && id > 0)
            {
                ids.Add(id);
                continue;
            }

            var tag = await GetOrCreateAsync(token.Trim());
            if (tag != null)
                ids.Add(tag.Id);
        }

        return ids.Distinct().ToArray();
    }

    public async Task SetTaskTagsAsync(int taskId, int[] tagIds)
    {
        tagIds ??= [];

        var existing = await _backlogTagRepository.GetAllAsync(q => q.Where(x => x.BacklogId == taskId));
        var existingIds = existing.Select(x => x.TagId).ToList();

        foreach (var link in existing.Where(x => !tagIds.Contains(x.TagId)))
            await _backlogTagRepository.DeleteAsync(link);

        foreach (var tagId in tagIds.Distinct().Where(id => !existingIds.Contains(id)))
            await _backlogTagRepository.InsertAsync(new BacklogTag { BacklogId = taskId, TagId = tagId });
    }

    #endregion
}
