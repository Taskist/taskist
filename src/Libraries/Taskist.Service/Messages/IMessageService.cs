using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Service.Messages;

public interface IMessageService
{
    #region Account

    Task EmailPasswordResetLinkAsync(User user, string token);

    Task EmailActivationKitAsync(User user);

    Task EmailWelcomeKitAsync(User user);

    Task EmailRegistrationKitAsync(User user);

    Task EmailNotificationAsync(User user, string content);

    #endregion

    #region Backlog

    Task<Task> EmailNewTaskNotificationAsync(Backlog backlog);

    Task<Task> EmailTaskNotificationAsync(Backlog backlog, User loggedUser, string status);

    #endregion
}
