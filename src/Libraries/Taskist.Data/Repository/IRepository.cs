using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using System.Linq.Expressions;

namespace Taskist.Data.Repository;

public interface IRepository<T> where T : BaseEntity
{
    #region Methods

    Task<IQueryable<T>> GetQueryableAsync(Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "");

    Task<T> GetByIdAsync(int id, bool includeDeleted = true, params Expression<Func<T, object>>[] includes);

    T GetById(int id, bool includeDeleted = true);

    Task<IList<T>> GetByIdsAsync(IList<int> ids, bool includeDeleted = true);

    Task<IList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? func = null, bool includeDeleted = true);

    IList<T> GetAll(Func<IQueryable<T>, IQueryable<T>>? func = null, bool includeDeleted = true);

    Task<IPagedList<T>> GetAllPagedAsync(Func<IQueryable<T>, IQueryable<T>>? func = null,
        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true);

    Task<IPagedList<T>> GetAllPagedAsync(Func<IQueryable<T>, Task<IQueryable<T>>>? func = null,
        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true);

    Task InsertAsync(T entity);

    Task<T> InsertAndGetAsync(T entity);

    void Insert(T entity);

    Task InsertAsync(IList<T> entities);

    void Insert(IList<T> entities);

    Task UpdateAsync(T entity);

    void Update(T entity);

    Task UpdateAsync(IList<T> entities);

    Task RewritePropertiesAsync(T entity, params string[] propertyNames);

    void Update(IList<T> entities);

    Task DeleteAsync(T entity);

    void Delete(T entity);

    Task DeleteAsync(IList<T> entities);

    Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);

    int Delete(Expression<Func<T, bool>> predicate);

    Task TruncateAsync(bool resetIdentity = false);

    #endregion

    #region Properties

    IQueryable<T> Table { get; }

    #endregion
}