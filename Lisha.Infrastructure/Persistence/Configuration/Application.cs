using Lisha.Domain.Entities;
using Lisha.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisha.Infrastructure.Persistence.Configuration
{
    public class BranchInformationConfig : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder) =>
            builder
                .ToTable("Branch", SchemaNames.Application);
    }

    public class TrailConfig : IEntityTypeConfiguration<Trail>
    {
        public void Configure(EntityTypeBuilder<Trail> builder) =>
            builder
                .ToTable("AuditTrails", SchemaNames.Application);
    }
}
