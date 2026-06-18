using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Appointments;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.RealTimeScheduling.Internals;

namespace Viora.Infrastructure.Configurations;

internal class ScheduleDelayConfiguration : IEntityTypeConfiguration<ScheduleDelay>
{
    public void Configure(EntityTypeBuilder<ScheduleDelay> builder)
    {
        builder.ToTable("ScheduleDelays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppointmentId)
            .IsRequired();

        builder.Property(x => x.DelayDuration)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.OccurrenceTime)
            .IsRequired();

        builder.Property(x => x.Initiator)
             .HasConversion(
                x => x.Value,
                value => InitiatorType.FromValue(value)
             )
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AppointmentId);

        builder.HasIndex(x => x.OccurrenceTime);
    }
}
