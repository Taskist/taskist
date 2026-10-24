using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Notification;

namespace Taskist.Service.Messages;

public interface IMessageService
{
    #region Account

    Task EmailPasswordResetLinkAsync(User user, string token);

    Task EmailActivationKitAsync(User user);

    Task EmailWelcomeKitAsync(User user);

    Task EmailRegistrationKitAsync(User user);

    Task EmailNotificationAsync(User user, string content, string? subject = null);

    #endregion

    #region Email

    Task<Task> EmailNewTaskNotificationAsync(Backlog backlog, bool tester);

    Task<Task> EmailTaskNotificationAsync(Backlog backlog, User loggedUser, string status, bool tester);

    #endregion
}
