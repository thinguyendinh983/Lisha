using Lisha.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Lisha.Infrastructure.Auth.Permissions
{
    public class MustHavePermissionAttribute : AuthorizeAttribute
    {
        public MustHavePermissionAttribute(string action, string resource) =>
            Policy = AppPermission.NameFor(action, resource);
    }
}
