using Lisha.Domain.Common.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Domain.Common.Contracts
{
    public interface IEntity
    {
        Collection<DomainEvent> DomainEvents { get; }
    }

    public interface IEntity<out TId> : IEntity
    {
        TId Id { get; }
    }
}
