using Lisha.Application.Common.Events;
using Lisha.Application.Common.Interfaces;
using Lisha.Domain.Identity;
using Lisha.Shared.Notifications;
using Microsoft.Extensions.Logging;

namespace Lisha.Application.Dashboard
{
    public class SendStatsChangedNotificationHandler :
    IEventNotificationHandler<ApplicationRoleCreatedEvent>,
    IEventNotificationHandler<ApplicationRoleDeletedEvent>,
    IEventNotificationHandler<ApplicationUserCreatedEvent>
    {
        private readonly ILogger<SendStatsChangedNotificationHandler> _logger;
        private readonly INotificationSender _notifications;

        public SendStatsChangedNotificationHandler(ILogger<SendStatsChangedNotificationHandler> logger, INotificationSender notifications) =>
            (_logger, _notifications) = (logger, notifications);

        public Task Handle(EventNotification<ApplicationRoleCreatedEvent> notification, CancellationToken cancellationToken) =>
            SendStatsChangedNotification(notification.Event, cancellationToken);
        public Task Handle(EventNotification<ApplicationRoleDeletedEvent> notification, CancellationToken cancellationToken) =>
            SendStatsChangedNotification(notification.Event, cancellationToken);
        public Task Handle(EventNotification<ApplicationUserCreatedEvent> notification, CancellationToken cancellationToken) =>
            SendStatsChangedNotification(notification.Event, cancellationToken);

        private Task SendStatsChangedNotification(IEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{event} Triggered => Sending StatsChangedNotification", @event.GetType().Name);

            return _notifications.SendToAllAsync(new StatsChangedNotification(), cancellationToken);
        }
    }
}
