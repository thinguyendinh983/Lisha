using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Domain.Common.Contracts
{
    public interface ISoftDeletable
    {
        DateTimeOffset? Deleted { get; set; }
        Guid? DeletedBy { get; set; }
    }
}
