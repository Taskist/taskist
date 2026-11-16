namespace Taskist.Core.Dtos.Git;

public class ReviewLinkedTaskDto
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Status { get; set; }

    public string? TaskType { get; set; }

    public int StatusGroupId { get; set; }

    public int TaskTypeGroupId { get; set; }

    public string? Description { get; set; }
}
