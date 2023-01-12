using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Auditing
{
    public interface IAuditService : ITransientService
    {
        Task<List<AuditDto>> GetUserTrailsAsync(Guid userId);
    }
}
