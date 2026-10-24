using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Wiki;

namespace Taskist.Core.Domain.WorkItems;

public class Backlog : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public Guid Code { get; set; }

    public string Title { get; set; }

    public string? Description { get; set; }

    public string? RootCauseAnalysis { get; set; }

    public string? ExternalNotes { get; set; }

    public string? EnvironmentJson { get; set; }

    public int? ParentId { get; set; }

    public int TaskTypeId { get; set; }

    public int SeverityId { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly? PlannedStartDate { get; set; }

    public int ProjectId { get; set; }

    public int? ModuleId { get; set; }

    public int? SubModuleId { get; set; }

    public int? ReleaseId { get; set; }

    public int? ReporterId { get; set; }

    public int? AssigneeId { get; set; }

    public int? TesterId { get; set; }

    public int ReOpenCount { get; set; }

    public int SubTaskCount { get; set; }

    public int? AttachmentCount { get; set; }

    public int? CommentCount { get; set; }

    public int StatusId { get; set; }

    public string? ResolutionDetails { get; set; }

    public int? MergeRequestId { get; set; }

    public int? MergeRequestRepositoryId { get; set; }

    public int? SourceId { get; set; }

    public int? SourceWikiDocumentId { get; set; }

    public int CreatedById { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedById { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool Deleted { get; set; }

    public virtual Backlog Parent { get; set; }

    public virtual TaskType TaskType { get; set; }

    public virtual Severity Severity { get; set; }

    public virtual Project Project { get; set; }

    public virtual Module Module { get; set; }

    public virtual SubModule SubModule { get; set; }

    public virtual Reporter Reporter { get; set; }

    public virtual Release Release { get; set; }

    public virtual User Assignee { get; set; }

    public virtual User Tester { get; set; }

    public virtual Status Status { get; set; }

    public virtual User CreatedBy { get; set; }

    public virtual User ModifiedBy { get; set; }

    public virtual WikiDocument SourceWikiDocument { get; set; }}
