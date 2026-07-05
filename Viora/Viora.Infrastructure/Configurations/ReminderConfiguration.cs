using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Appointments;
using Viora.Domain.Reminders;

namespace Viora.Infrastructure.Configurations;

internal class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("Reminders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppointmentId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasConversion(
            v => v.Value,
            v => new Domain.Reminders.Internal.TItle(v)
            )
            .HasMaxLength(100);

        builder.Property(x => x.Body)
            .IsRequired(false)
            .HasConversion(
            v => v.Value,
            v => new Domain.Reminders.Internal.Body(v)
            )
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.ScheduledFor).IsRequired();

        builder.HasOne<Appointment>()
            .WithOne()
            .HasForeignKey<Reminder>(x => x.AppointmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AppointmentId).IsUnique();

    }
}
