using Lisha.Application.Common.Models;
using Lisha.Application.Identity.Users;
using Lisha.Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Lisha.Api.Controllers.Identity
{
    public class UsersController : VersionNeutralApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("search")]
        [MustHavePermission(AppAction.Search, AppResource.Users)]
        [OpenApiOperation("Search Users using available filters.", "")]
        public Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
        {
            return _userService.SearchAsync(filter, cancellationToken);
        }

        [HttpPost("export")]
        [MustHavePermission(AppAction.Export, AppResource.Users)]
        [OpenApiOperation("Export a users.", "")]
        public async Task<FileResult> ExportAsync(CancellationToken cancellationToken)
        {
            var result = await _userService.ExportAsync(cancellationToken);
            return File(result, "application/octet-stream", "UserExports");
        }

        [HttpGet]
        [MustHavePermission(AppAction.View, AppResource.Users)]
        [OpenApiOperation("Get list of all users.", "")]
        public Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken)
        {
            return _userService.GetListAsync(cancellationToken);
        }

        [HttpGet("{id}")]
        [MustHavePermission(AppAction.View, AppResource.Users)]
        [OpenApiOperation("Get a user's details.", "")]
        public Task<UserDetailsDto> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            return _userService.GetAsync(id, cancellationToken);
        }

        [HttpGet("{id}/roles")]
        [MustHavePermission(AppAction.View, AppResource.UserRoles)]
        [OpenApiOperation("Get a user's roles.", "")]
        public Task<List<UserRoleDto>> GetRolesAsync(string id, CancellationToken cancellationToken)
        {
            return _userService.GetRolesAsync(id, cancellationToken);
        }

        [HttpPost("{id}/roles")]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Register))]
        [MustHavePermission(AppAction.Update, AppResource.UserRoles)]
        [OpenApiOperation("Update a user's assigned roles.", "")]
        public Task<string> AssignRolesAsync(string id, UserRolesRequest request, CancellationToken cancellationToken)
        {
            return _userService.AssignRolesAsync(id, request, cancellationToken);
        }

        [HttpPost]
        [MustHavePermission(AppAction.Create, AppResource.Users)]
        [OpenApiOperation("Creates a new user.", "")]
        public Task<string> CreateAsync(CreateUserRequest request)
        {
            // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
            // and return UnAuthorized when it isn't
            // Also: add other protection to prevent automatic posting (captcha?)
            return _userService.CreateAsync(request, GetOriginFromRequest());
        }

        [HttpPost("self-register")]
        [AllowAnonymous]
        [OpenApiOperation("Anonymous user creates a user.", "")]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Register))]
        public Task<string> SelfRegisterAsync(CreateUserRequest request)
        {
            // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
            // and return UnAuthorized when it isn't
            // Also: add other protection to prevent automatic posting (captcha?)
            return _userService.CreateAsync(request, GetOriginFromRequest());
        }

        [HttpPost("{id}/toggle-status")]
        [MustHavePermission(AppAction.Update, AppResource.Users)]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Register))]
        [OpenApiOperation("Toggle a user's active status.", "")]
        public async Task<ActionResult> ToggleStatusAsync(string id, ToggleUserStatusRequest request, CancellationToken cancellationToken)
        {
            if (id != request.UserId)
            {
                return BadRequest();
            }

            await _userService.ToggleStatusAsync(request, cancellationToken);
            return Ok();
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        [OpenApiOperation("Confirm email address for a user.", "")]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Search))]
        public Task<string> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code, CancellationToken cancellationToken)
        {
            return _userService.ConfirmEmailAsync(userId, code, cancellationToken);
        }

        [HttpGet("confirm-phone-number")]
        [AllowAnonymous]
        [OpenApiOperation("Confirm phone number for a user.", "")]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Search))]
        public Task<string> ConfirmPhoneNumberAsync([FromQuery] string userId, [FromQuery] string code)
        {
            return _userService.ConfirmPhoneNumberAsync(userId, code);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [OpenApiOperation("Request a pasword reset email for a user.", "")]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Register))]
        public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            return _userService.ForgotPasswordAsync(request, GetOriginFromRequest());
        }

        [HttpPost("reset-password")]
        [OpenApiOperation("Reset a user's password.", "")]
        [ApiConventionMethod(typeof(AppApiConventions), nameof(AppApiConventions.Register))]
        public Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return _userService.ResetPasswordAsync(request);
        }

        private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
    }
}
