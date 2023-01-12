using Lisha.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lisha.Infrastructure.Persistence.Initialization
{
    internal class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DatabaseSettings _dbSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(ApplicationDbContext dbContext, IOptions<DatabaseSettings> dbSettings, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            _dbContext = dbContext;
            _dbSettings = dbSettings.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                _logger.LogInformation("Applying Migrations.");
                await _dbContext.Database.MigrateAsync(cancellationToken);
            }

            // First create a new scope
            using var scope = _serviceProvider.CreateScope();

            // Then set current tenant so the right connectionstring is used
            _serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Then run the initialization in the new scope
            await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
                .InitializeAsync(cancellationToken);
        }
    }
}
