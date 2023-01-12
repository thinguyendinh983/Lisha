using AutoMapper;
using Lisha.Application.Auditing;
using Lisha.Infrastructure.Auditing;

namespace Lisha.Infrastructure.Mappings.Auditing
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            CreateMap<Trail, AuditDto>().ReverseMap();
        }
    }
}
