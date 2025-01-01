using Lisha.Domain.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Domain.Common
{
    public class AuditableEntity<TId> : BaseEntity<TId>, IAuditableEntity, ISoftDeletable
    {
        public DateTimeOffset Created { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public abstract class AuditableEntity : AuditableEntity<Guid>
    {
        protected AuditableEntity() => Id = Guid.NewGuid();
    }
}
