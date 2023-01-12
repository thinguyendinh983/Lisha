using Lisha.Application.Common.Interfaces;
using Lisha.Infrastructure.Auth.Jwt;
using Lisha.Infrastructure.Auth.Permissions;
using Lisha.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Lisha.Infrastructure.Auth
{
    internal static class Startup
    {
        internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddCurrentUser()
                .AddPermissions()

                // Must add identity before adding auth!
                .AddIdentity();

            services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));

            return config["SecuritySettings:Provider"].Equals("Jwt", StringComparison.OrdinalIgnoreCase)
                ? services.AddJwtAuth(config)
                : throw new InvalidOperationException($"Security Provider is not supported.");
        }

        internal static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app) =>
            app.UseMiddleware<CurrentUserMiddleware>();

        private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
            services
                .AddScoped<CurrentUserMiddleware>()
                .AddScoped<ICurrentUser, CurrentUser>()
                .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

        private static IServiceCollection AddPermissions(this IServiceCollection services) =>
            services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }
}
