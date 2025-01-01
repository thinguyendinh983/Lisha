using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Domain.Common.Contracts
{
    public interface IAuditableEntity
    {
        DateTimeOffset Created { get; }
        Guid CreatedBy { get; }
        DateTimeOffset LastModified { get; }
        Guid? LastModifiedBy { get; }
    }
}
