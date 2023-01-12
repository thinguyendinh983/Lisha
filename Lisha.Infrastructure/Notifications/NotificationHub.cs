using Lisha.Application.Common.Interfaces;
using Lisha.Shared.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Lisha.Infrastructure.Notifications
{
    [Authorize]
    public class NotificationHub : Hub, ITransientService
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, NotificationConstants.GroupName);

            await base.OnConnectedAsync();

            _logger.LogInformation("A client connected to NotificationHub: {connectionId}", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationConstants.GroupName);

            await base.OnDisconnectedAsync(exception);

            _logger.LogInformation("A client disconnected from NotificationHub: {connectionId}", Context.ConnectionId);
        }
    }
}
