using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Taskist.Service.WorkItems;

public class SprintService : ISprintService
{
	#region Fields

	protected readonly IRepository<Sprint> _sprintRepository;

	#endregion

	#region Ctor
	public SprintService(IRepository<Sprint> sprintRepository)
	{
		_sprintRepository = sprintRepository;
	}
	#endregion

	#region Methods

	public async Task<IPagedList<Sprint>> GetPagedListAsync(string search = "", int pageIndex = 0,
	int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
	{
		return await _sprintRepository.GetAllPagedAsync(query =>
		{
			query = query.Where(x => !x.Deleted);
			query = query.OrderBy($"{sortColumn} {sortDirection}");

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(c => c.Name.Contains(search));

			return query;
		}, pageIndex, pageSize);
	}
	public async Task<IList<Sprint>> GetAllAsync(bool showDeleted = false)
	{
		return await _sprintRepository.GetAllAsync(includeDeleted: showDeleted);
	}

	public async Task<IList<Sprint>> GetAllActiveAsync(bool showDeleted = false)
	{
		return await _sprintRepository.GetAllAsync(q => q.Where(x => !x.Deleted), showDeleted);
	}

	public async Task<IList<Sprint>> GetAllActiveByProjectAsync(int projectId)
	{
		return await _sprintRepository.GetAllAsync(q => q.Where(x => x.ProjectId == projectId && !x.Deleted && !x.Completed));
	}

	public async Task<IList<Sprint>> GetSprintBetweenDateAsync(int projectId, DateOnly startDate, DateOnly endDate)
	{
		return await _sprintRepository.GetAllAsync(q => q.Where(x => !x.Deleted
		&& x.ProjectId == projectId).Where(x => (startDate >= x.StartDate && startDate <= x.EndDate) || (endDate >= x.StartDate && endDate <= x.EndDate)));
	}

	public async Task<Sprint> GetByIdAsync(int id)
	{
		return id == 0 ? null : await _sprintRepository.GetByIdAsync(id);
	}

	public async Task<Sprint> GetByNameAsync(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return null;

		var query = from c in _sprintRepository.Table
					orderby c.Id
					where !c.Deleted && c.Name == name
					select c;
		return await query.FirstOrDefaultAsync();
	}

	public async Task InsertAsync(Sprint entity)
	{
		if (entity == null)
			throw new ArgumentNullException(nameof(entity));

		await _sprintRepository.InsertAsync(entity);
	}

	public async Task InsertAsync(IList<Sprint> entities)
	{
		if (entities == null)
			throw new ArgumentNullException(nameof(entities));

		await _sprintRepository.InsertAsync(entities);
	}

	public async Task UpdateAsync(Sprint entity)
	{

		if (entity == null)
			throw new ArgumentNullException(nameof(entity));

		await _sprintRepository.UpdateAsync(entity);
	}

	public async Task DeleteAsync(Sprint entity)
	{
		if (entity == null)
			throw new ArgumentNullException(nameof(entity));

		await _sprintRepository.DeleteAsync(entity);
	}

	#endregion
}
