using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using Taskist.Data.Extensions;

namespace Taskist.Data.Repository;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    #region Fields

    private readonly ApplicationContext _context;
    private readonly DbSet<T> _entities;

    #endregion

    #region Ctor

    public Repository(ApplicationContext context)
    {
        try
        {
            _context = context;
            _entities = context.Set<T>();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    #endregion

    #region Methods

    public async Task<T> GetByIdAsync(int id, bool includeDeleted = true)
    {
        async Task<T> getEntityAsync()
        {
            return await AddDeletedFilter(Table, includeDeleted).FirstOrDefaultAsync(entity => entity.Id == Convert.ToInt32(id));
        }

        return await getEntityAsync();
    }

    public T GetById(int id, bool includeDeleted = true)
    {
        T getEntity()
        {
            return AddDeletedFilter(Table, includeDeleted).FirstOrDefault(entity => entity.Id == Convert.ToInt32(id));
        }

        return getEntity();
    }

    public async Task<IList<T>> GetByIdsAsync(IList<int> ids, bool includeDeleted = true)
    {
        if (ids?.Any() != true)
            return new List<T>();

        async Task<IList<T>> getByIdsAsync()
        {
            var query = AddDeletedFilter(Table, includeDeleted);

            var entriesById = await query
                .Where(entry => ids.Contains(entry.Id))
                .ToDictionaryAsync(entry => entry.Id);

            var sortedEntries = new List<T>();
            foreach (var id in ids)
            {
                if (entriesById.TryGetValue(id, out var sortedEntry))
                    sortedEntries.Add(sortedEntry);
            }

            return sortedEntries;
        }

        return await getByIdsAsync();
    }

    public async Task<IList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? func = null, bool includeDeleted = true)
    {
        async Task<IList<T>> getAllAsync()
        {
            var query = AddDeletedFilter(Table, includeDeleted);
            query = func != null ? func(query) : query;

            return await query.ToListAsync();
        }

        return await getAllAsync();
    }

    public IList<T> GetAll(Func<IQueryable<T>, IQueryable<T>>? func = null, bool includeDeleted = true)
    {
        IList<T> getAll()
        {
            var query = AddDeletedFilter(Table, includeDeleted);
            query = func != null ? func(query) : query;

            return query.ToList();
        }

        return getAll();
    }

    public async Task<IList<T>> GetAllAsync(Func<IQueryable<T>, Task<IQueryable<T>>>? func = null, bool includeDeleted = true)
    {
        async Task<IList<T>> getAllAsync()
        {
            var query = AddDeletedFilter(Table, includeDeleted);
            query = func != null ? await func(query) : query;

            return await query.ToListAsync();
        }

        return await getAllAsync();
    }

    public async Task<IPagedList<T>> GetAllPagedAsync(Func<IQueryable<T>, IQueryable<T>>? func = null,
        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true)
    {
        var query = AddDeletedFilter(Table, includeDeleted);

        query = func != null ? func(query) : query;

        return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
    }

    public async Task<IPagedList<T>> GetAllPagedAsync(Func<IQueryable<T>, Task<IQueryable<T>>>? func = null,
        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true)
    {
        var query = AddDeletedFilter(Table, includeDeleted);

        query = func != null ? await func(query) : query;

        return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
    }

    public async Task InsertAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _entities.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task<T> InsertAndGetAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _entities.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
        return entity;
    }

    public void Insert(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _entities.Add(entity);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task InsertAsync(IList<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        try
        {
            await _entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public void Insert(IList<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        try
        {
            _entities.AddRange(entities);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public void Update(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task UpdateAsync(IList<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public void Update(IList<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        try
        {
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task DeleteAsync(T entity)
    {
        try
        {
            switch (entity)
            {
                case null:
                    throw new ArgumentNullException(nameof(entity));

                case ISoftDeletedEntity softDeletedEntity:
                    softDeletedEntity.Deleted = true;
                    await UpdateAsync(entity);
                    break;

                default:
                    _entities.Remove(entity);
                    await _context.SaveChangesAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public void Delete(T entity)
    {
        try
        {
            switch (entity)
            {
                case null:
                    throw new ArgumentNullException(nameof(entity));

                case ISoftDeletedEntity softDeletedEntity:
                    softDeletedEntity.Deleted = true;
                    Update(entity);
                    break;

                default:
                    _entities.Remove(entity);
                    _context.SaveChanges();
                    break;
            }
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task DeleteAsync(IList<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        try
        {
            _entities.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        try
        {
            var entities = await _entities.Where(predicate).ToListAsync();
            _entities.RemoveRange(entities);
            await _context.SaveChangesAsync();

            return entities.Count;
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
            return 0;
        }
    }

    public int Delete(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        try
        {
            var entities = _entities.Where(predicate).ToList();
            _entities.RemoveRange(entities);
            _context.SaveChanges();

            return entities.Count;
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
            return 0;
        }
    }

    public async Task TruncateAsync(bool resetIdentity = false)
    {
        try
        {
            await _context.BulkDeleteAsync(_entities, new BulkConfig
            {
                SetOutputIdentity = resetIdentity
            });
        }
        catch (Exception ex)
        {
            WriteTrace(ex);
        }
    }

    #endregion

    #region Properties

    public IQueryable<T> Table => _entities;

    #endregion

    #region Helper

    protected IQueryable<T> AddDeletedFilter(IQueryable<T> query, in bool includeDeleted)
    {
        if (includeDeleted)
            return query;

        if (typeof(T).GetInterface(nameof(ISoftDeletedEntity)) == null)
            return query;

        return query.OfType<ISoftDeletedEntity>().Where(entry => !entry.Deleted).OfType<T>();
    }

    private void WriteTrace(Exception ex)
    {
        var stackTrace = new StackTrace();
        var caller = stackTrace.GetFrame(1).GetMethod().Name;
        Trace.WriteLine($"Backlog.Data >> Repository >> {caller} >> Error >> {ex.Message}");
        Trace.WriteLine($"Backlog.Data >> Repository >> {caller} >> StackTrace >> {ex.StackTrace}");

        string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

        string filePath = Path.Combine(rootPath, $"{DateTime.Today:dd-MM-yyy}.txt");

        using StreamWriter outputFile = new StreamWriter(filePath, true);
        outputFile.WriteLine(ex.Message);
        if (ex.InnerException != null)
            outputFile.WriteLine(ex.InnerException.Message);
    }

    #endregion Helper
}
