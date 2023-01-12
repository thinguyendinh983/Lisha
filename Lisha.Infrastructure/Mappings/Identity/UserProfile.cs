using AutoMapper;
using Lisha.Application.Identity.Users;
using Lisha.Infrastructure.Identity;

namespace Lisha.Infrastructure.Mappings.Identity
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserDetailsDto>().ReverseMap();
            CreateMap<UserDetailsDto, UserExportDto>().ReverseMap();
        }
    }
}
