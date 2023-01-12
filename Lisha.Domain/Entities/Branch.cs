using Lisha.Domain.Common.Contracts;

namespace Lisha.Domain.Entities
{
    public class Branch : AuditableEntity, IAggregateRoot
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
