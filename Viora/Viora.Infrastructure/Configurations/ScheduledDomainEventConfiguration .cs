using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Scheduling;

namespace Viora.Infrastructure.Configurations;

internal class ScheduledDomainEventConfiguration
    : IEntityTypeConfiguration<ScheduledDomainEvent>
{
    public void Configure(EntityTypeBuilder<ScheduledDomainEvent> b)
    {
        b.ToTable("ScheduledDomainEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).HasMaxLength(500).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.ScheduledFor).IsRequired();
        b.Property(x => x.AttemptCount);
        b.Property(x => x.Error).HasMaxLength(2000);

        b.HasIndex(x => new { x.ProcessedOn, x.ScheduledFor })
            .HasFilter("[ProcessedOn] IS NULL");
    }
}
