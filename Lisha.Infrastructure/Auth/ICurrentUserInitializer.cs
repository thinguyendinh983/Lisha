using System.Security.Claims;

namespace Lisha.Infrastructure.Auth
{
    public interface ICurrentUserInitializer
    {
        void SetCurrentUser(ClaimsPrincipal user);

        void SetCurrentUserId(string userId);
    }
}
