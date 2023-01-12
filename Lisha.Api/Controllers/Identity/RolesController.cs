using Lisha.Application.Identity.Roles;
using Lisha.Infrastructure.Auth.Permissions;

namespace Lisha.Api.Controllers.Identity
{
    public class RolesController : VersionNeutralApiController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService) => _roleService = roleService;

        [HttpGet]
        [MustHavePermission(AppAction.View, AppResource.Roles)]
        [OpenApiOperation("Get a list of all roles.", "")]
        public Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken)
        {
            return _roleService.GetListAsync(cancellationToken);
        }

        [HttpGet("{id}")]
        [MustHavePermission(AppAction.View, AppResource.Roles)]
        [OpenApiOperation("Get role details.", "")]
        public Task<RoleDto> GetByIdAsync(string id)
        {
            return _roleService.GetByIdAsync(id);
        }

        [HttpGet("{id}/permissions")]
        [MustHavePermission(AppAction.View, AppResource.RoleClaims)]
        [OpenApiOperation("Get role details with its permissions.", "")]
        public Task<RoleDto> GetByIdWithPermissionsAsync(string id, CancellationToken cancellationToken)
        {
            return _roleService.GetByIdWithPermissionsAsync(id, cancellationToken);
        }

        [HttpPut("{id}/permissions")]
        [MustHavePermission(AppAction.Update, AppResource.RoleClaims)]
        [OpenApiOperation("Update a role's permissions.", "")]
        public async Task<ActionResult<string>> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
        {
            if (id != request.RoleId)
            {
                return BadRequest();
            }

            return Ok(await _roleService.UpdatePermissionsAsync(request, cancellationToken));
        }

        [HttpPost]
        [MustHavePermission(AppAction.Create, AppResource.Roles)]
        [OpenApiOperation("Create or update a role.", "")]
        public Task<string> RegisterRoleAsync(CreateOrUpdateRoleRequest request)
        {
            return _roleService.CreateOrUpdateAsync(request);
        }

        [HttpDelete("{id}")]
        [MustHavePermission(AppAction.Delete, AppResource.Roles)]
        [OpenApiOperation("Delete a role.", "")]
        public Task<string> DeleteAsync(string id)
        {
            return _roleService.DeleteAsync(id);
        }
    }
}
