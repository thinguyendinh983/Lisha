using AutoMapper;
using Lisha.Application.Auditing;
using Lisha.Infrastructure.Persistence.Context;

namespace Lisha.Infrastructure.Auditing
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuditService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<AuditDto>> GetUserTrailsAsync(Guid userId)
        {
            var trails = await _context.AuditTrails
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.DateTime)
                .Take(250)
                .ProjectToListAsync<AuditDto>(_mapper.ConfigurationProvider);

            return trails;
        }
    }
}
