using Lisha.Shared.Events;

namespace Lisha.Domain.Common.Contracts
{
    public abstract class DomainEvent : IEvent
    {
        public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
    }
}
