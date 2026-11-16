using Riok.Mapperly.Abstractions;
using Taskist.Core.Domain.Announcements;
using Taskist.Web.Models.Announcements;

namespace Taskist.Web.Helpers.Mapping;

[Mapper]
public partial class AnnouncementMapper
{
    [MapperIgnoreSource(nameof(Announcement.Roles))]
    [MapperIgnoreSource(nameof(Announcement.Deleted))]
    public partial AnnouncementModel ToModel(Announcement source);

    [MapperIgnoreTarget(nameof(Announcement.Roles))]
    [MapperIgnoreTarget(nameof(Announcement.Deleted))]
    public partial Announcement ToEntity(AnnouncementModel source);

    [MapperIgnoreTarget(nameof(Announcement.Roles))]
    [MapperIgnoreTarget(nameof(Announcement.Deleted))]
    public partial void Update(AnnouncementModel source, Announcement target);
}
