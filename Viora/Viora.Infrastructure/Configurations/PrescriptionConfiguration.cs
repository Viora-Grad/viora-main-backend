using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Appointments;
using Viora.Domain.Prescriptions;

namespace Viora.Infrastructure.Configurations;

internal class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppointmentId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.AppointmentId)
            .IsUnique();

        builder.HasIndex(x => x.AppointmentId);


        builder.HasOne<Appointment>()
            .WithOne()
            .HasForeignKey<Prescription>(x => x.AppointmentId);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
