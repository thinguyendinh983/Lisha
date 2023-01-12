using Lisha.Application.Common.Events;
using Lisha.Application.Common.Interfaces;
using Lisha.Domain.Entities;
using Lisha.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Lisha.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : BaseDbContext
    {
        public ApplicationDbContext(DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events, IDateTime dateTime)
            : base(options, currentUser, serializer, dbSettings, events, dateTime)
        {
        }

        public DbSet<Branch> Branchs => Set<Branch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(SchemaNames.Application);
        }
    }
}
