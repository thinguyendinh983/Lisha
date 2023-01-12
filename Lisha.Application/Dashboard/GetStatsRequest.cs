using Lisha.Application.Identity.Roles;
using Lisha.Application.Identity.Users;

namespace Lisha.Application.Dashboard
{
    public class GetStatsRequest : IRequest<StatsDto>
    {
    }

    public class GetStatsRequestHandler : IRequestHandler<GetStatsRequest, StatsDto>
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IStringLocalizer<GetStatsRequestHandler> _localizer;

        public GetStatsRequestHandler(IUserService userService, IRoleService roleService, IStringLocalizer<GetStatsRequestHandler> localizer)
        {
            _userService = userService;
            _roleService = roleService;
            _localizer = localizer;
        }

        public async Task<StatsDto> Handle(GetStatsRequest request, CancellationToken cancellationToken)
        {
            var stats = new StatsDto
            {
                UserCount = await _userService.GetCountAsync(cancellationToken),
                RoleCount = await _roleService.GetCountAsync(cancellationToken)
            };

            return stats;
        }
    }
}
