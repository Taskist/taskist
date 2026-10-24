using Taskist.Core.Extensions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Wiki;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Taskist.Service.Wiki;

public class WikiCategoryService : IWikiCategoryService
{
    #region Fields

    protected readonly IRepository<WikiCategory> _wikiCategoryRepository;

    #endregion

    #region Ctor
    public WikiCategoryService(IRepository<WikiCategory> wikiCategoryRepository)
    {
        _wikiCategoryRepository = wikiCategoryRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<WikiCategory>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _wikiCategoryRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.ApplySafeSort(sortColumn, sortDirection);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IList<WikiCategory>> GetAllActiveAsync()
    {
        return await _wikiCategoryRepository.GetAllAsync(query => query.Where(x => x.Active), false);
    }

    public async Task<IList<WikiCategory>> GetAllParentAsync(int projectId)
    {
        return await _wikiCategoryRepository.GetAllAsync(query => query.Where(x => x.ParentCategoryId == null && x.ProjectId == projectId && x.Active), false);
    }

    public async Task<IList<WikiCategory>> GetAllByParentCategoryIdAsync(int parentCategoryId, int projectId)
    {
        return await _wikiCategoryRepository.GetAllAsync(query => query.Where(x => x.Active && x.ProjectId == projectId
            && x.ParentCategoryId == parentCategoryId), false);
    }

    public async Task<WikiCategory> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _wikiCategoryRepository.GetByIdAsync(id);
    }

    public async Task<WikiCategory> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _wikiCategoryRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(WikiCategory entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _wikiCategoryRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(List<WikiCategory> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _wikiCategoryRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(WikiCategory entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _wikiCategoryRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(WikiCategory entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _wikiCategoryRepository.DeleteAsync(entity);
    }

    #endregion
}
