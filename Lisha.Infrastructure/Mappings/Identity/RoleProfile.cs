using AutoMapper;
using Lisha.Application.Identity.Roles;
using Lisha.Infrastructure.Identity;

namespace Lisha.Infrastructure.Mappings.Identity
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<ApplicationRole, RoleDto>().ReverseMap();
        }
    }
}
