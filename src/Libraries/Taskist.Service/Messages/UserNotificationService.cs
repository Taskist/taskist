using System.Net;
using Microsoft.EntityFrameworkCore;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Notification;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Common;

namespace Taskist.Service.Messages;

public class UserNotificationService : IUserNotificationService
{
    #region Fields

    protected readonly IRepository<UserNotification> _userNotificationRepository;
    protected readonly IRepository<User> _userRepository;
    protected readonly IMessageService _messageService;
    protected readonly IRealtimeAlertNotifier _alertNotifier;
    protected readonly IHttpHelper _httpHelper;
    protected readonly INotificationChannelService _notificationChannelService;

    #endregion

    #region Ctor

    public UserNotificationService(IRepository<UserNotification> userNotificationRepository,
        IRepository<User> userRepository,
        IMessageService messageService,
        IRealtimeAlertNotifier alertNotifier,
        IHttpHelper httpHelper,
        INotificationChannelService notificationChannelService)
    {
        _notificationChannelService = notificationChannelService;
        _userNotificationRepository = userNotificationRepository;
        _userRepository = userRepository;
        _messageService = messageService;
        _alertNotifier = alertNotifier;
        _httpHelper = httpHelper;
    }

    #endregion

    #region Methods

    public async Task<int> NotifyAsync(UserNotificationRequest request)
    {
        var ids = request.UserIds.Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0 || string.IsNullOrWhiteSpace(request.Title))
            return 0;

        var recipients = new List<int>();
        foreach (var id in ids)
        {
            if (!string.IsNullOrWhiteSpace(request.DedupeKey) && await ExistsAsync(id, request.DedupeKey, request.DedupeHours))
                continue;
            recipients.Add(id);
        }

        if (recipients.Count == 0)
            return 0;

        var now = DateTime.UtcNow;
        await _userNotificationRepository.InsertAsync(recipients.Select(id => new UserNotification
        {
            UserId = id,
            Type = (int)request.Type,
            Title = Truncate(request.Title, 300)!,
            Message = Truncate(request.Message, 1000),
            Url = Truncate(request.Url, 500),
            DedupeKey = Truncate(request.DedupeKey, 200),
            CreatedOnUtc = now
        }).ToList());

        try
        {
            await _alertNotifier.NotifyAsync(recipients.ToArray());
        }
        catch (Exception)
        {
        }

        if (request.Email || request.ChatProjectId > 0)
            await SendExternalAsync(request, recipients);

        return recipients.Count;
    }

    public async Task<IList<UserNotification>> GetRecentAsync(int userId, int take = 20)
    {
        return await _userNotificationRepository.Table
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedOnUtc)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _userNotificationRepository.Table.CountAsync(x => x.UserId == userId && !x.IsRead);
    }

    public async Task<UserNotification?> MarkReadAsync(int userId, int id)
    {
        var entity = await _userNotificationRepository.Table.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity == null)
            return null;

        if (!entity.IsRead)
        {
            entity.IsRead = true;
            entity.ReadOnUtc = DateTime.UtcNow;
            await _userNotificationRepository.UpdateAsync(entity);
        }

        return entity;
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var now = DateTime.UtcNow;
        await _userNotificationRepository.Table
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true).SetProperty(x => x.ReadOnUtc, now));
    }

    public async Task<bool> ExistsAsync(int userId, string dedupeKey, int? withinHours)
    {
        var query = _userNotificationRepository.Table.Where(x => x.UserId == userId && x.DedupeKey == dedupeKey);
        if (withinHours is > 0)
        {
            var since = DateTime.UtcNow.AddHours(-withinHours.Value);
            query = query.Where(x => x.CreatedOnUtc >= since);
        }

        return await query.AnyAsync();
    }

    #endregion

    #region Utilities

    private async Task SendExternalAsync(UserNotificationRequest request, IList<int> recipients)
    {
        var users = await _userRepository.Table
            .Where(x => recipients.Contains(x.Id) && !x.Deleted && !x.Locked)
            .ToListAsync();

        var link = AbsoluteUrl(request.Url);
        var title = WebUtility.HtmlEncode(request.Title);
        var message = string.IsNullOrWhiteSpace(request.Message) ? string.Empty : WebUtility.HtmlEncode(request.Message).Replace("\n", "<br/>");

        foreach (var user in users)
        {
            if (request.Email && !string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    var body = $"<p><strong>{title}</strong></p>"
                               + (message.Length > 0 ? $"<p>{message}</p>" : string.Empty)
                               + (link != null ? $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">{WebUtility.HtmlEncode(link)}</a></p>" : string.Empty);
                    await _messageService.EmailNotificationAsync(user, body, request.Title);
                }
                catch (Exception)
                {
                }
            }
        }

        if (request.ChatProjectId > 0 && users.Count > 0)
        {
            try
            {
                await _notificationChannelService.PublishAsync(request.ChatProjectId.Value, new ChatMessage
                {
                    Event = request.Type == NotificationType.MergeRequestConflict ? ChatEvent.Conflict
                        : request.Type == NotificationType.MergeRequestClosed ? ChatEvent.MergeRequestClosed
                        : ChatEvent.CodeReview,
                    Title = request.Title,
                    Lines = string.IsNullOrWhiteSpace(request.Message) ? [] : request.Message.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList(),
                    Url = link,
                    Recipients = users.Select(u => new ChatRecipient { Name = u.Name, Email = u.Email }).ToList()
                });
            }
            catch (Exception)
            {
            }
        }
    }

    private string? AbsoluteUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
        if (Uri.TryCreate(url, UriKind.Absolute, out _))
            return url;

        try
        {
            return _httpHelper.GetBaseURL().TrimEnd('/') + "/" + url.TrimStart('/');
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? Truncate(string? value, int length)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        return value.Length <= length ? value : value[..(length - 1)] + "…";
    }

    #endregion
}
