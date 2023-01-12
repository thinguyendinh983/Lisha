using Lisha.Application.Dashboard;
using Lisha.Infrastructure.Auth.Permissions;

namespace Lisha.Api.Controllers.Dashboard
{
    public class DashboardController : VersionedApiController
    {
        [HttpGet]
        [MustHavePermission(AppAction.View, AppResource.Dashboard)]
        [OpenApiOperation("Get statistics for the dashboard.", "")]
        public Task<StatsDto> GetAsync()
        {
            return Mediator.Send(new GetStatsRequest());
        }
    }
}
