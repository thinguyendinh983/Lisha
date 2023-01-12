using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lisha.Application
{
    public static class Startup
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddAutoMapper(assembly);
            services.AddValidatorsFromAssembly(assembly);
            services.AddMediatR(assembly);

            return services;
        }
    }
}
