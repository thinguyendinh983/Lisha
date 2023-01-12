using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Common.Events
{
    public interface IEventPublisher : ITransientService
    {
        Task PublishAsync(IEvent @event);
    }
}
