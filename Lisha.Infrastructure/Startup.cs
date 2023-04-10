using Hangfire;
using Lisha.Infrastructure.Auth;
using Lisha.Infrastructure.BackgroundJobs;
using Lisha.Infrastructure.Caching;
using Lisha.Infrastructure.Common;
using Lisha.Infrastructure.Cors;
using Lisha.Infrastructure.FileStorage;
using Lisha.Infrastructure.HealthCheck;
using Lisha.Infrastructure.Localization;
using Lisha.Infrastructure.Mailing;
using Lisha.Infrastructure.Middleware;
using Lisha.Infrastructure.Notifications;
using Lisha.Infrastructure.OpenApi;
using Lisha.Infrastructure.Persistence;
using Lisha.Infrastructure.Persistence.Initialization;
using Lisha.Infrastructure.SecurityHeaders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Lisha.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return services
                .AddApiVersioning()
                .AddAutoMapper(assembly)
                .AddAuth(config)
                .AddBackgroundJobs(config)
                .AddCaching(config)
                .AddCorsPolicy(config)
                .AddExceptionMiddleware()
                .AddHealthCheck()
                .AddLocalization(config)
                .AddMailing(config)
                .AddMediatR(cfg=>cfg.RegisterServicesFromAssembly(assembly))
                .AddNotifications(config)
                .AddOpenApiDocumentation(config)
                .AddPersistence(config)
                .AddRequestLogging(config)
                .AddRouting(options => options.LowercaseUrls = true)
                .AddServices();
        }

        private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

        private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
            services.AddHealthChecks().AddCheck<AppHealthCheck>("Lisha").Services;

        public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            // Create a new scope to retrieve scoped services
            using var scope = services.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
                .InitializeDatabasesAsync(cancellationToken);
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
            builder
                .UseLocalization(config)
                .UseStaticFiles()
                .UseSecurityHeaders(config)
                .UseFileStorage()
                .UseExceptionMiddleware()
                .UseRouting()
                .UseCorsPolicy()
                .UseAuthentication()
                .UseCurrentUser()
                .UseAuthorization()
                .UseRequestLogging(config)
                .UseHangfireDashboard(config)
                .UseOpenApiDocumentation(config);

        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapControllers().RequireAuthorization();
            builder.MapHealthCheck();
            builder.MapNotifications();
            return builder;
        }

        private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
            endpoints.MapHealthChecks("/api/health").RequireAuthorization();
    }
}
