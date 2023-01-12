using Lisha.Domain.Entities;
using Lisha.Infrastructure.Identity;
using Lisha.Infrastructure.Persistence.Context;
using Lisha.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lisha.Infrastructure.Persistence.Initialization
{
    internal class ApplicationDbSeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CustomSeederRunner _seederRunner;
        private readonly ILogger<ApplicationDbSeeder> _logger;

        public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _seederRunner = seederRunner;
            _logger = logger;
        }

        public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            await SeedRolesAsync(dbContext);
            await SeedAdminUserAsync();
            await SeedDefaultDataAsync(dbContext);
            await _seederRunner.RunSeedersAsync(cancellationToken);
        }

        private async Task SeedRolesAsync(ApplicationDbContext dbContext)
        {
            foreach (string roleName in AppRoles.DefaultRoles)
            {
                if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                    is not ApplicationRole role)
                {
                    // Create the role
                    _logger.LogInformation("Seeding {role} Role.", roleName);
                    role = new ApplicationRole(roleName, $"{roleName} Role");
                    await _roleManager.CreateAsync(role);
                }

                // Assign permissions
                if (roleName == AppRoles.Basic)
                {
                    await AssignPermissionsToRoleAsync(dbContext, AppPermissions.Basic, role);
                }
                else if (roleName == AppRoles.Admin)
                {
                    await AssignPermissionsToRoleAsync(dbContext, AppPermissions.All, role); // Nen dat AppPermissions.Admin
                }
            }
        }

        private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, IReadOnlyList<AppPermission> permissions, ApplicationRole role)
        {
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var permission in permissions)
            {
                if (!currentClaims.Any(c => c.Type == AppClaims.Permission && c.Value == permission.Name))
                {
                    _logger.LogInformation("Seeding {role} Permission '{permission}'.", role.Name, permission.Name);
                    dbContext.RoleClaims.Add(new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = AppClaims.Permission,
                        ClaimValue = permission.Name,
                        CreatedBy = "ApplicationDbSeeder"
                    });
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            if (string.IsNullOrWhiteSpace(AppUsers.AdminEmail))
            {
                return;
            }

            if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == AppUsers.AdminEmail)
                is not ApplicationUser adminUser)
            {
                string adminUserName = AppUsers.AdminUserName.ToLowerInvariant();
                adminUser = new ApplicationUser
                {
                    FirstName = AppUsers.AdminUserName,
                    LastName = AppUsers.AdminUserName,
                    Email = AppUsers.AdminEmail,
                    UserName = adminUserName,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    NormalizedEmail = AppUsers.AdminEmail?.ToUpperInvariant(),
                    NormalizedUserName = adminUserName.ToUpperInvariant(),
                    IsActive = true
                };

                _logger.LogInformation("Seeding Default Admin User.");
                var password = new PasswordHasher<ApplicationUser>();
                adminUser.PasswordHash = password.HashPassword(adminUser, AppUsers.DefaultPassword);
                await _userManager.CreateAsync(adminUser);
            }

            // Assign role to user
            if (!await _userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
            {
                _logger.LogInformation("Assigning Admin Role to Admin User.");
                await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }
        }

        private async Task SeedDefaultDataAsync(ApplicationDbContext dbContext)
        {
            if (!dbContext.Branchs.Any())
            {
                dbContext.Branchs.Add(new Branch
                {
                    Code = "395",
                    Name = "Bac Kan",
                    Address = "So 57, Truong Chinh, TP Bac Kan, Tinh Bac Kan",
                    Phone = "02093872382",
                    Email = "baccan@bidv.com.vn"
                });

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
