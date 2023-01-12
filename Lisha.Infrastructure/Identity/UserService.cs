using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Lisha.Application.Common.Caching;
using Lisha.Application.Common.Events;
using Lisha.Application.Common.Exceptions;
using Lisha.Application.Common.Exporters;
using Lisha.Application.Common.FileStorage;
using Lisha.Application.Common.Interfaces;
using Lisha.Application.Common.Mailing;
using Lisha.Application.Common.Models;
using Lisha.Application.Common.Specification;
using Lisha.Application.Identity.Users;
using Lisha.Domain.Common;
using Lisha.Domain.Identity;
using Lisha.Infrastructure.Auth;
using Lisha.Infrastructure.Common;
using Lisha.Infrastructure.Mailing;
using Lisha.Infrastructure.Persistence.Context;
using Lisha.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace Lisha.Infrastructure.Identity
{
    internal class UserService : IUserService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IStringLocalizer<UserService> _localizer;
        private readonly IJobService _jobService;
        private readonly IMailService _mailService;
        private readonly MailSettings _mailSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly IEmailTemplateService _templateService;
        private readonly IFileStorageService _fileStorage;
        private readonly IEventPublisher _events;
        private readonly ICacheService _cache;
        private readonly ICacheKeyService _cacheKeys;
        private readonly IMapper _mapper;
        private readonly IExcelWriter _excelWriter;

        public UserService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext dbContext,
            IStringLocalizer<UserService> localizer,
            IJobService jobService,
            IMailService mailService,
            IOptions<MailSettings> mailSettings,
            IEmailTemplateService templateService,
            IFileStorageService fileStorage,
            IEventPublisher events,
            ICacheService cache,
            ICacheKeyService cacheKeys,
            IOptions<SecuritySettings> securitySettings,
            IMapper mapper,
            IExcelWriter excelWriter)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _localizer = localizer;
            _jobService = jobService;
            _mailService = mailService;
            _mailSettings = mailSettings.Value;
            _templateService = templateService;
            _fileStorage = fileStorage;
            _events = events;
            _cache = cache;
            _cacheKeys = cacheKeys;
            _securitySettings = securitySettings.Value;
            _mapper = mapper;
            _excelWriter = excelWriter;
        }

        public async Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
        {
            var spec = new EntitiesByPaginationFilterSpec<ApplicationUser>(filter);

            var users = await _userManager.Users
                .WithSpecification(spec)
                .ProjectToListAsync<UserDetailsDto>(_mapper.ConfigurationProvider);

            int count = await _userManager.Users.CountAsync(cancellationToken);

            return new PaginationResponse<UserDetailsDto>(users, count, filter.PageNumber, filter.PageSize);
        }

        public async Task<Stream> ExportAsync(CancellationToken cancellationToken)
        {
            var list = _mapper.Map<List<UserExportDto>>(await GetListAsync(cancellationToken));
            return _excelWriter.WriteToStream(list);
        }

        public async Task<bool> ExistsWithNameAsync(string name)
        {
            return await _userManager.FindByNameAsync(name) is not null;
        }

        public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
        {
            return await _userManager.FindByEmailAsync(email.Normalize()) is ApplicationUser user && user.Id != exceptId;
        }

        public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
        {
            return await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is ApplicationUser user && user.Id != exceptId;
        }

        public async Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken) =>
            await _userManager.Users
                    .AsNoTracking()
                    .ProjectToListAsync<UserDetailsDto>(_mapper.ConfigurationProvider);

        public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
            _userManager.Users.AsNoTracking().CountAsync(cancellationToken);

        public async Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            _ = user ?? throw new NotFoundException(_localizer["User Not Found."]);

            return _mapper.Map<UserDetailsDto>(user);
        }

        public async Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.Where(u => u.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);

            _ = user ?? throw new NotFoundException(_localizer["User Not Found."]);

            bool isAdmin = await _userManager.IsInRoleAsync(user, AppRoles.Admin);
            if (isAdmin)
            {
                throw new ConflictException(_localizer["Administrators Profile's Status cannot be toggled"]);
            }

            user.IsActive = request.ActivateUser;

            await _userManager.UpdateAsync(user);

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id));
        }

        private async Task<string> GetEmailVerificationUriAsync(ApplicationUser user, string origin)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            const string route = "api/users/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), QueryStringKeys.UserId, user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, QueryStringKeys.Code, code);
            //verificationUri = QueryHelpers.AddQueryString(verificationUri, MultitenancyConstants.TenantIdName, _currentTenant.Id!);
            return verificationUri;
        }

        public async Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == userId && !u.EmailConfirmed)
                .FirstOrDefaultAsync(cancellationToken);

            _ = user ?? throw new InternalServerException(_localizer["An error occurred while confirming E-Mail."]);

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return result.Succeeded
                ? string.Format(_localizer["Account Confirmed for E-Mail {0}. You can now use the /api/tokens endpoint to generate JWT."], user.Email)
                : throw new InternalServerException(string.Format(_localizer["An error occurred while confirming {0}"], user.Email));
        }

        public async Task<string> ConfirmPhoneNumberAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new InternalServerException(_localizer["An error occurred while confirming Mobile Phone."]);

            var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);

            return result.Succeeded
                ? user.EmailConfirmed
                    ? string.Format(_localizer["Account Confirmed for Phone Number {0}. You can now use the /api/tokens endpoint to generate JWT."], user.PhoneNumber)
                    : string.Format(_localizer["Account Confirmed for Phone Number {0}. You should confirm your E-mail before using the /api/tokens endpoint to generate JWT."], user.PhoneNumber)
                : throw new InternalServerException(string.Format(_localizer["An error occurred while confirming {0}"], user.PhoneNumber));
        }

        public async Task<string> CreateAsync(CreateUserRequest request, string origin)
        {
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new InternalServerException(_localizer["Validation Errors Occurred."], result.GetErrors(_localizer));
            }

            await _userManager.AddToRoleAsync(user, AppRoles.Basic);

            var messages = new List<string> { string.Format(_localizer["User {0} Registered."], user.UserName) };

            if (_securitySettings.RequireConfirmedAccount && !string.IsNullOrEmpty(user.Email))
            {
                // send verification email
                string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);
                RegisterUserEmailModel eMailModel = new RegisterUserEmailModel()
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    Url = emailVerificationUri
                };
                var mailRequest = new MailRequest(
                    new List<string> { user.Email },
                    _localizer["Confirm Registration"],
                    _templateService.GenerateEmailTemplate("email-confirmation", eMailModel));
                _jobService.Enqueue(() => _mailService.SendAsync(mailRequest));
                messages.Add(_localizer[$"Please check {user.Email} to verify your account!"]);
            }

            await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id));

            return string.Join(Environment.NewLine, messages);
        }

        public async Task UpdateAsync(UpdateUserRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new NotFoundException(_localizer["User Not Found."]);

            string currentImage = user.ImageUrl ?? string.Empty;
            if (request.Image != null || request.DeleteCurrentImage)
            {
                user.ImageUrl = await _fileStorage.UploadAsync<ApplicationUser>(request.Image, FileType.Image);
                if (request.DeleteCurrentImage && !string.IsNullOrEmpty(currentImage))
                {
                    string root = Directory.GetCurrentDirectory();
                    _fileStorage.Remove(Path.Combine(root, currentImage));
                }
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (request.PhoneNumber != phoneNumber)
            {
                await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            }

            var result = await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id));

            if (!result.Succeeded)
            {
                throw new InternalServerException(_localizer["Update profile failed"], result.GetErrors(_localizer));
            }
        }

        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Normalize());
            if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                throw new InternalServerException(_localizer["An Error has occurred!"]);
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            const string route = "account/reset-password";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            string passwordResetUrl = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);
            var mailRequest = new MailRequest(
                new List<string> { request.Email },
                _localizer["Reset Password"],
                _localizer[$"Your Password Reset Token is '{code}'. You can reset your password using the {endpointUri} Endpoint."]);
            _jobService.Enqueue(() => _mailService.SendAsync(mailRequest));

            return _localizer["Password Reset Mail has been sent to your authorized Email."];
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email?.Normalize());

            // Don't reveal that the user does not exist
            _ = user ?? throw new InternalServerException(_localizer["An Error has occurred!"]);

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

            return result.Succeeded
                ? _localizer["Password Reset Successful!"]
                : throw new InternalServerException(_localizer["An Error has occurred!"]);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new NotFoundException(_localizer["User Not Found."]);

            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

            if (!result.Succeeded)
            {
                throw new InternalServerException(_localizer["Change password failed"], result.GetErrors(_localizer));
            }
        }

        public async Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new NotFoundException(_localizer["User Not Found."]);

            var userRoles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            foreach (var role in await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name))
                .ToListAsync(cancellationToken))
            {
                permissions.AddRange(await _dbContext.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == AppClaims.Permission)
                    .Select(rc => rc.ClaimValue)
                    .ToListAsync(cancellationToken));
            }

            return permissions.Distinct().ToList();
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken)
        {
            var permissions = await _cache.GetOrSetAsync(
                _cacheKeys.GetCacheKey(AppClaims.Permission, userId),
                () => GetPermissionsAsync(userId, cancellationToken),
                cancellationToken: cancellationToken);

            return permissions?.Contains(permission) ?? false;
        }

        public Task InvalidatePermissionCacheAsync(string userId, CancellationToken cancellationToken) =>
            _cache.RemoveAsync(_cacheKeys.GetCacheKey(AppClaims.Permission, userId), cancellationToken);

        public async Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken)
        {
            var userRoles = new List<UserRoleDto>();

            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);
            foreach (var role in roles)
            {
                userRoles.Add(new UserRoleDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Description = role.Description,
                    Enabled = await _userManager.IsInRoleAsync(user, role.Name)
                });
            }

            return userRoles;
        }

        public async Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);

            _ = user ?? throw new NotFoundException(_localizer["User Not Found."]);

            // Check if the user is an admin for which the admin role is getting disabled
            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin)
                && request.UserRoles.Any(a => !a.Enabled && a.RoleName == AppRoles.Admin))
            {
                // Get count of users in Admin Role
                int adminCount = (await _userManager.GetUsersInRoleAsync(AppRoles.Admin)).Count;

                // Check if user is not Root Tenant Admin
                // Edge Case : there are chances for other tenants to have users with the same email as that of Root Tenant Admin. Probably can add a check while User Registration
                if (user.Email == AppUsers.AdminEmail)
                {
                    throw new ConflictException(_localizer["Cannot Remove Admin Role From Root Tenant Admin."]);
                }
                else if (adminCount <= 1)
                {
                    throw new ConflictException(_localizer["Tenant should have at least 1 Admins."]);
                }
            }

            foreach (var userRole in request.UserRoles)
            {
                // Check if Role Exists
                if (await _roleManager.FindByNameAsync(userRole.RoleName) is not null)
                {
                    if (userRole.Enabled)
                    {
                        if (!await _userManager.IsInRoleAsync(user, userRole.RoleName))
                        {
                            await _userManager.AddToRoleAsync(user, userRole.RoleName);
                        }
                    }
                    else
                    {
                        await _userManager.RemoveFromRoleAsync(user, userRole.RoleName);
                    }
                }
            }

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, true));

            return _localizer["User Roles Updated Successfully."];
        }
    }
}
