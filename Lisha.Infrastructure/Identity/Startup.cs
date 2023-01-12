using Lisha.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;

namespace Lisha.Infrastructure.Identity
{
    internal static class Startup
    {
        internal static IServiceCollection AddIdentity(this IServiceCollection services) =>
            services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .Services;
    }
}
